# Circuit Repair

Circuit Repair is a mobile-first circuit rotation puzzle game built with Unity.

Rotate circuit tiles to connect the power source, light bulb, and ground. Each puzzle is procedurally generated, challenging players to restore a complete electrical path across the board.

This project was developed as a portfolio piece to explore deterministic procedural content generation, graph traversal, constraint-based algorithms, modular gameplay architecture, and cross-platform development.

## Play

Playable WebGL build:

https://mertseker.itch.io/circuit-repair

For the best experience, play in a desktop browser.

## Features

- Procedurally generated 5x5 circuit puzzles
- Deterministic, seed-based level generation
- Randomized backtracking with constraint pruning
- Difficulty filtering based on circuit path characteristics
- Reproducible puzzle scrambling
- Circuit connectivity and completion evaluation
- Previous, Restart, and Next puzzle navigation
- Mouse and touch controls
- Android, Windows, and WebGL builds
- Automated stress testing across 1,000 generation seeds

## Procedural Generation

The puzzle generator uses a solution-first approach.

It begins by constructing a Hamiltonian path that visits every cell of the board exactly once, connecting the fixed source and ground positions.

The generation pipeline includes:

1. **Path Generation:** Randomized backtracking constructs a full-grid path.
2. **Constraint Pruning:** Connectivity and remaining-degree checks eliminate invalid search branches.
3. **Topology Construction:** The generated path is converted into circuit components and orientations.
4. **Difficulty Filtering:** Candidate puzzles are evaluated against bend-count constraints.
5. **Deterministic Scrambling:** A seeded scrambling process rotates selected components to create the playable starting state.
6. **Circuit Evaluation:** Graph traversal evaluates reciprocal connections, source reachability, and puzzle completion.

The generation system was stress-tested across 1,000 seeds to verify successful puzzle construction and consistency.

## Tech Stack

- Unity 6.3 LTS (6000.3.19f1)
- C#
- Unity Input System
- Procedural Content Generation
- Android, Windows, WebGL

## Assets and AI Assistance

Some visual assets were created using AI-assisted tools.

AI tools were also used to assist parts of the code implementation. Gameplay systems, procedural generation behavior, integration, debugging, and validation were developed through an iterative engineering process.

Third-party assets remain subject to their respective licenses and attribution requirements.

## Repository Note

This repository is intended as a portfolio and source-code showcase.

The published WebGL version represents the V1 single-circuit gameplay prototype. The repository also contains experimental multi-circuit systems developed while exploring potential future mechanics.

The current playable version focuses on deterministic procedural puzzle generation rather than a complete campaign or commercial release.

## About the Project

Circuit Repair was designed as a compact puzzle game with an emphasis on procedural generation and gameplay-system architecture.

The goal was to build a functional, cross-platform puzzle prototype while exploring how graph-based algorithms and constraints can generate valid, reproducible levels.

The project includes:

- A solution-first procedural generation pipeline
- Modular C# gameplay and evaluation systems
- Separate authored and generated puzzle workflows
- Deterministic puzzle navigation and restarting
- Automated generation testing
- Mobile-first UI and cross-platform build validation
