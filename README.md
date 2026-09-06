# ⚓ Sailor's Companion — Quality of Life & Survival Suite for Raft

![Sailor's Companion Banner](banner.png)

**Sailor's Companion** is the ultimate all-in-one Quality of Life, Navigation, and Creative Survival mod for **Raft (The Final Chapter)**. Designed with a modern high-DPI UI, immersive on-screen HUD styles, intelligent raft recall teleportation, instant R&D blueprint unlocking, and robust protection against annoying game mechanics.

---

## 🌟 Key Features

### ⚡ 1. Intelligent Raft Teleportation & Recall (Never Lose Your Raft!)
- **Recall Player to Raft (`[F8]` or UI Button)**: Teleport safely back onto the raft deck from anywhere in the ocean or distant islands. Automatically snaps you to the nearest walkable deck block, parents your movement to the raft pivot (so you never glitch or fall through), and resets falling inertia.
- **Summon Raft to Player (`[F9]` or UI Button)**: Calls your raft directly in front of you on the water surface (~18m ahead) and automatically drops the anchor so it cannot drift away.
- **Remote Anchor Toggle**: Drop or raise the anchor remotely from anywhere in the world.

### 🧭 2. Real-Time Navigation HUD & Shark Radar
- Real-time compass heading (degrees + cardinal directions N/NE/E/SE/S/SW/W/NW).
- Live player XYZ coordinates & world clock.
- **Raft Distance & Drift Tracker**: Dynamic directional arrow pointing toward your raft, distance in meters, drift speed in knots, and anchored status.
- **Shark Bruce Radar**: Live proximity tracker showing Bruce's distance with red warning alerts when he approaches attack range (<25m).
- **4 Customizable HUD Styles (`[Shift + F6]` to cycle)**:
  1. *Sleek Ribbon* (Default, compact top-left glass bar).
  2. *Compass Bar* (Skyrim/Subnautica style top-center scrolling tape).
  3. *Mini Pill* (Minimalist one-line badge).
  4. *Classic Box* (Detailed stacked metrics).

### 🔬 3. Research Table & Blueprint Automation (R&D)
- One-click **"Unlock All R&D Recipes & Blueprints"**: Instantly learns every crafting recipe, tool, engine, weapon, furniture, and story blueprint in the game without sacrificing materials or searching for blueprints.

### 🦈 4. Raft & World Protection
- **Anti-Shark Raft Protection**: Stops Bruce the Shark from ever biting or destroying your raft foundations.
- **Free Instant Crafting**: Craft any recipe in the crafting menu without consuming materials.
- **Debris Hook Reel Speed Multiplier**: Reel in floating barrels and plastic up to 5x faster with reduced gather times.
- **Custom Stack Size (up to 999)**: Stack raw materials up to 999 per slot.
- **Time & Weather Controller**: Set time to Morning, Noon, or Night, and change weather to Sunny, Calm, Rain, or Fog.

### 🛡️ 5. Survival & God Mode
- **God Mode**: Full invulnerability to animal attacks, drowning, and environmental hazards.
- **Infinite Oxygen**: Dive deep underwater as long as you want without drowning.
- **Freeze Hunger & Thirst**: Keep food and hydration meters locked at 100%.
- **Infinite Tool Durability**: Tools, weapons, armor, and hooks never break or lose durability.
- **Speed Multipliers**: Customizable swimming and sprinting speed multipliers (1x to 4x).
- **Fly / Noclip Mode (`[F]` or `[F7]`)**: Fly freely through the air and underwater with WASD + Space/Shift + Alt turbo boost.

### 📦 6. In-Game Item Spawner
- Browse and search all 300+ items in Raft with instant `+1`, `+10`, and `+Full Stack` buttons directly into your inventory.

---

## ⌨️ Controls & Hotkeys Reference

| Key | Action |
|---|---|
| **`F5`** or **`Insert`** | Toggle Main Mod Menu Window |
| **`F6`** | Toggle On-Screen Navigation HUD |
| **`Shift + F6`** | Cycle Navigation HUD Styles (Ribbon / Compass / Pill / Box) |
| **`F8`** | **⚡ Recall Player to Raft** (Teleport onto deck) |
| **`F9`** | **⛵ Summon Raft to Player & Auto-Anchor** |
| **`F`** or **`F7`** | Toggle Fly / Noclip Mode |
| **`Escape`** | Close Mod Menu Window / Return to Game |

---

## 📥 Installation

### Method A: BepInEx (Recommended)
1. Install [BepInEx 5 (x64)](https://github.com/BepInEx/BepInEx/releases) into your Raft root game folder (`steamapps/common/Raft`).
2. Copy `SailorsCompanion.dll` into `steamapps/common/Raft/BepInEx/plugins/SailorsCompanion/`.
3. Launch Raft through Steam!

### Method B: Raft Mod Loader (RML)
1. Open the **Raft Mod Loader** launcher.
2. Place `SailorsCompanion.rmod` into your `steamapps/common/Raft/Mods/` directory.
3. Enable **Sailor's Companion** in the Mod Manager and launch the game.

---

## 👥 Multiplayer Compatibility
- **Client-Side Friendly**: The Navigation HUD, Teleport to Raft, Fly Mode, Item Spawner, and Durability patches function when joining multiplayer games.
- **Host Recommended**: Features that manipulate world state (weather control, summoning raft, anti-shark damage) work best when you are the host or playing singleplayer.

---

## 📜 License
Released under the **MIT License**. Free for personal and community use.
