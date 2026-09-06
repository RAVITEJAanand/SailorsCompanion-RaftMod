# ⚓ Sailor's Companion — Full Project History, Architecture & Process Guide

**రచయిత (Author)**: KONDURI (RAVITEJAanand)  
**వర్షన్ (Version)**: 1.0.0 (Release)  
**గేమ్ (Target Game)**: Raft (The Final Chapter Update 1.09 / v13.01)  
**టెక్నాలజీ (Technology Stack)**: C#, .NET Standard 2.1, BepInEx 5.4.21, HarmonyLib 2.2, Unity 2020.3 UI  
**తేదీ (Created Date)**: September 2026  

---

## 🌐 1. లైవ్ లింకులు (Official Live Links)

| ప్లాట్‌ఫారమ్ (Platform) | లింక్ (URL) | వివరాలు (Details) |
| :--- | :--- | :--- |
| **Nexus Mods** | [nexusmods.com/raft/mods/154](https://www.nexusmods.com/raft/mods/154) | ప్రధాన మోడ్ పేజీ, డౌన్‌లోడ్స్, కామెంట్స్ & బగ్ రిపోర్ట్స్ |
| **RaftModding.com** | [raftmodding.com/mods/sailors-companion](https://www.raftmodding.com/mods/sailors-companion) | అఫీషియల్ రాఫ్ట్ మోడింగ్ కమ్యూనిటీ పేజీ & RML ప్యాకేజీ |
| **GitHub Repository** | [github.com/RAVITEJAanand/SailorsCompanion-RaftMod](https://github.com/RAVITEJAanand/SailorsCompanion-RaftMod) | ఓపెన్ సోర్స్ కోడ్‌బేస్, ట్రాఫిక్ అనలిటిక్స్ & ఇష్యూ ట్రాకర్ |
| **Discord Community** | [discord.gg/B4EMrR5Vrf](https://discord.gg/B4EMrR5Vrf) | అఫీషియల్ డిస్కార్డ్ సర్వర్, సపోర్ట్ & కమ్యూనిటీ చర్చలు |

---

## 🌟 2. మోడ్ ఫీచర్లు (Complete Features Overview)

### 1. రాఫ్ట్ టెలిపోర్టేషన్ & రీకాల్ (Intelligent Raft Teleportation)

- **Player Recall to Raft [F8]**: సముద్రంలో లేదా సుదూర దీవుల్లో ఎక్కడ ఉన్నా సురక్షితంగా మీ తెప్ప (Raft) డెక్‌పైకి తక్షణమే చేరుస్తుంది.
- **Summon Raft to Player [F9]**: మీ తెప్పను మీరున్న స్థలానికి 18 మీటర్ల దూరంలో సముద్ర ఉపరితలంపై రప్పిస్తుంది. తెప్ప కొట్టుకుపోకుండా వెంటనే ఆటోమేటిక్‌గా యాంకర్ (Anchor) వేస్తుంది.
- **Remote Anchor Toggle**: ఎక్కడి నుంచైనా ఒకే క్లిక్‌తో యాంకర్ వేయడం లేదా ఎత్తడం చేయవచ్చు.
- **On-Screen Toast Notifications**: టెలిపోర్ట్ అయిన దూరం మరియు స్టేటస్ స్క్రీన్‌పై నోటిఫికేషన్‌గా చూపిస్తుంది.

### 2. హై-డెఫినిషన్ కాన్వాస్ UI (Modern Canvas UI [F5])

- 100% ప్యూర్ Unity UI కాన్వాస్ ఆర్కిటెక్చర్ (Zero OnGUI Overhead).
- మెయిన్ టైటిల్ స్క్రీన్‌లో **`MODS`** బటన్ మరియు పాజ్ మెనూలో **`MOD MENU`** బటన్ ఇంజెక్షన్.
- 5 రకాల ప్రత్యేక ట్యాబ్‌లు:
  - 🛡️ **Survival**: God Mode, Infinite Oxygen, No Hunger/Thirst, Infinite Tool Durability, Instant Heal.
  - 🦈 **Raft & World**: Anti-Shark Raft Attacks, Free Instant Crafting, Hook Pull Speed Multiplier (1x-5x), Custom Stack Size (999 వరకు).
  - 🔬 **R&D / Blueprints**: ఒకే క్లిక్‌తో రీసెర్చ్ టేబుల్‌లోని అన్ని బ్లూప్రింట్లు మరియు వస్తువులను అన్‌లాక్ చేయడం.
  - 📦 **Item Spawner**: రాఫ్ట్ గేమ్‌లోని 300+ వస్తువులను లైవ్‌గా సెర్చ్ చేసి +1, +10, +Full Stack ఇన్వెంటరీలోకి తెచ్చుకోవడం.
  - 🧭 **Navigation HUD**: కంపాస్, కోఆర్డినేట్స్, రాఫ్ట్ దిశ మరియు బ్రూస్ షార్క్ దూరం చూపే HUD కంట్రోల్స్.

### 3. 4 నావిగేషన్ HUD స్టైల్స్ [F6 / Shift+F6]

1. **Style 0: Top Compass Ribbon** (Skyrim / Subnautica స్టైల్ కంపాస్ బార్).
2. **Style 1: Modern Compass Tape** (డిగ్రీ మార్కర్లు మరియు రాఫ్ట్ మార్కర్‌తో కూడిన హెడింగ్ టేప్).
3. **Style 2: Mini Pill Widget** (స్క్రీన్ పైభాగంలో తక్కువ స్థలంలో కనిపించే కాంపాక్ట్ విడ్జెట్).
4. **Style 3: Classic Tactical Box** (ప్లేయర్, తెప్ప, మరియు షార్క్ లైవ్ టెలిమెట్రీతో కూడిన ఫుల్ బాక్స్).

- **100ms CPU Throttling**: గేమ్ FPS ఏమాత్రం తగ్గకుండా 10Hz ఆప్టిమైజ్డ్ మ్యాథమెటికల్ లూప్.

### 4. ఫ్లై / నో-క్లిప్ మోడ్ (Fly & Noclip Mode [F / F7])

- గాల్లో మరియు నీటిలో అడ్డంకులు లేకుండా స్వేచ్ఛగా ఎగరవచ్చు.
- కంట్రోల్స్: `WASD` దిశ, `Space` పైకి, `LeftShift/Ctrl` కిందకి, `LeftAlt` టర్బో బూస్ట్ స్పీడ్.
- మెనూ ఓపెన్ అయినప్పుడు ప్లేయర్ మూవ్‌మెంట్ ఆటోమేటిక్‌గా ఫ్రీజ్ అయ్యే ఇన్‌పుట్ సప్రెషన్.

---

## 🏛️ 3. కోడ్ ఆర్కిటెక్చర్ & `[START]` / `[END]` డీమార్కేషన్

భవిష్యత్తులో సులభంగా కోడ్ రివ్యూ చేయడానికి మరియు అప్‌డేట్స్ చేయడానికి, కోడ్‌బేస్‌లోని మొత్తం 18 ఫైల్స్‌కు ప్రామాణికమైన `#region [START] ...` మరియు `#endregion // [END] ...` ట్యాగ్‌లు ఏర్పాటు చేయబడ్డాయి.

### ఎడిటర్‌లో వాడటం ఎలా? (VS Code / Visual Studio Shortcuts)

- **`Ctrl + K, Ctrl + 0`**: మొత్తం కోడ్‌ను ఒక్కసారిగా ముడుచుకునేలా చేస్తుంది (Fold All).
- **`Ctrl + K, Ctrl + J`**: మొత్తం కోడ్‌ను విప్పదీస్తుంది (Unfold All).

### ఫైల్స్ & రీజియన్ల మ్యాపింగ్

```text
ModSource/SailorsCompanion/
├── Plugin.cs
│   ├── [START] CONFIGURATION DEFINITIONS
│   ├── [START] PLUGIN INITIALIZATION & CONFIG BINDING
│   ├── [START] HARMONY PATCH REGISTRATION
│   ├── [START] MANAGER INITIALIZATION & LIFECYCLE
│   ├── [START] ENGINE UPDATE LOOPS & KEY BINDINGS
│   └── [START] STATS & SPEED MULTIPLIERS TICK
├── Features/
│   ├── TeleportManager.cs
│   │   ├── [START] TOAST NOTIFICATION ENGINE
│   │   ├── [START] FEATURE 1: PLAYER RECALL TO RAFT (F8)
│   │   ├── [START] FEATURE 2: SUMMON RAFT TO PLAYER (F9)
│   │   └── [START] FEATURE 3: REMOTE ANCHOR TOGGLE
│   └── FlyController.cs
│       ├── [START] STATE & FIELDS
│       ├── [START] FLY CONTROLLER LIFECYCLE LOOP
│       ├── [START] FLIGHT ENGAGEMENT & TOGGLE
│       └── [START] 6-AXIS FLIGHT MOVEMENT ENGINE
├── UI/
│   ├── CanvasModUI.cs (100% Unity Canvas High-DPI UI)
│   │   ├── [START] LIFECYCLE & INITIALIZATION
│   │   ├── [START] MOD WINDOW FRAME & TABS CONTROLLER
│   │   ├── [START] TAB 0: SURVIVAL CHEATS & VITALS
│   │   ├── [START] TAB 1: RAFT, SHARK & WORLD CHEATS
│   │   ├── [START] TAB 2: RESEARCH & R&D BLUEPRINTS
│   │   ├── [START] TAB 3: ITEM SPAWNER & SEARCH
│   │   ├── [START] TAB 4: NAVIGATION HUD SETTINGS
│   │   ├── [START] UI COMPONENT BUILDERS
│   │   ├── [START] ENGINE UPDATE LOOP & HOTKEYS
│   │   └── [START] WORLD TIME & WEATHER HELPERS
│   ├── HUDOverlay.cs (Navigation Telemetry & Compass)
│   │   ├── [START] HUD: CACHED STATE & METRICS
│   │   ├── [START] HUD: STYLES & TEXTURES
│   │   ├── [START] HUD: 10HZ COMPASS & TELEMETRY COMPUTATIONS
│   │   ├── [START] HUD: ONGUI RENDER DISPATCHER
│   │   ├── [START] HUD: STYLES 0-3
│   │   ├── [START] HUD: TOAST NOTIFICATION ENGINE
│   │   └── [START] HUD: NAVIGATION & HEADING MATH HELPERS
│   └── ModGUI.cs (Legacy IMGUI Fallback Engine)
├── Patches/
│   ├── SharkPatch.cs          -> [START] PATCH: ANTI-SHARK RAFT PROTECTION
│   ├── DurabilityPatch.cs     -> [START] PATCH: INFINITE TOOL & ARMOR DURABILITY
│   ├── CraftingPatch.cs       -> [START] PATCH: FREE INSTANT CRAFTING
│   ├── StackSizePatch.cs      -> [START] PATCH: CUSTOM RESOURCE STACK SIZE
│   ├── PlayerStatsPatch.cs    -> [START] PATCH: GOD MODE DAMAGE IMMUNITY
│   ├── HookPatch.cs           -> [START] PATCH: FAST HOOK PULL & GATHER SPEED
│   ├── MainMenuPatch.cs       -> [START] PATCH: TITLE & PAUSE MENU BUTTON INJECTIONS
│   ├── CursorPatch.cs         -> [START] PATCH: CURSOR UNLOCKING & VISIBILITY
│   └── GameManagerPatch.cs    -> [START] PATCH: GAMEMANAGER TICK HOOK
└── Helpers/
    ├── InputHelper.cs         -> [START] KEY PRESS & HELD POLLING
    ├── PlayerHelper.cs        -> [START] LOCAL PLAYER RESOLUTION ENGINE
    └── PluginInfo.cs          -> [START] MOD METADATA & CONSTANTS
```

---

## 🛠️ 4. బిల్డ్ & పబ్లిషింగ్ ప్రక్రియ (Build & Publish Commands)

### ప్రాజెక్ట్‌ను కంపైల్ చేయడం

```powershell
# ModSource ఫోల్డర్‌లోకి వెళ్లి Release బిల్డ్ రన్ చేయాలి
cd "d:\SteamLibrary\steamapps\common\Raft\ModSource\SailorsCompanion"
dotnet build SailorsCompanion.csproj -c Release
```

- అవుట్‌పుట్ DLL నేరుగా `d:\SteamLibrary\steamapps\common\Raft\BepInEx\plugins\SailorsCompanion\SailorsCompanion.dll` లోకి వెళ్తుంది.

### డిస్ట్రిబ్యూషన్ జిప్ ఫైల్ తయారు చేయడం

```powershell
Copy-Item "d:\SteamLibrary\steamapps\common\Raft\BepInEx\plugins\SailorsCompanion\SailorsCompanion.dll" "d:\SteamLibrary\steamapps\common\Raft\Publish\BepInEx\plugins\SailorsCompanion\SailorsCompanion.dll" -Force
Compress-Archive -Path "d:\SteamLibrary\steamapps\common\Raft\Publish\BepInEx\*" -DestinationPath "d:\SteamLibrary\steamapps\common\Raft\Publish\SailorsCompanion_BepInEx_v1.0.0.zip" -Force
```

### గిట్‌హబ్‌లోకి కమిట్ & పుష్ చేయడం

```powershell
cd "d:\SteamLibrary\steamapps\common\Raft\ModSource\SailorsCompanion"
git add .
git commit -m "Your commit message"
git push origin main
```

---

## 📊 5. మోడ్ పర్ఫామెన్స్ & ఫీడ్‌బ్యాక్ ఎలా గమనించాలి?

1. **Nexus Mods (కామెంట్స్ & రేటింగ్స్)**:
   - మీ లింక్: `https://www.nexusmods.com/raft/mods/154`
   - **`POSTS`** ట్యాబ్‌లో ప్లేయర్లు రాసే కామెంట్లు మరియు ప్రశ్నలు ఉంటాయి.
   - **`BUGS`** ట్యాబ్‌లో ఏవైనా లోపాలుంటే రిపోర్ట్ చేస్తారు.
   - వెబ్‌సైట్ పైభాగంలో ఉన్న బెల్ ఐకాన్ (🔔) ద్వారా కొత్త కామెంట్ అలర్ట్స్ వస్తాయి.

2. **RaftModding.com (డౌన్‌లోడ్స్)**:
   - మీ లింక్: `https://www.raftmodding.com/mods/sailors-companion`
   - డౌన్‌లోడ్ కౌంట్ మరియు లైక్స్ కనిపిస్తాయి.
   - వాళ్ల కమ్యూనిటీ Discord సర్వర్‌లో ప్లేయర్లు మోడ్ గురించి చర్చిస్తారు.

3. **టెక్నికల్ ఎర్రర్ లాగ్స్ (In-Game Log)**:
   - పాత్: `d:\SteamLibrary\steamapps\common\Raft\BepInEx\LogOutput.log`
   - గేమ్‌లో ఎక్కడైనా రెడ్ ఎర్రర్స్ (Exceptions) ఉన్నాయా అని తనిఖీ చేయడానికి ఈ లాగ్ ఫైల్ ఉపయోగపడుతుంది.

---

## 🔮 6. తదుపరి అప్‌డేట్ (v1.1.0) ప్లానింగ్ సూచనలు

రాబోయే కొద్ది రోజులు ప్లేయర్ల రెస్పాన్స్ చూసిన తర్వాత v1.1.0 లో చేర్చదగిన కొన్ని మంచి ఆలోచనలు:

1. **Radar / Island Waypoint ESP**: సముద్రంలో సమీపంలో ఉన్న పెద్ద దీవులు, రేడియో టవర్లను స్క్రీన్‌పై మార్కర్‌గా చూపించడం.
2. **Auto-Water Crops & Purify**: పంటలకు నీళ్లు ఆటోమేటిక్‌గా పెట్టే సదుపాయం.
3. **Storage Chest Sorter**: బాక్సుల్లో ఉన్న వస్తువులను ఒకే క్లిక్‌తో అక్షరక్రమంలో సర్దడం (Stack & Sort).
4. **Custom Keybinding Menu**: మోడ్ మెనూ నుంచే F5, F6, F8, F9 బటన్లను యూజర్ తనకు నచ్చిన కీలకు మార్చుకునే సదుపాయం.
