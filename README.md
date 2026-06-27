# Arbor

*A chess engine written in C#.*

The first seed of Arbor was planted by another project of mine: a pass-and-play chess game built in Unity. After weeks of planning, watching YouTube videos (thanks, Sebastian Lague!), and getting intimidated by big words like Zobrist Hashing and Iterative Deepening, I finally decided to build it.

What started as a simple goal of building a chess engine capable of beating a 1000-rated player gradually evolved into a much more capable engine implementing all of those once-intimidating concepts.

A simple alpha-beta search has gradually grown into a modern chess engine through countless hours of experimentation, testing, debugging, and refinement.

Rather than focusing only on playing strength, Arbor has always been about learning the ideas behind chess engine design. Every major feature has been implemented incrementally, tested against previous versions, and documented throughout its development.

## Try Arbor
Play against Arbor on Lichess: <[Play!](https://lichess.org/@/ArborBot)>

## Features

### Search

* Negamax with Alpha-Beta Pruning
* Quiescence Search
* Iterative Deepening
* Transposition Tables
* Zobrist Hashing
* Killer Moves
* History Heuristic
* Late Move Reductions (LMR)
* Null Move Pruning (NMP)

### Evaluation

* Material Evaluation
* Piece-Square Tables
* Dynamic Game Phase Interpolation
* Mobility
* Pawn Structure
* Passed Pawns
* King Safety

### Other

* UCI Compatible
* Time Management
* Threefold Repetition Detection
* Insufficient Mating Material Detection

## Why "Arbor"?

An arbor is a tree—a fitting name for both the search tree explored by every chess engine and the way this project has grown over time, from a small sapling into something far more capable.

## Future Work

Version 1.0 marks the completion of Arbor's core architecture. Whether the engine itself continues to grow or shifts toward a dedicated front-end remains to be seen, but this is certainly not the end of the project.
---

Thank you for checking out Arbor.
If you'd like to follow my other projects, you can find them on my GitHub profile