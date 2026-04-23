# SFXEmitter — Procedural Sound Effect Generator

`SFXEmitter` is a Unity `MonoBehaviour` that generates sound effects in real time on the audio thread. It supports two modes that can be used independently or combined on the same emitter:

1. **Procedural synthesis** — generate waveforms from scratch (sine, square, saw, triangle, pulse, noise).
2. **AudioClip processing** — play an existing `AudioClip` through the same effects chain (pitch shift, filter, bit crush, tremolo, vibrato, start delay, randomization).

## Architecture Overview

The system has three data classes and one MonoBehaviour:

1. **`WaveEnvelope`** — parameters and runtime state for a single synthesized waveform layer.
2. **`ClipEnvelope`** — parameters and runtime state for an AudioClip layer (cached sample data + effects).
3. **`SFXEmitter`** — the MonoBehaviour that owns six `WaveEnvelope` instances (one per wave type), one `ClipEnvelope`, and drives all audio in `OnAudioFilterRead`.

Communication between the main thread and the audio thread uses `volatile` fields — no locks.

## WaveEnvelope

Each `WaveEnvelope` stores two categories of data.

### User Parameters (set from game code)

| Parameter | Description |
|---|---|
| `intensity` | Target amplitude (0 = silent) |
| `pitchBend` | Frequency drift in Hz/second |
| `decayRate` | Per-sample amplitude multiplier (< 1 = fade out, 1 = sustain) |
| `attackRate` | How fast amplitude ramps up to `intensity` |
| `vibratoSpeed` / `vibratoDepth` | LFO-based pitch wobble |
| `tremoloSpeed` / `tremoloDepth` | LFO-based amplitude wobble |
| `dutyCycle` | Pulse width for the Pulse waveform (0.1–0.9) |
| `harmonics` | Blend in 2nd and 3rd overtones (0–1) |
| `filterCutoff` / `filterResonance` | 2-pole low-pass filter |
| `bitCrush` | Bit-depth reduction for lo-fi effect (1–16, where 16 = no crush) |
| `fmAmount` / `fmRatio` | Frequency modulation synthesis |
| `pitchRandomness` / `ampRandomness` | Per-trigger random variation |
| `startDelay` | Delay before the wave begins producing sound |

### Runtime State (audio thread only)

`amp`, `freq`, `phase`, `lfoPhase`, `filterState1/2`, `randomPitchOffset`, `randomAmpMult`, `delayCounter` — all reset on each trigger via `Reset()`.

## Waveform Types

The emitter maintains six independent wave slots:

| Slot | Waveform | Generation |
|---|---|---|
| `sine` | Smooth sine | `FastSin(phase)` — 4096-entry LUT with linear interpolation |
| `square` | Hard-clipped square | `normalizedPhase < 0.5 ? 1 : -1` (no sine call) |
| `saw` | Sawtooth ramp | Linear ramp -1 to +1 |
| `triangle` | Triangle | Folded ramp |
| `pulse` | Variable-width pulse | Threshold against `dutyCycle` |
| `noise` | White noise | Xorshift-style PRNG |

All seven layers (six waves + one clip) are mixed additively each sample. Layers below the intensity threshold are skipped for the full buffer (see *Performance Optimizations*).

## ClipEnvelope

Holds cached AudioClip sample data and effects parameters for the clip layer.

### Clip-Specific Parameters

| Parameter | Description |
|---|---|
| `samples` | Mono float array, cached from AudioClip on main thread |
| `clipSampleRate` | Original sample rate of the clip |
| `clipSampleCount` | Total mono samples in the cached array |
| `playbackSpeed` | Speed multiplier (1.0 = normal, 2.0 = octave up) |
| `loop` | Whether to loop when reaching the end |

### Shared Effects Parameters

Same as `WaveEnvelope`: `intensity`, `pitchBend`, `decayRate`, `attackRate`, `vibratoSpeed/Depth`, `tremoloSpeed/Depth`, `filterCutoff/Resonance`, `bitCrush`, `pitchRandomness`, `ampRandomness`, `startDelay`.

### Runtime State

`amp`, `playbackPosition` (double for fractional-sample precision), `currentSpeed`, `lfoPhase`, `filterState1/2`, `randomSpeedMult`, `randomAmpMult`, `delayCounter`, `finished`.

## Lifecycle

### Triggering a Synthesized Sound

```
SFXManager → pool.Get() → emitter.Activate(intensityMod)
           → emitter.TriggerSounds(waveSettings)
```

1. **`Activate(intensityModifier)`** — Resets all waves to a 200 Hz base frequency, zeros all intensities and the clip layer, starts the `AudioSource`.
2. **`TriggerSounds(waves)`** — Copies parameters into the emitter's internal waves via `Copy()`. Intensity is set to `max(current, incoming * modifier)` so overlapping triggers don't cut each other off.
3. **`TriggerSustainedSounds(waves)`** — Same but forces `decayRate = 1.0` so the sound holds indefinitely until `Release()` is called.

Current call shape (tuple-based, Option B):

```csharp
SFXManager.Instance.Emit(
    (WaveLayer.Noise,    new(intensity: 0.8f, decayRate: 0.96f, filterCutoff: 0.5f)),
    (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: -200f, decayRate: 0.97f)),
    (WaveLayer.Saw,      new(intensity: 0.6f, pitchBend: 200f, decayRate: 0.98f, startDelay: 0.04f)));
```

For the sustained variants (`Emit(waves, duration)` and `EmitSustained(waves, ...)`), `params` can't be used because the duration / releaseTime arguments must follow the wave array. Callers pass an explicit array:

```csharp
SFXManager.Instance.Emit(new (WaveLayer, WaveEnvelope)[]
{
    (WaveLayer.Sine, new(intensity: 0.5f, ...)),
    (WaveLayer.Saw,  new(intensity: 0.3f, ...)),
}, duration: 1.5f, releaseTime: 0.3f);
```

### Triggering an AudioClip

```
SFXManager → pool.Get() → emitter.Activate(intensityMod)
           → emitter.TriggerClip(audioClip, clipSettings)
```

1. **`TriggerClip(clip, settings)`** — Caches the AudioClip's sample data into a mono float array (downmixed if stereo+), copies effect parameters, and starts playback. Can be called alongside `TriggerSounds()` on the same emitter to layer a clip with synthesis.
2. **`TriggerSustainedClip(clip, settings)`** — Same but forces `loop = true` and `decayRate = 1.0`, holding the sound until `Release()`.

```csharp
var settings = new ClipEnvelope(
    intensity: 0.8f,
    playbackSpeed: 1.2f,       // slightly pitched up
    filterCutoff: 0.4f,        // muffled
    bitCrush: 8f,              // lo-fi
    vibratoSpeed: 5f,          // wobble
    vibratoDepth: 0.05f,
    pitchRandomness: 0.1f      // slight random variation each trigger
);
emitter.TriggerClip(myAudioClip, settings);
```

### Sustain / Release

- `TriggerSustainedSounds()` / `TriggerSustainedClip()` set `sustainMode = true` and `decayRate = 1.0` (no decay).
- `Release(releaseTime)` calculates a per-sample decay rate from the release time and applies it to all waves and the clip layer, causing a smooth fade-out.

### Pause / Focus

- `Pause()` / `Unpause()` toggle the `isPaused` flag. When paused, `OnAudioFilterRead` outputs silence.
- `OnApplicationFocus` / `OnApplicationPause` set `appPaused`. Alt-tabbing or backgrounding the app silences emitters without individual management.

### Global Volume & Listener Sync

- `SFXManager.GlobalSFXVolume` (volatile static, 0–1) is snapshotted each frame by every active emitter into its `globalVolume` field and applied in the audio thread.
- `SFXManager.SetGlobalVolume(sliderValue)` applies a 2.2 power curve so a 0.5 UI slider sounds like half volume.
- `AudioListener.volume` and `AudioListener.pause` are also synced each frame — muting or pausing the listener silences all SFX.

### Auto-Reclaim

In `Update()`, once all six waves have decayed below `0.0001` intensity **and** the clip layer is finished or below threshold, the emitter stops its `AudioSource` and returns itself to the `SFXManager` pool via `manager.Reclaim(this)`.

## Audio Thread Processing (`OnAudioFilterRead`)

This is where synthesis happens, running on Unity's audio thread at the output sample rate.

### Fast-Path Intensity Hoist

At the top of each buffer the emitter snapshots `intensity > 0.0001` once per layer. Layers below threshold are skipped for the whole buffer — the per-sample `ProcessWave/Noise/Clip` calls are never made. If every layer is silent, the whole buffer is zero-filled with `Array.Clear` and the method returns.

This is safe because `intensity *= decayRate` (with `decayRate ≤ 1`) is monotonically non-increasing on the audio thread, and triggers that raise intensity only happen on the main thread between buffer callbacks.

### Per-Sample Pipeline

For each sample:

1. **Process active layers** — Only layers flagged active by the hoist run: `ProcessWave()` for each tonal wave, `ProcessNoise()` for noise, `ProcessClip()` for the clip. Each returns a float sample, summed into the accumulator.
2. **Saturate** — The accumulator runs through `tanh(sample * 2.5)` for soft clipping. Removes clicks and adds body.
3. **Output** — The saturated sample is scaled by `0.4 * globalVolume * listenerVolume` and written to all channels.

### `ProcessWave()` Detail

Per sample, per active wave:

1. Backstop intensity check (mid-buffer decay past threshold).
2. Apply pitch bend (`freq += pitchBend * dt`).
3. Ramp amplitude toward intensity (attack), then decay intensity.
4. Vibrato and tremolo via `FastSin` LUT.
5. Compute actual frequency = `baseFreq + freq + vibrato + randomPitchOffset`.
6. FM synthesis if configured (also via `FastSin`).
7. Advance phase; wrap at `2π × 100` to keep phase accumulator bounded.
8. Generate base waveform sample (sine via `FastSin`, square via `normalizedPhase < 0.5`, others via arithmetic).
9. Add 2nd and 3rd overtones at 0.5× and 0.25× amplitude if `harmonics > 0`.
10. Apply amplitude envelope, tremolo, amp randomness, and intensity modifier.
11. Bit crushing (quantization).
12. 2-pole resonant low-pass filter.

### `ProcessNoise()` Detail

Simpler than `ProcessWave` — no pitch, no FM, no harmonics. Uses an LCG PRNG (`noiseSeed = noiseSeed * 1664525 + 1013904223`) to generate white noise, then applies tremolo (via `FastSin`), filter, and bit crush.

### `ProcessClip()` Detail

Per sample:

1. Skip if finished, intensity negligible, or no cached samples.
2. Handle start delay.
3. Apply pitch bend to `currentSpeed`.
4. Ramp amplitude (attack), decay intensity.
5. Vibrato (modulates playback speed) and tremolo via `FastSin`.
6. Effective playback speed = `(clipSampleRate / outputSampleRate) * (currentSpeed + vibrato) * randomSpeedMult`.
7. Advance `playbackPosition` fractionally.
8. End-of-clip: wrap if looping, mark `finished` if not.
9. **Linear interpolation** between adjacent cached samples for smooth pitch shifting.
10. Amplitude, tremolo, randomness, intensity modifier.
11. Bit crushing.
12. 2-pole resonant low-pass filter.

## Performance Optimizations

### Fast-Path Intensity Hoist

See above. Collapses silent or decayed-out layers into a no-op for the whole buffer instead of calling `ProcessWave`/`ProcessNoise`/`ProcessClip` per sample only to have them early-return. A pooled emitter awaiting `Update()` reclaim costs one `Array.Clear` per buffer.

### Sin Lookup Table (`FastSin`)

All hot-path sine calls — base sine waveform, vibrato LFO, tremolo LFO, FM modulator — go through `FastSin(float)`, which reads from a 4096-entry table with linear interpolation. The LUT is built once at class init.

- Max error ~3 × 10⁻⁷, well below audibility.
- ~4 cycles/lookup vs. ~20–30 for `Mathf.Sin`.
- The Square waveform was rewritten to skip sine entirely — one comparison on `normalizedPhase`.
- Marked `[MethodImpl(AggressiveInlining)]` so the JIT inlines into callers.

## Thread Safety

- `isActive`, `sustainMode`, `releasing`, `isPaused`, `appPaused`, `globalVolume`, `listenerVolume`, `listenerPaused`, `currentIntensityModifier` are all `volatile`.
- `Update()` (main thread) copies `intensityModifier`, `SFXManager.GlobalSFXVolume`, `AudioListener.volume`, and `AudioListener.pause` into those volatile fields.
- `OnAudioFilterRead()` (audio thread) reads volatile fields and mutates `WaveEnvelope` runtime state (which the main thread doesn't touch during playback).
- `TriggerSounds()` / `Copy()` / `TriggerClip()` / `CopyClip()` run on the main thread and write envelope fields that the audio thread also reads — technically a race, but the worst case is a single glitched sample. Inaudible.
- `CacheClipData()` calls `AudioClip.GetData()` on the main thread and writes the `samples` array reference atomically (reference assignment is atomic in C#).

## Integration Points

- **`SFXManager`** — Object pool owner, dropoff calculator, sequence runner, global volume host.
- **Game code** — Calls `SFXManager.Instance.Emit(...)` or `EmitSustained(...)` for procedural synthesis. Clips go through `TriggerClip()` / `TriggerSustainedClip()` directly on an emitter. Both can target the same emitter simultaneously.

---

## Migration: String-Keyed Dict → Enum-Keyed Structures

The `TriggerSounds` / `Emit` API originally took `Dictionary<string, WaveEnvelope>` with keys like `"SINE"`, `"NOISE"`. Each emit allocated a dictionary, interned strings on first use, and ran a `switch (kvp.Key)` inside `TriggerSounds` that did string hashing per entry. The project had ~99 `SFXManager.Instance.Emit` call sites across 31 files, so the aggregate GC and CPU cost was non-trivial.

Four options were considered; **Option B is now implemented**.

### Option A — Enum + Fixed Array

```csharp
public enum WaveLayer { Sine = 0, Square, Saw, Triangle, Pulse, Noise, _Count }

var waves = new WaveEnvelope[(int)WaveLayer._Count];
waves[(int)WaveLayer.Noise]    = new(intensity: 0.8f, ...);
waves[(int)WaveLayer.Triangle] = new(intensity: 0.7f, ...);
SFXManager.Instance.Emit(waves);
```

Still allocates the array and envelopes. Slightly less garbage than the dict. Call sites are noisier because of the `[(int)WaveLayer.X]` index ceremony. Not recommended.

### Option B — `params` of `(WaveLayer, WaveEnvelope)` tuples **(chosen)**

```csharp
public void Emit(params (WaveLayer layer, WaveEnvelope env)[] waves) { ... }

SFXManager.Instance.Emit(
    (WaveLayer.Noise,    new(intensity: 0.8f, decayRate: 0.96f, filterCutoff: 0.5f)),
    (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: -200f, decayRate: 0.97f)),
    (WaveLayer.Saw,      new(intensity: 0.6f, pitchBend: 200f, decayRate: 0.98f, startDelay: 0.04f)));
```

- Kills the dictionary hash and the string switch.
- `params` still allocates a small backing array per emit; envelopes still allocate as classes.
- Call-site churn is mechanical: one regex pass.
- Overloads taking `duration` / `releaseTime` after the waves parameter can't use `params` (C# rule: `params` must be last) — those take an explicit `(WaveLayer, WaveEnvelope)[]`, so callers write `new[] { (WaveLayer.X, ...), ... }` for the sustained variants.

### Option C — `WaveEnvelope` as struct + stack-allocated bundle

```csharp
public struct WaveEnvelope { ... }  // currently a class

public ref struct WaveBundle
{
    public WaveEnvelope Sine, Square, Saw, Triangle, Pulse, Noise;
    public WaveLayerFlags Active; // bitmask of populated fields
}

SFXManager.Instance.Emit(new WaveBundle {
    Noise    = new(intensity: 0.8f, ...),
    Triangle = new(intensity: 0.7f, ...),
    Active   = WaveLayerFlags.Noise | WaveLayerFlags.Triangle
});
```

Zero heap allocation per emit. Requires splitting `WaveEnvelope`'s runtime state (`amp`, `phase`, `filterState1/2`, `lfoPhase`, etc.) out of the caller-facing type — that state now lives only on `SFXEmitter`'s internal envelopes, and callers hand over a parameters-only struct. Bigger refactor, the right long-term move for hot-path emitters.

### Option D — Named Preset Registry

```csharp
public static class SFXPresets
{
    public static readonly (WaveLayer, WaveEnvelope)[] CoinPickup = {
        (WaveLayer.Noise,    new(intensity: 0.8f, ...)),
        (WaveLayer.Triangle, new(intensity: 0.7f, ...)),
        (WaveLayer.Saw,      new(intensity: 0.6f, ...)),
    };
}

SFXManager.Instance.Emit(SFXPresets.CoinPickup);
```

Zero per-emit allocation, shortest call sites. Forces every inline sound definition into a named preset — loses the "tweak in place" ergonomic where parameters live next to the code that triggers them. Fits sounds that get reused across many call sites, overkill for one-shots.

### Recommendation

Land **Option B** first as a near-mechanical cut-over. If GC pressure is still visible after, graduate the hottest emitters (Coin, bullet fire, enemy hit) to **Option C**, and promote recurring sounds to **Option D** preset refs.
