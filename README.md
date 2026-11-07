<p align="center">
  <img src="assets/readme-banner.svg" alt="The Iron Magnate — Victorian ornamental title banner" width="100%" />
</p>

<h1 align="center">The Iron Magnate</h1>
<p align="center"><em>Fortune is forged.</em></p>
<p align="center"><strong>Desktop-first, single-player steampunk industrial survival tycoon.</strong></p>

> **Status: v0 conceptual architecture agreed; domain contracts and implementation next.** This is **not yet a playable Godot game**. The C# simulation clock, factory lifecycle, and coal boiler foundations already exist. See the [Linear project](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455) and [master architecture board](https://www.tldraw.com/f/Nbuou6MsW99Cefb9__URf).

## Vision and design principles

Build a struggling fictional 19th-century-inspired workshop into an industrial empire. Start by manufacturing and selling humble components; much later, engineering research, specialized equipment, and patented inventions could enable advanced devices. Drones are **not** known or available at the start.

- **Hard but fair:** cash shortages, overexpansion, missed obligations, and bankruptcy are real threats. A starting factory must nevertheless have a feasible path to survival.
- **Automate routine work:** the player chooses production, purchases, sales, investment, and financial settlements; the factory automatically operates eligible machines and allocates available workers.
- **Architecture first:** approve the end-to-end lifecycle, then domain responsibilities and contracts, then incremental code. Avoid premature machine hierarchies, social simulation, and elaborate UI.
- **Small v0, extensible future:** no need to build the later factory empire before the first part can be sold.

## v0 — the first operational factory

The intended starting scenario provides one worn coal boiler, **one basic production machine and one tiny assembly station**, a handful of interchangeable general workers, simple known schematics, limited fuel/materials, expandable low-end storage, and enough working Crowns (Cr.) to have a viable first day. Exact quantities and prices are **not yet balanced**.

| Domain | Agreed v0 behavior |
| --- | --- |
| **Simulation / time** | 08:00–16:00; 1 simulated minute per real second at 1×; pause / 1× / 2× / 4×. At 16:00 show a blocking report; Begin Next Day resumes at 08:00 and 1×. No overnight or offline production. |
| **Materials & central supply** | Manually buy raw inputs and coal. Raw stock and finished components share a conceptual central supply location; automatic transfers, no hauling. Capacity can be expanded by purchasing cheap storage. |
| **Energy** | One coal boiler; burns coal whenever ON, even with idle or blocked machines. A machine can work only for the actual powered portion of elapsed simulated time. Assume direct boiler connection in v0; alternative local fuels, steam pipe networks, and cleaner energy belong later. |
| **Workforce** | Finite pool of interchangeable general workers. Eligible machines request their **complete minimum crew**; assign only available workers automatically. Keep productive assignments stable, never steal workers, and release crews when a machine is blocked/disabled. No worker priority system. |
| **Machinery & production** | A machine has a selected known part/recipe and operates continuously while eligible. It needs compatible equipment, materials, power, complete staffing, and reserved output space. A small assembly station may consume existing parts to produce another basic component. No automatic recipe fallback. |
| **Inventory reservations** | Before starting each workpiece, check and commit inputs and reserve enough central output capacity. Competing machines cannot claim the same stock, staff, or space; deterministic request order is sufficient for v0. Full storage blocks *new* work, with a visible reason. |
| **Work in progress** | A pause, manual power-off, fuel loss, or end-of-day preserves the current workpiece, its input commitment, progress, and reserved output capacity. An **explicit cancellation** discards committed inputs/work and releases the reservation; no salvage in v0. |
| **Sales & economy** | Finished parts enter central supply, where the player sells them **manually** for immediate Crowns. Purchasing inputs/fuel costs cash immediately. Selling inventory or liquidating unused machinery can provide emergency liquidity. |
| **Payroll & obligations** | At 16:00 calculate/accrue wages **once**; actual payment is **manual**. Outstanding wages carry a clear due date and finite grace period. If loans are later included, installments are also manually paid and have their own deadlines/interest terms. Exact amounts and deadlines await balancing. |
| **Failure** | Zero cash does not equal game over. The player can attempt recovery via sales or asset liquidation. Missing a **final critical payment deadline** causes company closure/game over in v0; no revolts, morale, or strikes yet. |
| **Reports** | Compact live cash/operating snapshot plus a final 16:00 ledger: cash, sales, purchases, outstanding/projected wages, net **cash change** (not accounting profit), production, workforce, storage, and warnings. |
| **Persistence** | Versioned local save/load of exact factory state: day/time/speed, funds and obligations/deadlines, inventories and reservations, boiler status/fuel, machine recipes and partial work, staffing, and daily report. Resume without offline progress. |
| **Presentation** | Minimal desktop Godot C# factory with placeholders, HUD, clock/speed controls, operating indicators, supply/sales actions, and end-day ledger. Presentation is separate from the authoritative C# simulation. |

### End-to-end operating loop

1. Begin at 08:00 with the installed starter equipment, staff, supply stock, and treasury.
2. Buy raw materials and coal; choose a known product for each enabled machine.
3. The factory checks **materials + power + available complete crew + output capacity** before admitting new work.
4. Commit one workpiece's inputs and reserve its eventual output space; process using actual powered simulated time.
5. On completion, convert the output reservation into central inventory, then repeat. On shortage or stoppage, preserve existing work, expose the reason, and release idle crews for other eligible work.
6. The player manually sells components, buys supplies, and reinvests; assembly draws eligible components from the same central inventory.
7. At **16:00**, stop, accrue wages once, display the ledger and payment deadlines. Player may pay outstanding obligations when cash is available. Begin Next Day explicitly.
8. Save, quit, and restore the identical authoritative state at any point, including partially completed work and financial crises.

A machine becoming eligible again **does not recall workers** currently operating elsewhere. Reservations belong to unfinished workpieces—not to machines merely being switched on.

### Financial viability and difficulty

- The starter factory must be capable of a first sale and at least one sustainable baseline strategy under reasonable play; difficulty is intentional, mathematically impossible starts are bugs.
- Costs, sales prices, manufacturing rates, storage prices, payroll, loan interest, and grace-period lengths will be designed **together with the Budget/Treasury domain** and validated with deterministic and multi-scenario simulations.
- At an unpaid-obligation deadline, bankruptcy is possible even if the factory owns valuable equipment. A 0-Cr. balance alone is not failure.
- No unlimited free labor: unpaid wages accrue, manual settlement is required, and the unpaid critical obligation ultimately has a deadline.

## Architecture and implementation sequence

**Conceptual lifecycle (agreed) → Domain ownership and cross-domain contracts → Headless vertical slice → Economy/budget pricing and validation → Minimal Godot presentation.**

The simulation core coordinates **Materials/Supply, Energy, Workforce, Machinery/Production, Economy/Treasury, and Time/Persistence**. Each system owns its state; machines should not directly manage warehouse storage, payroll, or advisors.

The first integrated vertical slice should demonstrate: **purchase → reserve input/output → staff + power → process → store → sell → 16:00 ledger and obligation → next day → exact save/restore**.

## Beyond v0 — ideas, not first-release commitments

Multiple machinery archetypes (presses, furnaces, lathes, assembly lines), configurable production routes and physical hauling; advanced energy networks and cleaner sources with tradeoffs; engineer-led research, discoverable schematics, level/production prerequisites, patents and eventual drones; professional roles, worker experience/promotion, trust, housing, strikes, unrest, and industrial reputation; sophisticated production rules (produce/sell/keep), equipment condition, finance and loans; ornate Victorian art, advisors, audio, and potentially fixed-angle 3D factory presentation.

## Technology & development

- **C# / .NET 10:** Authoritative deterministic simulation; testable without Godot.
- **Godot with C#:** Future desktop presentation; visual style (2D isometric or angled 3D) can change without rewriting simulation rules.
- **Local saves:** Versioned state snapshots; the UI reconstructs itself from saved state.
- **Assets:** Placeholder-first; ornamental assets and animation later.

The warehouse-device API prototype was removed. Current C# foundations include the clock, factory lifecycle, coal boiler, and automated tests; **the complete production/economy loop is not yet implemented**.

Prerequisite: [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).

```sh
dotnet restore TheIronMagnate.sln
dotnet build TheIronMagnate.sln --no-restore
dotnet test TheIronMagnate.sln --no-build
```

See [Linear planning](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455) and the [master architecture](https://www.tldraw.com/f/Nbuou6MsW99Cefb9__URf) for live design decisions.

---

<p align="center"><em>Build the factory first. Build the empire later.</em></p>
