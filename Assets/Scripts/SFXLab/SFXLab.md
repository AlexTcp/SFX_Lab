# SFX Lab

A standalone Unity scene for designing and auditioning `SFXEmitter` sounds interactively. Reuses the game's runtime synth verbatim, so what you hear in the lab is what you get in-game.

## Goal

Build a dedicated tool that can be packaged into its own executable — including a mobile build that runs on a phone — without carrying the rest of the game along with it. The tool:

- Loads any preset from `SFXExamples` into per-layer sliders.
- Lets you tweak all 18 `WaveEnvelope` parameters per wave layer (Sine, Square, Saw, Triangle, Pulse, Noise).
- Auditions the current parameter state via the same `SFXManager` the game uses.
- Exports the current state as pasteable C# (`SFXManager.Instance.Emit(...)` tuple syntax).
- Loops playback at a user-adjustable interval so you can iterate on a sound while dragging sliders.

## Files

```
SFXLab/
├── SFXLab.md                       ← this file
├── Runtime/
│   ├── SFXLabParamRanges.cs        ← static min/max/default table for each WaveEnvelope field
│   ├── SFXLabSlider.cs             ← compact slider row: label + slider + value text
│   ├── SFXLabLayerPanel.cs         ← one per WaveLayer; owns an enable toggle + 18 sliders
│   └── SFXLabController.cs         ← scene root; playback, preset loading, export
└── Editor/
    └── SFXLabSceneBuilder.cs       ← [MenuItem("Tools/SFX Lab/Build Scene")]
```

## Setup

1. **Import TMP Essentials once**: Window → TextMeshPro → Import TMP Essential Resources. Only needed on a fresh project; without it every label renders blank and the console logs an error.
2. Create an empty scene, add an empty GameObject, attach `SFXLabController`.

That's the whole setup — the controller builds the entire UI at runtime in `Awake`, including the `Canvas`, `EventSystem` (uses `InputSystemUIInputModule` when the Input System package is enabled, otherwise `StandaloneInputModule`), `SFXManager`, and an `AudioListener` if the scene doesn't already have one.

No editor menu item, no pre-wired prefab references — edit `SFXLabController.cs`, hit Play.

## Scene layout

The canvas splits into a scrollable top area and a sticky bottom bar. Portrait 1080×1920 reference resolution with width-match scaling, wide slider handles (55px), and ≥80px touch targets across all controls.

**Sticky bottom bar** (always visible):

- **Play** — big primary button.
- **Loop** — toggle. While on, replays the current patch every N seconds (N = Loop Interval in the Playback section).
- **Stop** — releases every active emitter immediately and turns Loop off.

**Scrollable content** (top-to-bottom):

- **Presets** — dropdown (built-ins alphabetical, then user presets prefixed with ★), variation slider (0–24), Load Preset + Delete buttons. Delete is disabled unless a user preset is selected.
- **Playback** — Loop interval slider (0.05–2s).
- **Export** — output folder field, Export + Clear buttons, preset name field + Save button, status line.
- **Layers** — six collapsible layer panels (Sine/Square/Saw/Triangle/Pulse/Noise). Each panel:
    - Header row with enable toggle, layer name, and collapse arrow — tap the name area to toggle.
    - Four collapsible category groups inside: **Core** (cyan), **Modulation** (magenta), **Timbre** (amber), **Variance** (green). Core is expanded by default; the others start collapsed.
    - Long-press any slider (≈0.5s, no drag) to reset that param to its default.

**Preset load behavior**: collapses every panel, then expands and populates the panels the preset uses.

**User presets**: the Save button stores the current state under the name in the text field. User presets persist across sessions via `PlayerPrefs` (key `SFXLab.UserPresets`) and appear in the dropdown with a `★ ` prefix.

## Runtime flow

- `Awake`: `SFXLabController` ensures an `EventSystem`/`SFXManager`/`AudioListener` exist, builds the full UI tree under a new `Canvas`, loads `SFXExamples.GetExamples()`, populates the preset dropdown, sets default ranges on the variation / loop-interval sliders, defaults the output folder to `Application.persistentDataPath`, and wires button click listeners. It also calls `Setup(layer)` on each `SFXLabLayerPanel`, which pushes each field's default value into its slider and registers a callback that writes slider changes back into the panel's cached `WaveEnvelope`.
- **Play**: builds a `(WaveLayer, WaveEnvelope)[]` from every panel whose enable toggle is on, calls `SFXManager.Instance.Emit(...)`.
- **Loop toggle**: `Update` calls `Play()` every N seconds while checked.
- **Load Preset**: zeroes all panels, then pushes the selected preset's layer values into the matching panels and flips their enable toggles on.
- **Export**: builds a C# string of the current emit call, copies to `GUIUtility.systemCopyBuffer`, and writes a timestamped `.txt` file to the output folder if it's writable.
- **Clear**: resets every panel to defaults and turns all enable toggles off.

## Building a phone executable

The lab is a normal Unity scene in the main project, so no separate project is needed.

1. File → Build Settings.
2. Add your scene (the one with the `SFXLabController` GameObject) to Scenes In Build; remove anything else.
3. Target platform: **Android** (or **iOS**). Enable Development Build if you want the clipboard / file paths to show in logcat.
4. Player Settings → Resolution → default orientation **Portrait**.
5. Build. Install the APK on the phone.

Notes for mobile:

- `Application.persistentDataPath` is the safest default output folder (always writable, sandboxed per app). The lab pre-fills the folder field with this path; override it in the UI if you want writes to go elsewhere (e.g. `/storage/emulated/0/Download/` on Android — may require additional Unity permissions).
- `GUIUtility.systemCopyBuffer` works on Android and iOS. You can paste into Messages / email / etc. to move the exported code off-device.
- No keyboard shortcuts — all controls are on-screen toggles / buttons / sliders.

## Parameter ranges

Defined in `SFXLabParamRanges.cs`. Ranges mirror the `VaryParam` bounds used by `SFXExamples.GenerateVariations`, so the lab's design space matches the variation space the game already generates. Defaults equal `WaveEnvelope`'s constructor defaults, so a freshly-cleared panel produces silence until you raise `intensity`.

## Exports

Exported code shape matches the game's call convention:

```csharp
SFXManager.Instance.Emit(
    (WaveLayer.Noise,    new(intensity: 0.8f, decayRate: 0.96f, filterCutoff: 0.5f)),
    (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: -200f)),
    (WaveLayer.Saw,      new(intensity: 0.6f)));
```

Only non-default fields are emitted, matching the concise style of existing `SFXExamples` entries. The export format is chosen so the output can be pasted directly into a caller or added as a new entry in `SFXExamples.FullList()` (wrap with `["MyKey"] = new (WaveLayer, WaveEnvelope)[] { ... }`).

## v2: Clip layer (deferred)

Clip playback (the `ClipEnvelope` layer) was intentionally **skipped** for the first version because:

- Adding a seventh panel would take the UI from "busy but scannable" to "crowded" on a phone screen.
- Clip selection needs a file picker / `AudioClip` reference slot, which is a different UX problem than slider tweaking.
- Clip effects overlap heavily with wave effects (same filter, tremolo, vibrato, bit crush) — you'd want to share rows, which means a richer layout system than the current one-panel-per-thing.

When it's added:

- Add a seventh panel at the bottom of the scroll area labelled "Clip".
- Add an `AudioClip` picker. On mobile this is non-trivial: options are (a) an in-project `Resources/Clips/` browse list, (b) a file path text field that uses `WWW` / `UnityWebRequest` to load a WAV from the device, or (c) a bundled set of test clips loaded via `Resources.Load`. Option (c) is simplest for v2.
- Add clip-specific sliders: `playbackSpeed` (0.25–4), `loop` (toggle). Reuse the common filter/tremolo/vibrato/bitcrush/randomness/startDelay rows.
- Change Play to call `TriggerClip` directly on an emitter fetched from the pool, rather than going through `SFXManager.Emit` (which doesn't have a clip overload today). Or extend `SFXManager` with an `Emit(AudioClip, ClipEnvelope, ...)` overload.
- Export: serialize the clip reference by name (`Resources.Load<AudioClip>("Clips/X")`) so the generated code is standalone.

## Known rough edges

- UI uses legacy `UnityEngine.UI` (not TextMeshPro). Text rendering is sharper with TMP but would require hand-rolling the widget hierarchy or depending on the TMP extras package.
- The programmatic UI is spartan. Colors/spacing/sizes are the `Color*` and size constants at the top of `SFXLabController.cs` — edit there, hit Play to see changes.
- Corner roundness is controlled by `LargeSpriteSize`/`LargeCornerRadius` (panels) and `SmallSpriteSize`/`SmallCornerRadius` (buttons/toggles/sliders) at the top of `SFXLabController.cs`. Both radii are in native pixels and persist at that size regardless of widget scale (9-slice).
- No undo / history. "Clear" is destructive. If you've dialed in a sound and want to keep iterating, hit Export first so you can paste it back.
- Variation slider scrubs 0–24 regardless of how many variations a given preset actually has (always 25 from `GenerateVariations`). If `SFXExamples` ever produces variable-length variation arrays, clamp the slider's max to `presets[key].Length - 1` on preset change.

## Extending

- **New `WaveEnvelope` field**: add to `SFXLabParamRanges.FieldOrder` and `Ranges`, update `SFXLabController.FormatEnvelope` so the export skips the default value. The slider row appears automatically on next Play (no editor step).
- **Different layout (tabs, grid)**: edit `BuildUI` / `BuildControls` / `BuildLayerPanels` in `SFXLabController.cs`. The current design uses `VerticalLayoutGroup` + `ScrollRect`; a tabbed version would swap the scroll view for a tab container with one panel visible at a time.
- **Different export format (JSON, ScriptableObject)**: change `SFXLabController.BuildCodeString` to emit whatever shape you want. The current `(WaveLayer, WaveEnvelope)` input is easy to serialize in any direction.

---

## Context for AI agents

Read this if you're an AI asked to modify or extend the lab.

### Mental model

The lab is a **dev tool that lives in the game project but is not part of the shipped game**. It reuses `SFXEmitter` / `SFXManager` verbatim, so any DSP change in the game is automatically reflected in the lab — there is no parallel implementation. The lab's only job is to drive the existing emitter with UI-sourced parameters and render exports that paste back into game code.

Three runtime responsibilities, all in `SFXLabController`:
1. Bridge UI → `(WaveLayer, WaveEnvelope)[]` → `SFXManager.Instance.Emit(...)`.
2. Bridge `SFXExamples.GetExamples()` → sliders (preset load).
3. Bridge current slider state → concise C# string (export).

### Entry points to read first

- `SFXEmitter.md` (parent dir) — authoritative spec for `WaveEnvelope`, `WaveLayer`, the Option-B tuple API, and the audio-thread model. **Read this before the lab code.**
- `SFXLabController.cs` — all orchestration *and* UI construction is here. If you want to understand the lab, this is the spine. The UI is built in `BuildUI` / `BuildControls` / `BuildLayerPanels`; the gameplay logic is `Play` / `LoadPreset` / `Export` / `Clear`.

### Hidden coupling (change one, update others)

| Change | Also update |
|---|---|
| Add a field to `WaveEnvelope` | `SFXLabParamRanges.FieldOrder` (order matters — drives slider layout), `SFXLabParamRanges.Ranges` (min/max/default), `SFXLabController.FormatEnvelope` (skip default in export). The slider row appears automatically on next Play. |
| Add a value to `WaveLayer` enum | `SFXEmitter.cs` switch statements in `TriggerSounds` / `TriggerSustainedSounds` (add case). `SFXLabController.BuildLayerPanels` picks up the new value automatically via `Enum.GetValues`. |
| Change `SFXExamples.GetExamples()` return type | `SFXLabController.presets` field type, `LoadPreset` tuple destructuring. |
| Change `SFXManager.Emit` signature | `SFXLabController.Play` call site. |

### Invariants to preserve

- **`SFXLabParamRanges.FieldOrder` must match `WaveEnvelope` field names exactly** (case-sensitive). Reflection in `SFXLabLayerPanel` looks them up with `FieldInfo.GetField(name, Public | Instance)`. A typo → silent null → NullReferenceException at slider init time.
- **`SFXLabLayerPanel.Sliders[]` is index-aligned to `FieldOrder`.** `BuildSliderRow` iterates `FieldOrder` in order; the runtime assumes this. Don't reorder one without the other.
- **`WaveLayer.Sine == 0`** and the enum order matches `WaveEnvelope`'s constructor parameter order. Not directly load-bearing in the lab, but preserve for export-code readability consistency with `SFXExamples`.
- **`SFXManager` in the scene is required** — its `Awake` does `Resources.Load<SFXEmitter>("Prefabs/SFX_Emitter")` and builds the pool. The controller calls `_ = SFXManager.Instance` in `Awake` so one gets created if missing — but the prefab must exist at `Resources/Prefabs/SFX_Emitter.prefab`.

### What the lab is *not*

- **Not TMP-based.** Uses `UnityEngine.UI` (legacy UGUI). Do not convert to TextMeshPro without a reason — you would have to rewrite every widget builder.
- **Not aware of the Clip layer.** `ClipEnvelope` / `TriggerClip` are intentionally out of scope. If someone asks to add clip support, follow the v2 plan above — do not shoehorn it into a seventh `SFXLabLayerPanel` because `WaveEnvelope` and `ClipEnvelope` don't share a base type.
- **Not using an editor menu item anymore.** Earlier versions built the scene via `Tools/SFX Lab/Build Scene`; that's gone — the controller constructs the UI at runtime. No scene asset is authoritative; source of truth is `SFXLabController.cs`.

### Files and where they compile

- `SFXLab/Runtime/*.cs` → Assembly-CSharp (shipped in builds, available at runtime). Six files:
    - `SFXLabController.cs` — entry point, builds UI, owns all actions and user-preset persistence.
    - `SFXLabLayerPanel.cs` — per-layer state (enable toggle + 18 sliders + collapse refs).
    - `SFXLabSlider.cs` — per-param slider row (label + slider + value text + default value).
    - `SFXLabCollapsible.cs` — generic collapse/expand helper used for panels and category groups.
    - `SFXLabLongPress.cs` — reusable ≈0.5s hold detector with drag-cancel.
    - `SFXLabParamRanges.cs` — static tables: field order, categories, min/max/default per param.

### Debugging checklist

- **Play does nothing / silence**: check `SFXManager.Instance` is non-null (controller creates one in `Awake`, but `Resources.Load<SFXEmitter>("Prefabs/SFX_Emitter")` must resolve — confirm the prefab is at that path). Check at least one layer's enable toggle is on and that layer's `intensity` slider is > 0.
- **"You are trying to read Input using the UnityEngine.Input class"**: the scene's EventSystem has a legacy `StandaloneInputModule` but the project uses the new Input System only. The controller creates `InputSystemUIInputModule` when `ENABLE_INPUT_SYSTEM` is defined, but if you pre-created an EventSystem manually with the legacy module, delete it before Play.
- **Sliders at zero but previous preset still audible**: `Clear` only resets sliders and toggles, it doesn't stop currently-playing emitters. They decay naturally.
- **Export file missing**: folder field must be a writable directory. `Directory.CreateDirectory` is called, but a malformed path (on desktop) or a permission-restricted path (on Android without MANAGE_EXTERNAL_STORAGE) will throw and fall back to clipboard-only. Status text reports the error.
- **Compile errors after editing `WaveEnvelope`**: the lab uses reflection by field *name*, not position. Adding a field is safe; renaming one requires updating `SFXLabParamRanges.FieldOrder`.

### When extending, think about

- Whether the UI change is mobile-friendly → target touch hit sizes (≥60px; the current build uses ≥70px for sliders and ≥80px for buttons/toggles), vertical layout, no hover-only interactions.
- Whether new export format is still a valid paste into `SFXExamples.FullList()` or a direct `Emit` call → preserve the tuple shape.

