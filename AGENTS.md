# AGENTS.md — The Iron Magnate

Instructions for AI coding agents contributing to this repository. Read this file, the root [README](README.md), and the linked [Linear v0 domain contracts](https://linear.app/josh-soll/document/v0-factory-core-architecture-and-domain-contracts-1b0c7112f109) before making changes.

## Project status and intent

The Iron Magnate is a **desktop-first, single-player Victorian/steampunk industrial survival tycoon**, built to learn **C#/.NET 10** through a deterministic, testable factory simulation. Godot C# is the planned eventual desktop presentation; the 2D/3D camera and art direction are not implementation requirements yet.

- The former ASP.NET/warehouse-telemetry prototype has **already been removed**. The repository now contains a plain .NET core library, a simulation clock, a basic `FactorySimulation` coordinator, a `CoalBoiler`, and xUnit tests. **Do not reintroduce the former API stack.**
- **Conceptual v0 domain behavior is agreed.** Concrete C# contracts are defined incrementally while implementing tested, end-to-end vertical slices, not by generating a speculative domain hierarchy.
- **Do not build Godot UI, visuals, game menus, or an art pipeline until the headless factory simulation and its player-journey tests work.**
- The [Linear project](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455) is the planning tracker. The [master architecture board](https://www.tldraw.com/f/Nbuou6MsW99Cefb9__URf) records conceptual decisions. Future ideas are **not** authorization to implement them.
- Implement **only the explicitly assigned slice**. Read existing code and tests first; preserve working foundations.

## Core architecture and boundaries

The authoritative simulation lives in `src/IronMagnate.Core`, **independent of Godot, UI, file storage implementation, HTTP, databases, networking, and real-world timers**.

- **Simulation / coordination:** controls game-time advancement, stable ordering of resource requests, day lifecycle, and cross-domain operations. It should **coordinate** rather than absorb every domain's business rules.
- **Supply / inventory:** owns raw materials, manufactured parts, available quantities, total storage, output reservations, and consumption/production inventory changes.
- **Energy:** owns fuel, sources, power availability, and powered-time calculation; machinery must not directly manipulate boiler fuel.
- **Workforce:** owns hired workers, idle pool, assignments, and crew eligibility; Economy owns the resulting payables.
- **Machinery / production:** owns equipment eligibility, selected known recipe, workpiece progress/state, and completed outputs. An assembly station is a type of production equipment; assembly is **optional**.
- **Economy / Treasury:** owns Crowns, immediate purchase/sale cash changes, outstanding obligations, manual debt payments, deadlines, and bankruptcy evaluation.
- **Persistence:** restores a complete, consistent versioned snapshot of authoritative domain state; the UI later derives its presentation from restored state.

Keep immutable **schematic/item/equipment definitions** separate from mutable instances where useful. Avoid speculative inheritance trees, distributed services, event buses, unnecessary service layers, and premature interfaces. Define a contract only when the assigned implementation slice needs it.

## Agreed v0 gameplay invariants

### Time and coordination
- Shift **08:00–16:00**; at 1×, one in-game minute equals one real second. Support pause / 1× / 2× / 4×. Stop advancing at 16:00, expose an end-of-day settlement state, and require explicit **Begin Next Day** to continue at 08:00/1×. No overnight or offline progression.
- The clock returns **actual elapsed simulated minutes**, including when capped at 16:00. Energy determines **actual powered minutes**; production must never receive more operating time than it has power for.
- Stable, deterministic machine request order is adequate in v0. Never allow duplicated material allocations, workers, or output reservations.

### Starter factory, materials and machines
- Intended starter scenario: **one worn central coal boiler, one basic production machine, one tiny assembly station**, a small interchangeable workforce, simple known schematics, starter raw stock/fuel, central supply capacity, and enough starting Crowns for a feasible first manufacturing-and-sale loop. No starting drones.
- **Production is an item transformation:** raw goods → machine → optional assembly → finished item; equipment may directly manufacture a sellable item. An assembly may use existing inventory parts as inputs. Each product/item is storable and can be used in a later recipe.
- A schematic specifies input/output items and quantities, processing duration, and compatible equipment; equipment also has crew/energy requirements. The active known recipe runs **continuously** while eligible, not a mandatory finite-quantity production order.
- Before starting each workpiece, validate all required eligibility conditions, **commit inputs once**, and reserve the full output capacity. On completion, convert the output reservation into occupied inventory. If start fails, do not consume materials or leave orphan reservations.
- **Every physical inventory unit occupies exactly one central storage slot**, regardless of type or size. `available capacity = capacity - occupied - reserved`. Output reservations are exclusive and persist with unfinished workpieces; storage can be expanded with simple purchased capacity.
- Pausing, boiler shutdown/fuel shortage, end-of-day, or turning equipment off preserves committed inputs, workpiece progress, and its output reservation. **Explicit cancellation** discards the unfinished work and committed inputs (no scrap refund in v0) and releases its reservation. Blocked machines cannot start new work.
- Vendors have **unlimited instant availability** of catalog-listed raw materials and coal in v0. A manual purchase succeeds only when the player can pay and the receiving destination has sufficient capacity; inventory/fuel and Crowns must change together or not at all. Manual sales remove stored items and credit Crowns atomically.

### Energy
- Two conceptual machine energy requirements: **(A) connected shared power** and **(B) individual local power**. The latter consumes its own finite fuel at a fixed rate per simulated minute. The existing starter boiler supports shared connected production.
- For shared supply with inadequate generation capacity, preserve connection order and shed **last-connected consumers first**; restore eligibility in original order when possible. **Low coal reserves shorten powered duration, not generation capacity**; coal exhaustion shuts off the source and all dependent equipment.
- Boiler burns coal when **ON**, including when production is idle or blocked; **OFF** consumes none. No physical pipes, steam-pressure dynamics, random brownouts, or configurable power priorities in v0.

### Workforce
- Hiring adds a worker to the available pool. Machines can automatically obtain **complete minimum crews** from idle workers; players may also manually assign **idle** workers. Never partially allocate an insufficient crew or preempt staff from a productive machine.
- Stable assignments stay with productive machines across consecutive workpieces. Blocked or disabled machines release their crew. If a machine becomes eligible again, it waits for free workers rather than reclaiming them from another active machine.
- All hired workers receive a simple fixed **full-day wage for any day in which they were employed**, even if idle or dismissed before 16:00. Dismissal reduces **future** payroll, not already-earned obligations. Worker skill, morale, trust, strikes, housing, and career progression are future systems.

### Treasury, obligations and failure
- Crown balances must never go negative through purchases/payments. Manual purchases charge immediately; manual sales earn immediately. Financial reports show actual cash movement, not mislabeled accounting profit.
- At 16:00, **accrue payroll exactly once** for that day; **do not automatically deduct wages**. Each day's payroll is a distinct obligation with its own balance and final deadline.
- Player can select a specific obligation and make **full or partial manual payments**. The payment reduces only that obligation and Treasury together. Partial repayment **does not extend/reset the original deadline**; payables from different days (or future loan installments) never overwrite one another.
- Show outstanding amount and time remaining. **At a final 16:00 deadline, allow settlement actions before evaluating failure**. An unpaid critical balance after that opportunity causes company closure/game over; having **zero Crowns alone is not game over**. Recovery may include selling inventory or liquidating assets.
- Loans, installment interest, grace lengths, equipment/raw material costs, selling prices, and profitable starter parameters belong to the later **Budget/Treasury balancing pass**. If loans are added, use independently tracked obligations; do not invent complex credit systems now.

### Persistence and ledger
- Save & Quit is allowed at **any stable simulation point**, including during production, **at 16:00 with the ledger open**, and after partial debt repayment.
- Load restores **the same day, minute, paused/settlement lifecycle, available cash, debts and original deadlines, staff/assignments, inventory/reservations, energy and fuel, current workpiece progress, and daily report**. A pending 16:00 ledger must appear again in the eventual UI based on restored simulation state.
- Never double-accrue payroll, double-pay debts, consume input twice, duplicate outputs, or advance time when restoring. Capture snapshots only between completed domain operations, not halfway through a purchase or inventory transfer.
- Design versioned snapshots, validation, and safe save-file replacement when implementing persistence. No silent reset after corrupt/incompatible saves.

## Implementation approach: working vertical slices

1. Inspect current core code and tests (`SimulationClock`, `FactorySimulation`, `CoalBoiler`). Keep the existing .NET foundations.
2. Implement the **smallest production slice** across only the needed domains: available materials and output reservation + power and complete crew + machine workpiece → one finished iron plate in central supply.
3. Extend into assembly through shared inventory, manual procurement/sales, day-end payroll and obligations, and exact persistence **incrementally**, with focused tests each time.
4. Validate multi-day factory scenarios, recovery and bankruptcy, actual powered-time production, and save/load during a pending 16:00 settlement **before any Godot work**.
5. Balance costs, prices, starting capital, production rates, wages, and deadlines together with the Budget/Treasury work. Do not hard-code unapproved economic balance as product decisions.

Never interpret these ordered slices as a request to build the entire simulation in one PR.

## Testing and correctness

- Add **xUnit** tests for each new rule and integration boundary. Test state invariants, time caps/speed behavior, over-capacity/insufficient materials, exclusive storage reservations, insufficient staffing, fuel exhaustion, workpiece interruptions, purchase/sale atomicity, debt deadlines, and partial payments.
- Maintain deterministic **headless player-journey tests** that play across multiple workdays: manufacture → store → sell → accrue wage debt → pay or default → save at the 16:00 ledger → reload and see identical state → continue.
- A save/load round trip must preserve every authoritative state field and not repeat side effects. Validate boundary cases around exactly 16:00 and the final obligation deadline.
- Run the relevant checks for code changes. Unless the repository changes, the baseline commands are:

  ```sh
  dotnet restore TheIronMagnate.sln
  dotnet build TheIronMagnate.sln --no-restore
  dotnet test TheIronMagnate.sln --no-build
  ```

- Report **actual** commands and results. Do not claim builds/tests passed unless they were run.

## Out of scope until explicitly authorized

Godot UI, 2D/3D assets, ornate HUD, music, physical steam pipes and logistics, item-specific storage volumes, physical machine construction and wear, worker skills/social systems, random incidents, market supply/demand, named suppliers and bulk discounts, research/patents, drones, production priority controls, and complex financing are **future ideas**. Do not implement them as prerequisites.

## Repository and PR discipline

1. Inspect relevant code, tests, and recent instructions before editing.
2. Keep PRs small and coherent: one assigned slice, no unrelated refactors, no legacy cleanup already completed, and no placeholder abstractions.
3. Prefer built-in .NET capabilities. Do not introduce PostgreSQL, EF Core, web APIs, Redis, or cloud systems just because the previous prototype had them.
4. Use focused branches and PRs; **do not merge your own PR**.
5. Summarize changed behavior, tests performed, residual risks, and why key C# design choices were made. The purpose is also to learn C# architecture.
6. Mark a milestone complete only when the intended behavior is demonstrated and tested. When something is undecided, preserve v0 scope and call it out rather than silently inventing a new system.

**Guiding principle: Build the factory first. Build the empire later.**
