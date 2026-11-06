# AGENTS.md — The Iron Magnate

Instructions for AI coding agents contributing to this repository. Read this file and the root README before making changes.

## Product and scope

The Iron Magnate is a **desktop-first, single-player Victorian/steampunk industrial tycoon** and a project for learning **C#/.NET** through a real, testable simulation. The planned presentation is **Godot 4 with C# and a 2D isometric-style view**.

- The **README** describes the product vision and current repository status.
- The [Linear project](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455) tracks the agreed v0 scope and implementation work.
- The repository currently contains a **legacy ASP.NET Core warehouse telemetry API**, not a completed game. Do not mistake existing implementation for the target architecture.
- Implement **only the assigned slice**. Future ideas in planning documents are **not authorization to build them**.

## Architectural boundaries

- Keep the simulation's authoritative rules and mutable state in a **plain C#/.NET core library**, independent of Godot, rendering, the filesystem, HTTP, database frameworks, and GUI-specific types.
- Godot translates user actions into domain commands and renders the resulting state. Do not duplicate economic or simulation rules in scenes/UI scripts.
- Keep game-time advancement explicit and controlled; do not use wall-clock time as a source of gameplay progress.
- Make state transitions and resource consumption testable without launching Godot.
- Prefer small, clear domain models over speculative inheritance trees, plugin frameworks, distributed services, or elaborate event buses.
- Local snapshot persistence is the planned v0 persistence mechanism. **Do not introduce PostgreSQL, EF Core, web APIs, cloud services, or networking merely because the old prototype used them.**
- Maintain a separation between **immutable definitions** (e.g., product or equipment specs) and **mutable instances** (e.g., inventory, progress, status) when needed; do not build abstractions prematurely.

## v0 simulation rules to preserve

- The workday runs **08:00–16:00**. At **1×**, one in-game minute is one real-world second; support **pause, 1×, 2×, and 4×**.
- Pausing stops simulation activity. Inspection and management actions remain possible. At 16:00, display the daily summary; **Begin Next Day** advances to 08:00, with **no overnight events in v0**.
- Start with **one installed production line and one simple part**. A production operation converts **one input unit into one finished unit**, with processing time and preserved partial progress.
- Support a finite quantity target or continuous production; wait when required input, workers, power, or finished-goods storage is unavailable. Do not lose or duplicate workpieces.
- Provide manual purchases of material and coal, capped finished-goods storage, and immediate selling for **Crowns (Cr.)**.
- One basic coal boiler consumes fuel whenever **ON**, including during idle or blocked production; **OFF** consumes no fuel.
- Require basic staffing; pay workers a **fixed wage at the end of the workday**, even when idle. Keep payroll and roles simple.
- Track cash and transaction history sufficient for the end-day report. **Cash movement is not necessarily accounting profit**; label summaries accurately.
- **Complete save snapshots** capture all authoritative state: day/time, progress, workers, assignments, orders, inventory, finances, equipment, fuel, and lifecycle. Loading must restore the exact game-state instant, with no offline progress; open paused for safe inspection.
- Exclude random incidents, deterioration, repairs, construction, multi-line production, complex recipes, research, advisors, reputation, intricate visuals, and advanced finance from v0 unless explicitly scoped later.

## Repository change discipline

1. Inspect relevant files, current structure, and recent project instructions before editing. If a task changes architecture, identify references and tests first.
2. For legacy cleanup, **audit before deletion**. Remove seeding, migrations, controllers, database setup, Docker files, and stale dependencies only when their lack of relevance is confirmed; preserve any reusable code/tests and keep the resulting build sound.
3. Make the smallest coherent change. Do not mix cleanup, simulation features, asset work, and unrelated refactors in one PR.
4. Prefer standard .NET and Godot capabilities; introduce packages only for clear, documented needs.
5. Avoid placeholder scaffolding, empty service layers, speculative interfaces, and unnecessary configuration.
6. Respect existing project conventions unless the assigned task is to change them. Do not claim setup commands work until verified.

## Tests, saves, and correctness

- Add/adjust automated tests for new rules or changed behavior. Cover normal and boundary cases: pause/speed, 16:00 transition, unavailable inputs, full storage, fuel exhaustion, wage deduction, and partially completed work.
- Keep simulation results consistent across speed settings. Speed changes how quickly in-game time advances, **not gameplay rules**.
- Treat a save/load round trip as a correctness invariant. Do not double-consume inventory/fuel, duplicate goods or wages, or advance the clock during loading.
- Version save formats and validate data when persistence is implemented. Use safe file-write practices; handle invalid saves without silently resetting player progress.
- Run the checks applicable to the change (restore, build, tests, and Godot checks when available); report exact commands and results. If validation cannot run, explicitly say why.

## Visuals and assets

- For the first Godot milestone, prefer functional placeholder art and accessible, readable controls.
- Keep Victorian ornaments, sprites, sounds, voice, advisor portraits, and event cards separate from simulation correctness. Do not add a Blender/3D workflow to v0.
- Do not add unlicensed third-party artwork, fonts, music, or voice assets. Record provenance/licenses for imported assets.

## Agent delivery / pull requests

- Use a focused branch and a small PR for each task. **Do not merge your own PR**.
- Summarize: purpose, key changes, tests/checks performed, and remaining risks or follow-ups.
- For significant C#/.NET decisions, include a **brief educational explanation** of why the approach was chosen and any important trade-off. The goal is to learn, not just generate code.
- Do not mark a milestone complete until its behavior is demonstrated and tested.
- When uncertain about a design choice, prefer the simplest solution consistent with v0 and flag the choice in the PR rather than quietly expanding scope.

**Guiding principle: Build the factory first. Build the empire later.**
