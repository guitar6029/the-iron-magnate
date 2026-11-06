<p align="center">
  <img src="assets/readme-banner.svg" alt="The Iron Magnate — Victorian ornamental title banner" width="100%" />
</p>

<h1 align="center">The Iron Magnate</h1>
<p align="center"><em>Fortune is forged.</em></p>
<p align="center"><strong>Planned desktop-first, single-player Victorian / steampunk industrial tycoon.</strong></p>

> **Status: v0 planning / architecture transition.** This repository currently contains an older ASP.NET Core warehouse-device API prototype. It is **not yet** a playable Godot game. The legacy API, database migrations, Docker tooling, and seeding will be audited before any removal. See the [v0 Linear project](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455).

## The vision

Begin with a humble industrial workshop and build toward an empire of mechanical devices and drones. Balance materials, coal, workers, inventory, and Crowns (Cr.) while your factory turns raw inputs into valuable goods. A small, working factory comes first; elaborate Victorian art, advisors, audio, lore, and sprawling manufacturing systems will follow gradually.

## v0 — The first playable factory

The first milestone intentionally centers on **one factory, one production line, and one part**.

| System | v0 behavior |
| --- | --- |
| **Clock** | 08:00–16:00 workday; 1 in-game minute per real second at 1×; pause / 1× / 2× / 4× |
| **Production** | 1 raw input → 1 finished part after processing; fixed target or continuous production |
| **Supplies** | Manually purchase materials and coal; production waits when input is missing |
| **Warehouse** | Limited finished-goods capacity; full storage blocks further production until space is freed |
| **Energy** | One coal boiler; fuel burns whenever it is **on**, even with an idle production line |
| **Workforce** | Hire and assign sufficient workers; fixed wages paid at the end of each workday |
| **Finances** | Buy supplies, sell finished parts immediately for **Crowns (Cr.)**, track cash and daily cash flow |
| **Day boundary** | At 16:00 show the daily report; **Begin Next Day** resumes at 08:00 |
| **Persistence** | Complete timestamped local snapshot: save, quit, load, and resume the exact factory state, with no offline progression |
| **Interface** | Minimal 2D isometric-style factory, resource HUD, selectable line controls, and a bottom-left simulation clock |

No equipment wear, breakdowns, construction, random incidents, research, multi-part recipes, or complex payroll for v0.

## Technology direction

- **C# / .NET:** The authoritative, testable simulation core, independent of any game engine.
- **Godot with C#:** Desktop-first 2D isometric-style factory scene, interactions, HUD, audio and animation.
- **Local saves:** Versioned snapshots of simulation state. The interface is rebuilt from restored data.
- **Assets:** Placeholder visuals first. Ornamental brass frames, corner filigree, advisors, machines, music, and sound effects later.

A 3D angled presentation or a marketing website might be explored in the future, without changing the core simulation rules.

## Development milestones

1. **Foundation & cleanup:** Audit the existing warehouse-telemetry prototype; establish a clean .NET core and Godot integration without blindly deleting reusable code.
2. **Headless factory:** Test the clock, materials, production, energy, staffing, sales, daily summary, and exact snapshot restoration in C# before relying on visual presentation.
3. **First playable Godot scene:** Display the factory, resource bar, machinery controls, clock, buying and selling actions, and day summary using placeholders.
4. **Victorian visual polish:** Gradually replace placeholders with purpose-made 2D assets, decorative corners, animated clockwork and sound.

### Definition of done for v0

A player can start a factory, buy materials and fuel, hire sufficient workers, run a boiler and one production line, produce and sell a simple part, see storage and resource shortages, finish a workday, view the ledger, start another day, save and quit at any moment, and reload exactly where they left off.

## Beyond v0 (not committed)

Multiple production lines, components combined into devices and drones, research blueprints, supplier relationships, construction and maintenance, worker development, night shifts, reactive incidents, advisors, hidden legacies, and richer narrative presentation are candidates for later versions—not prerequisites for the first release.

## Development / contributing

This project is currently **in transition**. The current main branch still uses an older ASP.NET Core Web API architecture (with PostgreSQL / EF Core / Docker) and does **not** yet offer a supported Godot run command. Setup, build and test instructions will be documented once the foundation milestone establishes the actual solution layout.

For current planning and technical cleanup, see the [Iron Magnate Linear project](https://linear.app/josh-soll/project/the-iron-magnate-2b4d7ae57455).

---

<p align="center"><em>Build the factory first. Build the empire later.</em></p>
