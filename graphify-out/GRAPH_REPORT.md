# Graph Report - .  (2026-08-30)

## Corpus Check
- 179 files · ~111,802 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1138 nodes · 1592 edges · 119 communities (81 shown, 38 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 9 edges (avg confidence: 0.8)
- Token cost: 1,200 input · 450 output

## Community Hubs (Navigation)
- AI NPC Client & LLM Network
- UI Button Effects & Interactions
- Unity Burst & Low-Level Runtime
- Camera & Scene Navigation
- Procedural City Generator
- HDRP & Visual Effects Pipeline
- Dialogue & Chat Interface
- Module Subsystem 7
- Dialogue & Chat Interface
- AI NPC Client & LLM Network
- Crime Scene & Clue Investigation
- Module Subsystem 11
- Module Subsystem 12
- UI Button Effects & Interactions
- Module Subsystem 14
- Crime Scene & Clue Investigation
- TextMeshPro Text Rendering
- Module Subsystem 17
- Module Subsystem 18
- Camera & Scene Navigation
- Module Subsystem 20
- Module Subsystem 21
- Module Subsystem 22
- Camera & Scene Navigation
- Module Subsystem 24
- Module Subsystem 25
- Module Subsystem 26
- Module Subsystem 27
- Module Subsystem 28
- Room & Interrogation Scene Builder
- TextMeshPro Text Rendering
- TextMeshPro Text Rendering
- Module Subsystem 32
- Module Subsystem 33
- Module Subsystem 34
- Module Subsystem 35
- Module Subsystem 36
- Module Subsystem 37
- Module Subsystem 38
- Module Subsystem 39
- Module Subsystem 40
- Module Subsystem 41
- Module Subsystem 42
- TextMeshPro Text Rendering
- Module Subsystem 44
- Module Subsystem 45
- Module Subsystem 46
- TextMeshPro Text Rendering
- Module Subsystem 48
- Module Subsystem 49
- Module Subsystem 50
- Module Subsystem 51
- Module Subsystem 52
- Module Subsystem 53
- Module Subsystem 54
- Module Subsystem 55
- TextMeshPro Text Rendering
- Module Subsystem 57
- Module Subsystem 58
- Module Subsystem 59
- Module Subsystem 60
- Module Subsystem 61
- Module Subsystem 62
- Module Subsystem 63
- Module Subsystem 64
- Module Subsystem 65
- Module Subsystem 66
- Module Subsystem 67
- TextMeshPro Text Rendering
- Module Subsystem 69
- Module Subsystem 70
- Module Subsystem 71
- Module Subsystem 72
- Module Subsystem 73
- Module Subsystem 74
- Module Subsystem 75
- Module Subsystem 76
- Module Subsystem 77
- Module Subsystem 78
- Module Subsystem 79
- Module Subsystem 80
- Module Subsystem 81
- Module Subsystem 82
- Module Subsystem 83
- Module Subsystem 84
- Module Subsystem 85
- Module Subsystem 86
- Module Subsystem 87
- Module Subsystem 88
- Module Subsystem 89
- Module Subsystem 90
- Module Subsystem 91
- Module Subsystem 92
- Module Subsystem 93
- Module Subsystem 94
- Module Subsystem 95
- Module Subsystem 96
- Module Subsystem 97
- HDRP & Visual Effects Pipeline
- Module Subsystem 99
- Module Subsystem 100
- Module Subsystem 101
- Module Subsystem 102
- Module Subsystem 103
- Module Subsystem 104
- Module Subsystem 105
- Module Subsystem 106
- Module Subsystem 107
- Module Subsystem 108
- Module Subsystem 109
- Module Subsystem 110
- Module Subsystem 111
- Module Subsystem 112
- Module Subsystem 113
- Module Subsystem 114
- Module Subsystem 115
- Module Subsystem 116
- Module Subsystem 117
- Module Subsystem 118

## God Nodes (most connected - your core abstractions)
1. `ChatUI` - 35 edges
2. `CityBuilder` - 30 edges
3. `TMPro.Examples` - 28 edges
4. `OfficeRoomBuilder` - 25 edges
5. `TMP_TextSelector_B` - 25 edges
6. `TMP_TextEventHandler` - 22 edges
7. `InterrogationCameraController` - 20 edges
8. `TextMeshProFloatingText` - 19 edges
9. `FirstPerson` - 16 edges
10. `TKPZone` - 16 edges

## Surprising Connections (you probably didn't know these)
- `AIChatClient` --references--> `AIDataLoader`  [EXTRACTED]
  Assets/Script/AIChatClient.cs → Assets/Script/AIDataLoader.cs
- `EvidenceManager` --references--> `AIChatClient`  [EXTRACTED]
  Assets/Script/EvidenceManager.cs → Assets/Script/AIChatClient.cs
- `ChatUI` --references--> `FirstPerson`  [EXTRACTED]
  Assets/Script/ChatUI.cs → Assets/Script/FirstPerson.cs
- `NPCInteraction` --references--> `ChatUI`  [EXTRACTED]
  Assets/Script/NPCInteraction.cs → Assets/Script/ChatUI.cs
- `NPCInteraction` --references--> `FirstPerson`  [EXTRACTED]
  Assets/Script/NPCInteraction.cs → Assets/Script/FirstPerson.cs

## Import Cycles
- None detected.

## Communities (119 total, 38 thin omitted)

### Community 0 - "AI NPC Client & LLM Network"
Cohesion: 0.06
Nodes (23): Action, AIChatClient, IEnumerator, ChatUI, bool, float, GameObject, IEnumerator (+15 more)

### Community 1 - "UI Button Effects & Interactions"
Cohesion: 0.05
Nodes (27): ButtonHoverEffect, float, PointerEventData, Vector3, bool, Camera, int, PointerEventData (+19 more)

### Community 2 - "Unity Burst & Low-Level Runtime"
Cohesion: 0.06
Nodes (39): com.unity.burst, com.unity.collections, com.unity.mathematics, com.unity.nuget.mono-cecil, dependencies, depth, source, url (+31 more)

### Community 3 - "Camera & Scene Navigation"
Cohesion: 0.07
Nodes (23): Camera, float, int, string, TextMeshPro, Transform, FpsCounterAnchorPositions, TMP_FrameRateCounter (+15 more)

### Community 4 - "Procedural City Generator"
Cohesion: 0.16
Nodes (15): CityBuilder, CityPreset, bool, float, GameObject, int, List, MenuItem (+7 more)

### Community 5 - "HDRP & Visual Effects Pipeline"
Cohesion: 0.06
Nodes (35): com.unity.render-pipelines.core, com.unity.render-pipelines.high-definition-config, com.unity.searcher, com.unity.shadergraph, com.unity.visualeffectgraph, depth, source, version (+27 more)

### Community 6 - "Dialogue & Chat Interface"
Cohesion: 0.16
Nodes (12): bool, Color, GameObject, Material, MenuItem, Quaternion, string, Transform (+4 more)

### Community 7 - "Module Subsystem 7"
Cohesion: 0.07
Nodes (30): dependencies, depth, source, version, dependencies, depth, source, version (+22 more)

### Community 8 - "Dialogue & Chat Interface"
Cohesion: 0.11
Nodes (13): Color, GameObject, Material, MenuItem, string, Transform, InterrogationRoomBuilder, bool (+5 more)

### Community 9 - "AI NPC Client & LLM Network"
Cohesion: 0.12
Nodes (18): NPC Profiles (Database Karakter), Prompt Template (Roleplay NPC Engine), World Lore (Latar Kasus & Tersangka), AIRequest, AIResponse, bool, float, int (+10 more)

### Community 10 - "Crime Scene & Clue Investigation"
Cohesion: 0.11
Nodes (9): List, Text, TKPManager, bool, GameObject, string, TKPZone, BoxCollider (+1 more)

### Community 11 - "Module Subsystem 11"
Cohesion: 0.08
Nodes (24): dependencies, depth, source, version, dependencies, depth, source, version (+16 more)

### Community 12 - "Module Subsystem 12"
Cohesion: 0.12
Nodes (13): float, string, ReadmeEditor, bool, string, Readme, Section, Editor (+5 more)

### Community 13 - "UI Button Effects & Interactions"
Cohesion: 0.13
Nodes (11): Camera, Canvas, int, PointerEventData, TMP_Text, TMP_TextEventHandler, CharacterSelectionEvent, LineSelectionEvent (+3 more)

### Community 14 - "Module Subsystem 14"
Cohesion: 0.18
Nodes (8): Color, GameObject, MenuItem, Transform, Vector2, MainMenuBuilder, EditorWindow, Func

### Community 15 - "Crime Scene & Clue Investigation"
Cohesion: 0.16
Nodes (9): bool, string, EvidenceItem, List, EvidenceManager, float, Transform, NPCInteraction (+1 more)

### Community 16 - "TextMeshPro Text Rendering"
Cohesion: 0.14
Nodes (13): bool, Font, GameObject, IEnumerator, int, Quaternion, TextMesh, TextMeshPro (+5 more)

### Community 17 - "Module Subsystem 17"
Cohesion: 0.31
Nodes (15): Color, Vector3, DrawBounds(), DrawCharactersBounds(), DrawCrosshair(), DrawDottedRectangle(), DrawLineBounds(), DrawLinkBounds() (+7 more)

### Community 18 - "Module Subsystem 18"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, url, version, depth, source, version (+8 more)

### Community 19 - "Camera & Scene Navigation"
Cohesion: 0.21
Nodes (7): Animator, Camera, int, Material, Text, AnimationController, DavidJalbert.LowPolyPeople

### Community 20 - "Module Subsystem 20"
Cohesion: 0.16
Nodes (7): bool, float, IEnumerator, Object, TMP_Text, VertexAnim, VertexJitter

### Community 21 - "Module Subsystem 21"
Cohesion: 0.14
Nodes (14): com.unity.settings-manager, com.unity.testtools.codecoverage, dependencies, depth, source, url, version, dependencies (+6 more)

### Community 22 - "Module Subsystem 22"
Cohesion: 0.19
Nodes (7): bool, IEnumerator, string, TMP_Text, MainMenuManager, Button, CanvasGroup

### Community 23 - "Camera & Scene Navigation"
Cohesion: 0.17
Nodes (8): bool, float, string, Transform, Vector3, CameraController, CameraModes, CameraModes

### Community 24 - "Module Subsystem 24"
Cohesion: 0.21
Nodes (5): bool, IEnumerator, Object, TMP_Text, TextConsoleSimulator

### Community 25 - "Module Subsystem 25"
Cohesion: 0.17
Nodes (6): bool, float, IEnumerator, Object, TMP_Text, VertexShakeA

### Community 26 - "Module Subsystem 26"
Cohesion: 0.17
Nodes (6): bool, float, IEnumerator, Object, TMP_Text, VertexShakeB

### Community 27 - "Module Subsystem 27"
Cohesion: 0.17
Nodes (6): bool, float, IEnumerator, Object, TMP_Text, VertexZoom

### Community 28 - "Module Subsystem 28"
Cohesion: 0.15
Nodes (13): com.unity.ext.nunit, com.unity.ide.rider, dependencies, depth, source, version, dependencies, depth (+5 more)

### Community 29 - "Room & Interrogation Scene Builder"
Cohesion: 0.18
Nodes (4): TMP_DigitValidator, TMP_PhoneNumberValidator, TMPro, TMP_InputValidator

### Community 30 - "TextMeshPro Text Rendering"
Cohesion: 0.17
Nodes (10): Font, IEnumerator, int, Material, string, TextContainer, TextMesh, TextMeshPro (+2 more)

### Community 31 - "TextMeshPro Text Rendering"
Cohesion: 0.17
Nodes (10): Canvas, Font, IEnumerator, int, Material, string, Text, TextMeshProUGUI (+2 more)

### Community 32 - "Module Subsystem 32"
Cohesion: 0.17
Nodes (12): com.unity.editorcoroutines, dependencies, depth, source, url, version, dependencies, depth (+4 more)

### Community 33 - "Module Subsystem 33"
Cohesion: 0.17
Nodes (11): com.unity.modules.screencapture, com.unity.modules.vectorgraphics, com.unity.modules.vr, dependencies, com.unity.modules.screencapture, com.unity.modules.unitywebrequestassetbundle, com.unity.modules.unitywebrequestaudio, com.unity.modules.vectorgraphics (+3 more)

### Community 34 - "Module Subsystem 34"
Cohesion: 0.17
Nodes (12): com.unity.test-framework, com.unity.test-framework.performance, depth, dependencies, depth, source, url, version (+4 more)

### Community 35 - "Module Subsystem 35"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, version (+4 more)

### Community 36 - "Module Subsystem 36"
Cohesion: 0.18
Nodes (8): float, int, Transform, Vector3, MotionType, ObjectSpin, Color32, MotionType

### Community 37 - "Module Subsystem 37"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 38 - "Module Subsystem 38"
Cohesion: 0.22
Nodes (6): AnimationCurve, float, IEnumerator, Material, ShaderPropAnimator, Renderer

### Community 39 - "Module Subsystem 39"
Cohesion: 0.27
Nodes (5): AnimationCurve, float, IEnumerator, TMP_Text, SkewTextExample

### Community 40 - "Module Subsystem 40"
Cohesion: 0.20
Nodes (7): bool, int, string, TMP_Text, objectType, TMP_ExampleScript_01, objectType

### Community 41 - "Module Subsystem 41"
Cohesion: 0.20
Nodes (3): TMP_Text, TMP_TextEventCheck, TMP_TextEventHandler

### Community 42 - "Module Subsystem 42"
Cohesion: 0.27
Nodes (5): AnimationCurve, float, IEnumerator, TMP_Text, WarpTextExample

### Community 43 - "TextMeshPro Text Rendering"
Cohesion: 0.22
Nodes (4): int, Transform, Benchmark04, TMPro.Examples

### Community 44 - "Module Subsystem 44"
Cohesion: 0.25
Nodes (5): Font, int, Benchmark03, BenchmarkType, BenchmarkType

### Community 45 - "Module Subsystem 45"
Cohesion: 0.25
Nodes (4): ChatController, TMP_InputField, TMP_Text, Scrollbar

### Community 46 - "Module Subsystem 46"
Cohesion: 0.25
Nodes (5): IEnumerator, Material, TMP_Text, Vector3, EnvMapAnimator

### Community 47 - "TextMeshPro Text Rendering"
Cohesion: 0.29
Nodes (4): float, string, TextMeshPro, SimpleScript

### Community 48 - "Module Subsystem 48"
Cohesion: 0.29
Nodes (4): IEnumerator, string, TMP_Text, TeleType

### Community 49 - "Module Subsystem 49"
Cohesion: 0.48
Nodes (6): CharacterSelectionEvent, LineSelectionEvent, LinkSelectionEvent, SpriteSelectionEvent, WordSelectionEvent, UnityEvent

### Community 50 - "Module Subsystem 50"
Cohesion: 0.33
Nodes (3): IEnumerator, TMP_Text, VertexColorCycler

### Community 51 - "Module Subsystem 51"
Cohesion: 0.29
Nodes (7): com.unity.ide.visualstudio, dependencies, depth, source, url, version, com.unity.ide.visualstudio

### Community 52 - "Module Subsystem 52"
Cohesion: 0.29
Nodes (7): com.unity.modules.hierarchycore, dependencies, depth, source, version, dependencies, com.unity.modules.hierarchycore

### Community 53 - "Module Subsystem 53"
Cohesion: 0.29
Nodes (7): com.unity.performance.profile-analyzer, dependencies, depth, source, url, version, com.unity.performance.profile-analyzer

### Community 54 - "Module Subsystem 54"
Cohesion: 0.29
Nodes (6): dependencies, depth, source, version, dependencies, com.unity.modules.ai

### Community 55 - "Module Subsystem 55"
Cohesion: 0.53
Nodes (5): AIRequest, clean_reply(), generate_text(), parse_9router_response(), BaseModel

### Community 56 - "TextMeshPro Text Rendering"
Cohesion: 0.33
Nodes (3): Font, int, TextMeshSpawner

### Community 57 - "Module Subsystem 57"
Cohesion: 0.33
Nodes (6): com.unity.modules.subsystems, dependencies, depth, source, version, com.unity.modules.subsystems

### Community 58 - "Module Subsystem 58"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 59 - "Module Subsystem 59"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.assetbundle, com.unity.modules.assetbundle

### Community 60 - "Module Subsystem 60"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.imageconversion, com.unity.modules.imageconversion

### Community 61 - "Module Subsystem 61"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.jsonserialize, com.unity.modules.jsonserialize

### Community 62 - "Module Subsystem 62"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.physics, com.unity.modules.physics

### Community 63 - "Module Subsystem 63"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.unitywebrequest, com.unity.modules.unitywebrequest

### Community 64 - "Module Subsystem 64"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.unitywebrequestassetbundle, com.unity.modules.unitywebrequestassetbundle

### Community 65 - "Module Subsystem 65"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.unitywebrequestaudio, com.unity.modules.unitywebrequestaudio

### Community 66 - "Module Subsystem 66"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, version, com.unity.modules.xr, com.unity.modules.xr

### Community 67 - "Module Subsystem 67"
Cohesion: 0.40
Nodes (3): bool, int, Benchmark02

### Community 68 - "TextMeshPro Text Rendering"
Cohesion: 0.40
Nodes (3): TextMeshProUGUI, DropdownSample, TMP_Dropdown

### Community 69 - "Module Subsystem 69"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.accessibility

### Community 70 - "Module Subsystem 70"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.adaptiveperformance

### Community 71 - "Module Subsystem 71"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.androidjni

### Community 72 - "Module Subsystem 72"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.cloth

### Community 73 - "Module Subsystem 73"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.screencapture

### Community 74 - "Module Subsystem 74"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.umbra

### Community 75 - "Module Subsystem 75"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unityanalytics

### Community 76 - "Module Subsystem 76"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequesttexture

### Community 77 - "Module Subsystem 77"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestwww

### Community 78 - "Module Subsystem 78"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.vehicles

### Community 79 - "Module Subsystem 79"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.vr

### Community 80 - "Module Subsystem 80"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

## Knowledge Gaps
- **321 isolated node(s):** `DavidJalbert.LowPolyPeople`, `CityPreset`, `BenchmarkType`, `CameraModes`, `MotionType` (+316 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **38 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `dependencies` connect `Module Subsystem 54` to `Unity Burst & Low-Level Runtime`, `HDRP & Visual Effects Pipeline`, `Module Subsystem 7`, `Module Subsystem 11`, `Module Subsystem 18`, `Module Subsystem 21`, `Module Subsystem 28`, `Module Subsystem 32`, `Module Subsystem 34`, `Module Subsystem 35`, `Module Subsystem 37`, `Module Subsystem 51`, `Module Subsystem 52`, `Module Subsystem 53`, `Module Subsystem 57`, `Module Subsystem 58`, `Module Subsystem 59`, `Module Subsystem 60`, `Module Subsystem 61`, `Module Subsystem 62`, `Module Subsystem 63`, `Module Subsystem 64`, `Module Subsystem 65`, `Module Subsystem 66`, `Module Subsystem 69`, `Module Subsystem 70`, `Module Subsystem 71`, `Module Subsystem 72`, `Module Subsystem 73`, `Module Subsystem 74`, `Module Subsystem 75`, `Module Subsystem 76`, `Module Subsystem 77`, `Module Subsystem 78`, `Module Subsystem 79`, `Module Subsystem 80`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `ChatUI` connect `AI NPC Client & LLM Network` to `Dialogue & Chat Interface`, `AI NPC Client & LLM Network`, `Dialogue & Chat Interface`, `Crime Scene & Clue Investigation`?**
  _High betweenness centrality (0.068) - this node is a cross-community bridge._
- **Why does `AIChatClient` connect `AI NPC Client & LLM Network` to `Dialogue & Chat Interface`, `AI NPC Client & LLM Network`, `Dialogue & Chat Interface`, `Crime Scene & Clue Investigation`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **What connects `DavidJalbert.LowPolyPeople`, `CityPreset`, `BenchmarkType` to the rest of the system?**
  _321 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `AI NPC Client & LLM Network` be split into smaller, more focused modules?**
  _Cohesion score 0.0647307924984876 - nodes in this community are weakly interconnected._
- **Should `UI Button Effects & Interactions` be split into smaller, more focused modules?**
  _Cohesion score 0.050170068027210885 - nodes in this community are weakly interconnected._
- **Should `Unity Burst & Low-Level Runtime` be split into smaller, more focused modules?**
  _Cohesion score 0.0553306342780027 - nodes in this community are weakly interconnected._