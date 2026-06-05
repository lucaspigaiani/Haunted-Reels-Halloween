# Haunted Reels - Halloween Slot Machine

## Overview

Haunted Reels is a Halloween-themed slot machine game developed in Unity 2022 LTS for WebGL.

The project simulates a modern video slot featuring:

* 5 reels × 3 rows
* 10 active paylines
* Weighted RNG system
* Wild symbol support
* Adjustable betting system
* Auto Play mode
* Spine animation integration
* Winning payline visualization

The project was developed as a technical challenge with a focus on clean architecture, maintainability, and separation of responsibilities.

---

## Technologies

* Unity 2022 LTS
* C#
* Spine Runtime
* WebGL

---

## Project Structure

```text
Assets/
└── Scripts/
    ├── SpinController.cs
    ├── ReelController.cs
    ├── RNGService.cs
    ├── PaylineSystem.cs
    ├── PaylineCalculator.cs
    ├── PaylineDrawer.cs
    ├── PoolSystem.cs
    ├── ReelAnimator.cs
    ├── SymbolSystem.cs
    └── SpinResult.cs
```

---

## Architecture

### SpinController

Main game controller responsible for:

* Credits management
* Betting system
* Spin flow
* Auto Play
* UI updates
* Win processing

### ReelController

Responsible for:

* Reel movement
* Symbol recycling
* Spin timing
* Stop sequence
* Final symbol placement

### RNGService

Generates slot results using weighted probabilities.

Responsibilities:

* Random symbol selection
* Weight-based probability calculation
* Spin result generation

### PaylineCalculator

Contains the pure payline evaluation logic.

Responsibilities:

* Count symbol occurrences
* Process Wild symbols
* Determine winning combinations
* Calculate payouts

### PaylineDrawer

Responsible for the visual presentation of paylines.

Responsibilities:

* Draw paylines on screen
* Highlight winning combinations
* Control payline display timing
* Manage visual feedback

### PaylineSystem

Acts as the coordinator between calculation and presentation layers.

Responsibilities:

* Trigger payline evaluation
* Aggregate winning results
* Forward results to visual systems
* Provide a single entry point for win processing

### SymbolSystem

ScriptableObject containing symbol configuration:

* Symbol type
* Weight
* Sprite
* Spine skin
* Payout multipliers

---

## Main Design Decisions

### 1. Separation of Responsibilities

The payline feature is divided into three dedicated components:

```text
PaylineCalculator → Calculates wins
PaylineSystem     → Coordinates evaluation flow
PaylineDrawer     → Displays winning paylines
```

Benefits:

* Easier maintenance
* Better testability
* Clear responsibility boundaries
* Reduced coupling

---

### 2. Pre-Generated Spin Results

The final spin result is generated before the reels start spinning.

Benefits:

* Deterministic behavior
* Easier debugging
* Predictable reel stopping
* Common slot-machine industry approach

---

### 3. Weighted RNG

Symbol appearance is controlled by configurable weights.

```csharp
public int Weight;
```

Benefits:

* Easy balancing
* Designer-friendly workflow
* Adjustable probabilities without code changes

---

### 4. ScriptableObject-Based Symbols

Each symbol is configured through a ScriptableObject.

Benefits:

* Centralized configuration
* Reusable data assets
* Faster balancing process

---

### 5. Wild Symbol Logic

Wild symbols contribute to the symbol with the highest occurrence in a payline.

Example:

```text
L3 | H1 | L3 | Wild | L3
```

Result:

```text
4x L3
```

Benefits:

* Simple player understanding
* Straightforward implementation
* Increases Wild value without excessive complexity

---

### 6. Dual Rendering Support

Symbols support both Sprite and Spine rendering.

Benefits:

* Better visual flexibility
* Optimized performance
* Easy transition between static and animated symbols

---

### 7. Coroutine-Based Timing

Coroutines are used for:

* Reel stop delays
* Auto Play timing
* Payline display sequences

Benefits:

* Readable code
* Precise timing control
* Easy maintenance

---

## Game Flow

```text
1. Player selects a bet amount
2. Player presses Spin
3. RNG generates the final result
4. Reels start spinning
5. Reels stop sequentially
6. Paylines are evaluated
7. Winnings are calculated
8. Credits are updated
9. Winning paylines are displayed
10. Auto Play starts a new spin if enabled
```

---

## Symbol Configuration

Each symbol contains:

| Property    | Description          |
| ----------- | -------------------- |
| Type        | Symbol identifier    |
| Weight      | Probability weight   |
| Sprite      | Static image         |
| SpineSkin   | Animated Spine skin  |
| Multiplier3 | Payout for 3 matches |
| Multiplier4 | Payout for 4 matches |
| Multiplier5 | Payout for 5 matches |

---

## Payout System

All 10 paylines are always active.

The total bet is automatically distributed equally across all paylines during payout calculation.

Formula:

```text
Win = Multiplier × (Total Bet / 10)
```

Example:

```text
Total Bet = 100
Winning Symbol Multiplier = 150x

Win = 150 × (100 / 10)
Win = 1500
```

---

## Features

### Betting System

* Adjustable bet values
* Minimum and maximum limits
* Credit validation

### Auto Play

* Automatic consecutive spins
* Configurable delay
* Stops automatically when credits are insufficient

### Winning Payline Visualization

* Sequential payline display
* Highlighted winning combinations
* Different colors for winning paylines
* Configurable display timing

### Weighted RNG

* Configurable symbol frequencies
* Balanced gameplay experience

---

## How to Run

1. Open the project using Unity 2022 LTS.
2. Import Spine Runtime if required.
3. Open the main scene.
4. Press Play in the Unity Editor.

### WebGL Build

```text
File → Build Settings → WebGL → Build
```

---

## Time Spent

| Activity                         |   Hours |
| -------------------------------- | ------: |
| Project setup and architecture   |      4h |
| Reel system and symbol pooling   |      6h |
| RNG and spin result generation   |      4h |
| Payline evaluation system        |      5h |
| Betting system and UI            |      3h |
| Spine integration and animations |      3h |
| Auto Play implementation         |      1h |
| Debugging and bug fixes          |      2h |
| Documentation and WebGL build    |      2h |
| **Total**                        | **30h** |

The project was developed over approximately **30 hours**, including implementation, testing, debugging, balancing, and documentation.

---

## Author

**Lucas Pigaiani**

Portfolio: https://lucaspigaiani.home.blog

Repository: https://github.com/lucaspigaiani/Haunted-Reels-Halloween
