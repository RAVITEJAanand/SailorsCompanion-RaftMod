# ⚓ Sailor's Companion — Full Project History, Architecture & Process Guide

**రచయిత (Author)**: KONDURI (RAVITEJAanand)  
**వర్షన్ (Version)**: 1.1.0 (Release)  
**గేమ్ (Target Game)**: Raft (The Final Chapter Update 1.09 / v13.01)  
**టెక్నాలజీ (Technology Stack)**: C#, .NET Standard 2.1, BepInEx 5.4.21, HarmonyLib 2.2, Unity 2020.3 UI  
**తేదీ (Created Date)**: September 2026  

---

## 🌐 1. లైవ్ లింకులు (Official Live Links)

| ప్లాట్‌ఫారమ్ (Platform) | లింక్ (URL) | వివరాలు (Details) |
| :--- | :--- | :--- |
| **Nexus Mods** | [nexusmods.com/raft/mods/155](https://www.nexusmods.com/raft/mods/155) | ప్రధాన మోడ్ పేజీ, డౌన్‌లోడ్స్, కామెంట్స్ & బగ్ రిపోర్ట్స్ |
| **RaftModding.com** | [raftmodding.com/mods/sailors-companion](https://www.raftmodding.com/mods/sailors-companion) | అఫీషియల్ రాఫ్ట్ మోడింగ్ కమ్యూనిటీ పేజీ & RML ప్యాకేజీ |
| **GitHub Repository** | [github.com/RAVITEJAanand/SailorsCompanion-RaftMod](https://github.com/RAVITEJAanand/SailorsCompanion-RaftMod) | ఓపెన్ సోర్స్ కోడ్‌బేస్, ట్రాఫిక్ అనలిటిక్స్ & ఇష్యూ ట్రాకర్ |
| **Discord Community** | [discord.gg/B4EMrR5Vrf](https://discord.gg/B4EMrR5Vrf) | అఫీషియల్ డిస్కార్డ్ సర్వర్, సపోర్ట్ & కమ్యూనిటీ చర్చలు |

---

## 🌟 2. v1.1.0 కొత్త ఫీచర్లు (New Features Overview)

### 1. స్మార్ట్ బోట్ కంట్రోల్ (Smart Boat Propulsion & Auto-Align Sails)
- **ఇండిపెండెంట్ ఇంజిన్ & సెయిల్ కంట్రోల్**: ఇంజిన్ వీల్స్‌ని `[Toggle All Engines]` (F3) తో మరియు సెయిల్స్‌ని `[Toggle All Sails]` (F4) తో విడివిడిగా ఒకదానితో ఒకటి సంబంధం లేకుండా ఆన్/ఆఫ్ చేయవచ్చు.
- **యాంగిల్ ప్రిజర్వేషన్**: సెయిల్స్‌ను తెరిచినా లేదా మూసినా మీ పూర్వపు రుడ్డర్ / స్టీరింగ్ యాంగిల్ డిస్టర్బ్ అవ్వకుండా భద్రపరుస్తుంది.
- **స్మార్ట్ సెయిల్ ఆటో-ఎలైన్ మోడ్స్**:
  - `Manual`: సాధారణ ప్లేయర్ హ్యాండ్‌మేడ్ యాంగిల్స్.
  - `Auto-Align Wind`: గాలి వీచే దిశను (Wind Direction) ఆటోమేటిక్‌గా రీడ్ చేసి, గరిష్ట వేగం కోసం అన్ని సెయిల్స్‌ను ఆటోమేటిక్‌గా సరైన కోణానికి తిప్పుతుంది!
  - `Follow Raft Heading`: తెప్ప కదులుతున్న ఫిజికల్ డైరెక్షన్‌కి సెయిల్స్‌ను ఎలైన్ చేస్తుంది.

### 2. ఐలాండ్ 3D పల్స్ స్కానర్ (Item Detector & Island Sonar Pulse)
- **10 సెకన్ల 3D పల్స్ స్కానర్**: 15 మీటర్ల పరిధిలో భూమిపైన మరియు నీటి అడుగున ఉన్న లూట్, చెస్ట్‌లు, పండ్లు, బెర్రీలు, పూలు, క్లే, సాండ్ మరియు మెటల్/కాపర్ ఓర్‌లను గుర్తించి డైరెక్షనల్ మీటర్లు చూపిస్తుంది.
- **సర్వైవల్ బ్యాలెన్స్**: సర్వైవల్ మోడ్‌లో 10 సెకన్లు యాక్టివ్‌గా ఉండి 30 సెకన్ల కూల్‌డౌన్ ఉంటుంది. క్రియేటివ్ మోడ్‌లో ఎప్పుడైనా వాడవచ్చు.

### 3. ఐలాండ్ హ్యాండ్ పికప్ & రీఫ్ షాలో వాటర్ హార్వెస్టింగ్ (Hand Pickup & Reef Mining)
- **ల్యాండ్ హ్యాండ్ పికప్**: నేలపై ఉండే పువ్వులు, పండ్లు, విత్తనాలను హుక్ లేకుండా చేత్తోనే నేరుగా ఏరుకోవచ్చు.
- **అండర్‌వాటర్ రీఫ్ మైనింగ్**: లోతులేని నీళ్లలోని సాండ్, క్లే, స్క్రాప్, మెటల్/కాపర్ ఓర్‌లను హుక్ లేకుండా చేతులతోనే తవ్వవచ్చు.
- **0.4s ర్యాపిడ్ మైనింగ్**: షార్క్ వచ్చేలోపు సురక్షితంగా బయటకు రావడం కోసం ఛానెలింగ్ టైమ్‌ను 3 సెకన్ల నుంచి 0.4 సెకన్లకు తగ్గించి వేగవంతం చేయబడింది.

### 4. ఓషన్ మాగ్నెట్ డిబ్రిస్ పుల్ (Magnetic Ocean Debris Collector)
- **20 మీటర్ల అయస్కాంత ఆకర్షణ**: సముద్రంలో తేలే బారెల్స్, వుడ్, ప్లాస్టిక్ వంటి డిబ్రిస్‌ను ప్లేయర్ వైపు సహజమైన నీటి తేలియాడే భౌతికశాస్త్రం (Floating Physics) తో సున్నితంగా లాగుతుంది.
- **సర్వైవల్ బ్యాలెన్స్**: సర్వైవల్‌లో 45 సెకన్లు పని చేసి 60 సెకన్ల కూల్‌డౌన్ ఉంటుంది. క్రియేటివ్‌లో నిరంతరాయంగా వాడవచ్చు.

### 5. ఆప్షనల్ ప్రో క్విక్ హాట్‌కీస్ (Anti-Conflict Pro Hotkeys)
- **డీఫాల్ట్ UI-ఫస్ట్ డిజైన్**: సాధారణ ప్లేయర్లకు కీబోర్డ్ గందరగోళం లేకుండా అన్ని ఆప్షన్లు మెనూ బటన్ల ద్వారా సులభంగా ఆపరేట్ చేయవచ్చు.
- **ప్రో హాట్‌కీస్ ఆప్షన్**: మెనూలో `[Enable Quick Hotkeys]` ఆన్ చేస్తే గేమ్‌ప్లే లోనే ఫాస్ట్ యాక్షన్ కీలు అన్‌లాక్ అవుతాయి:
  - `[F4]` — Toggle All Sails
  - `[F3]` — Toggle All Engines
  - `[F7]` — Toggle Ocean Magnet
  - `[F10]` — Trigger Island Scan

---

## 🏛️ 3. కోడ్ ఆర్కిటెక్చర్ & `[START]` / `[END]` డీమార్కేషన్

కోడ్‌బేస్‌లోని ప్రతీ ఫైల్ మరియు మెథడ్‌కు ప్రామాణికమైన `#region [START] ...` మరియు `#endregion // [END] ...` ట్యాగ్‌లు ఏర్పాటు చేయబడ్డాయి:

```text
ModSource/SailorsCompanion/
├── Plugin.cs                          -> ప్రధాన కాన్ఫిగరేషన్, లైఫ్‌సైకిల్ & హాట్‌కీ మేనేజర్
├── PluginInfo.cs                      -> మోడ్ మెటాడేటా & వెర్షన్ 1.1.0 కాన్‌స్టంట్స్
├── InputHelper.cs                     -> కీబోర్డ్ ఇన్‌పుట్ డిటెక్షన్ & పోలింగ్
├── PlayerHelper.cs                    -> లోకల్ ప్లేయర్ రిజల్యూషన్ ఇంజిన్
├── Features/
│   ├── BoatController.cs              -> స్మార్ట్ బోట్ కంట్రోల్, సెయిల్ & ఇంజిన్ ఆటోమేషన్
│   ├── ItemDetector.cs                -> 3D పల్స్ స్కానర్ & సోనార్ ఇండికేటర్స్
│   ├── ReefHandHarvesting.cs          -> ఐలాండ్ & రీఫ్ హ్యాండ్ పికప్ ఇంజిన్
│   ├── MagneticCollector.cs           -> 20m ఓషన్ మాగ్నెట్ డిబ్రిస్ అట్రాక్షన్
│   ├── TeleportManager.cs             -> రీకాల్ టు రాఫ్ట్ (F8), సమ్మన్ రాఫ్ట్ (F9)
│   ├── FlyController.cs               -> 6-యాక్సిస్ ఫ్లై & నోక్లిప్ ఇంజిన్
│   ├── Cheat.cs                       -> గాడ్ మోడ్, ఇన్ఫినిట్ ఆక్సిజన్, అన్‌లాక్ బ్లూప్రింట్స్
│   └── UpdateChecker.cs               -> ఆటోమేటిక్ గిట్‌హబ్ అప్‌డేట్ డిటెక్టర్
├── UI/
│   ├── CanvasModUI.cs                 -> 1.3x స్కేల్డ్ రాఫ్ట్ వుడెన్ ప్లాంక్ మెనూ కాన్వాస్
│   └── HUDOverlay.cs                  -> ఆన్-స్క్రీన్ నావిగేషన్ కంపాస్ & షార్క్ రాడార్
└── Patches/
    ├── HarvestingPatch.cs             -> PickupChanneling 0.4s & బేర్ హ్యాండ్ రీఫ్ ప్యాచ్
    ├── SharkPatch.cs                  -> యాంటీ-షార్క్ రాఫ్ట్ ప్రొటెక్షన్
    ├── DurabilityPatch.cs             -> ఇన్ఫినిట్ టూల్ & ఆర్మర్ డ్యూరబిలిటీ
    ├── CraftingPatch.cs               -> ఫ్రీ ఇన్‌స్టంట్ క్రాఫ్టింగ్
    ├── StackSizePatch.cs              -> కస్టమ్ స్టాక్ సైజ్ ప్యాచ్
    ├── PlayerStatsPatch.cs            -> గాడ్ మోడ్ డ్యామేజ్ ఇమ్యూనిటీ
    ├── HookPatch.cs                   -> ఫాస్ట్ హుక్ రీల్ స్పీడ్
    ├── MainMenuPatch.cs               -> టైటిల్ & పాజ్ మెనూ ఇంజెక్షన్
    ├── CursorPatch.cs                 -> కర్సర్ అన్‌లాక్ మేనేజ్‌మెంట్
    └── GameManagerPatch.cs            -> గేమ్‌మేనేజర్ టిక్ హుక్
```

---

## 🛠️ 4. బిల్డ్ & ప్యాకేజింగ్ కమాండ్స్

### ప్రాజెక్ట్‌ను కంపైల్ చేయడం (0 Warnings, 0 Errors)
```powershell
cd "d:\SteamLibrary\steamapps\common\Raft\ModSource\SailorsCompanion"
dotnet build SailorsCompanion.csproj -c Release
```

### ఆటో-సింక్ & జిప్ ప్యాకేజ్ క్రియేషన్
```powershell
Copy-Item "d:\SteamLibrary\steamapps\common\Raft\BepInEx\plugins\SailorsCompanion\SailorsCompanion.dll" "d:\SteamLibrary\steamapps\common\Raft\mods\SailorsCompanion\SailorsCompanion.dll" -Force
Copy-Item "d:\SteamLibrary\steamapps\common\Raft\BepInEx\plugins\SailorsCompanion\SailorsCompanion.dll" "Release\SailorsCompanion.dll" -Force
Compress-Archive -Path "Release\SailorsCompanion.dll", "modinfo.json", "README.md" -DestinationPath "Release\SailorsCompanion_BepInEx_v1.1.0.zip" -Force
```

### గిట్‌హబ్‌లోకి కమిట్ & పుష్ చేయడం
```powershell
git add .
git commit -m "Update v1.1.0 release files"
git tag -f v1.1.0
git push origin main --tags -f
```

---

## 📜 లైసెన్స్
ఈ ప్రాజెక్ట్ **[MIT License](LICENSE)** క్రింద ఓపెన్ సోర్స్‌గా అందించబడింది.
