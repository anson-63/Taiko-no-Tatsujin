# Taiko WPF — Rhythm Game in C#

A Windows WPF implementation of a Taiko-style rhythm game. Built in C# with XAML animations and storyboard-driven UI. Gameplay is time-accurate to audio and .tja charts, with local multiplayer, difficulty selection, and configurable controls/multipliers.

- Smooth page flows: Title → Song Select → Gameplay → Results
- Timing-accurate judgments (Perfect/Good/Miss) with combo, rating text, and SFX
- Big-note logic (simultaneous dual-key presses)
- .tja chart parsing with audio-time synced notes
- Local multiplayer (P1/P2 lanes with separate keybinds)
- Configurable score multiplier and key bindings
- Results artwork varies by score and difficulty

## 👥 Collaborators
- [@anson-63](https://github.com/anson-63)
- [@BCHY34](https://github.com/BCHY34)
- [@hsaufbcsuh](https://github.com/hsaufbcsuh)

## Demo (Overview)

- Storyboard + DoubleAnimation for fades and moving backgrounds
- Absolute, audio-synced note positioning to prevent drift/lag
- Media playback via System.Windows.Media.MediaPlayer

## Table of Contents

- Features
- Project Structure
- Gameplay Rules
- Scoring
- Architecture Notes
- Controls
- Getting Started
- Adding Songs
- License

---

## Features

- Pages: Title, Song Selection, Gameplay, Result
- UI: Smooth fades and moving backgrounds via Storyboard + DoubleAnimation
- Judgments: Timing-accurate hit/miss with combo display, rating text (“良”, “可”), and SFX
- Big Notes: Requires near-simultaneous dual keys of the same color
- Charts: Reads .tja and syncs notes to audio time
- Difficulty: Redesign with multiple levels and mapping via dictionaries
- Multiplayer: P1 + P2 lanes with independent keybinds
- Config: Score multiplier (scaler) and key bindings on the Controls page
- Results: Artwork selected by totalscore and difficulty

## Project Structure

- asset/ — images, sounds, results artwork, etc.
- Models/, pages/ — game models and WPF Pages (Title, Select, Gameplay, Result, Controls)
- App.xaml, MainWindow.xaml — app shell and navigation
- Taiko.csproj, Taiko.sln — project/solution files

Note: bin/, obj/, .vs/ and other build artifacts are intentionally excluded.

## Gameplay Rules

Timing windows (relative to the judgment line):

- Perfect (“良”): |Δt| ≤ 0.05s → +100 points (×2 for big notes)
- Good (“可”): within the overall hit range but not perfect → +50 points (×2 for big notes)
- Miss: note passes > 0.18s late (auto-miss) or unhit

Input mapping:

- Don keys → small/big Don (types 1, 3)
- Kat keys → small/big Ka (types 2, 4)
- Big notes (types 3, 4): after the first press, within ~30 ms both L+R keys of the same color must be held; otherwise ignored and will miss.

On hit:

- Combo +1, show combo and rating text, play SFX, remove note

On miss:

- Combo resets, miss count +1, remove note

## Scoring

Totalscore:

- totalscore = (perfect × 100 × scaler) + (good × 50 × scaler) − (miss × 50 ÷ scaler)

Notes:

- scaler is the user-selected score multiplier from the Controls page
- Result page selects images based on totalscore and difficulty

## Architecture Notes

- WPF with Storyboard + DoubleAnimation for:
  - Title fade-in/out text
  - Song Select moving background
- Absolute, audio-time-based positioning in the render loop to avoid drift and animation lag
- Audio via System.Windows.Media.MediaPlayer
- .tja parsed by a helper class (TjaParser) to populate notes
- Static config variables (e.g., Controlpage.scaler) shared across pages
- Dictionaries map difficulties and related settings

## Controls

Single player (default):

- Don (red): Left/Right key pair (e.g., F/J)
- Kat (blue): Left/Right key pair (e.g., D/K)
- Big notes: press both left+right of the same color nearly simultaneously

Local multiplayer:

- Player 2 has its own four keys (Don L/R, Kat L/R)
- Duplicate lane rendered; logic mirrors Player 1

All bindings are configurable on the Controls page. Choose a multiplier (scaler) from the dropdown.

## Getting Started

Prerequisites:

- Windows 10/11
- .NET Desktop SDK (matching the project’s TargetFramework)
- Visual Studio with the WPF workload, or the dotnet CLI

Build/run:

- Visual Studio: open Taiko.sln and Run
- CLI:
  - dotnet restore
  - dotnet build
  - dotnet run --project Taiko.csproj

## Adding Songs

- Place audio (e.g., .ogg/.mp3) and the matching .tja chart into the expected songs directory used by the app (see asset/ or configuration in code)
- Select the song on the Song Selection page

## License

MIT