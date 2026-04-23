using UnityEngine;
using System.Runtime.CompilerServices;

#region WaveLayer

public enum WaveLayer : byte
{
    Sine,
    Square,
    Saw,
    Triangle,
    Pulse,
    Noise,
}

#endregion

#region WaveEnvelope

[System.Serializable]
public class WaveEnvelope
{
    // ===== User parameters =====
    public float intensity;      // target amplitude
    public float pitchBend;      // Hz / second
    public float decayRate;      // amplitude decay
    public float attackRate;     // fade-in speed

    // ===== NEW: Advanced modulation parameters =====
    public float vibratoSpeed;   // LFO frequency for pitch vibrato (Hz)
    public float vibratoDepth;   // pitch vibrato amount (Hz)
    public float tremoloSpeed;   // LFO frequency for amplitude tremolo (Hz)
    public float tremoloDepth;   // amplitude tremolo amount (0-1)
    public float dutyCycle;      // pulse width for square/pulse waves (0.1-0.9)
    public float harmonics;      // add harmonic overtones (0-1)
    public float filterCutoff;   // low-pass filter cutoff frequency (0-1)
    public float filterResonance;// filter resonance/Q factor (0-1)
    public float bitCrush;       // bit reduction for lo-fi effect (0-16)
    public float fmAmount;       // frequency modulation amount (0-1000)
    public float fmRatio;        // FM carrier/modulator ratio (0.5-4)

    // ===== NEW: Randomization parameters =====
    public float pitchRandomness; // random pitch variation per note (0-1)
    public float ampRandomness;   // random amplitude variation (0-1)
    public float startDelay;      // random start delay (seconds)

    // ===== Runtime state (audio thread) =====
    [System.NonSerialized] public float amp;     // current amplitude
    [System.NonSerialized] public float freq;    // current frequency offset
    [System.NonSerialized] public float phase;   // individual phase for this wave
    [System.NonSerialized] public float lfoPhase;// LFO phase
    [System.NonSerialized] public float filterState1; // filter state
    [System.NonSerialized] public float filterState2; // filter state
    [System.NonSerialized] public float bitCrushSteps;
    [System.NonSerialized] public float randomPitchOffset;
    [System.NonSerialized] public float randomAmpMult;
    [System.NonSerialized] public float delayCounter;

    public WaveEnvelope(
        float intensity = 0f,
        float pitchBend = 0f,
        float decayRate = 0.99f,
        float attackRate = 0.01f,
        float vibratoSpeed = 0f,
        float vibratoDepth = 0f,
        float tremoloSpeed = 0f,
        float tremoloDepth = 0f,
        float dutyCycle = 0.5f,
        float harmonics = 0f,
        float filterCutoff = 1f,
        float filterResonance = 0f,
        float bitCrush = 16f,
        float fmAmount = 0f,
        float fmRatio = 1f,
        float pitchRandomness = 0f,
        float ampRandomness = 0f,
        float startDelay = 0f
    )
    {
        this.intensity = intensity;
        this.pitchBend = pitchBend;
        this.decayRate = decayRate;
        this.attackRate = attackRate;
        this.vibratoSpeed = vibratoSpeed;
        this.vibratoDepth = vibratoDepth;
        this.tremoloSpeed = tremoloSpeed;
        this.tremoloDepth = tremoloDepth;
        this.dutyCycle = Mathf.Clamp(dutyCycle, 0.1f, 0.9f);
        this.harmonics = harmonics;
        this.filterCutoff = Mathf.Clamp01(filterCutoff);
        this.filterResonance = Mathf.Clamp01(filterResonance);
        this.bitCrush = Mathf.Clamp(bitCrush, 1f, 16f);
        this.fmAmount = fmAmount;
        this.fmRatio = fmRatio;
        this.pitchRandomness = Mathf.Clamp01(pitchRandomness);
        this.ampRandomness = Mathf.Clamp01(ampRandomness);
        this.startDelay = Mathf.Max(0f, startDelay);

        amp = 0f;
        freq = 0f;
        phase = 0f;
        lfoPhase = 0f;
        filterState1 = 0f;
        filterState2 = 0f;
        randomPitchOffset = 0f;
        randomAmpMult = 1f;
        delayCounter = 0f;
    }

    public void Reset(float baseFreq)
    {
        amp = 0f;
        freq = baseFreq;
        phase = 0f;
        lfoPhase = 0f;
        filterState1 = 0f;
        filterState2 = 0f;
        bitCrushSteps = bitCrush < 16f ? Mathf.Pow(2f, bitCrush) : 0f;
        delayCounter = startDelay;

        // Apply randomization
        randomPitchOffset = (Random.value * 2f - 1f) * pitchRandomness * baseFreq;
        randomAmpMult = 1f - Random.value * ampRandomness;
    }
}

#endregion

#region ClipEnvelope

public class ClipEnvelope
{
    // ===== Clip data (cached for audio thread) =====
    public float[] samples;       // mono sample data, cached from AudioClip
    public int clipSampleRate;    // original clip sample rate
    public int clipSampleCount;   // total mono samples

    // ===== User parameters =====
    public float intensity;       // target amplitude
    public float playbackSpeed;   // 1.0 = normal, 2.0 = double speed / octave up
    public float pitchBend;       // playback speed change per second
    public float decayRate;       // amplitude decay per sample
    public float attackRate;      // fade-in speed
    public bool loop;             // loop the clip

    // ===== Modulation =====
    public float vibratoSpeed;    // LFO frequency for pitch vibrato (Hz)
    public float vibratoDepth;    // pitch vibrato amount (speed multiplier range)
    public float tremoloSpeed;    // LFO frequency for amplitude tremolo (Hz)
    public float tremoloDepth;    // amplitude tremolo amount (0-1)
    public float filterCutoff;    // low-pass filter cutoff (0-1, 1 = bypass)
    public float filterResonance; // filter Q (0-1)
    public float bitCrush;        // bit depth reduction (1-16, 16 = off)
    public float pitchRandomness; // random playback speed variation (0-1)
    public float ampRandomness;   // random amplitude variation (0-1)
    public float startDelay;      // delay before playback (seconds)

    // ===== Runtime state (audio thread) =====
    [System.NonSerialized] public float amp;
    [System.NonSerialized] public double playbackPosition; // fractional sample index
    [System.NonSerialized] public float currentSpeed;      // current speed after pitch bend
    [System.NonSerialized] public float lfoPhase;
    [System.NonSerialized] public float filterState1;
    [System.NonSerialized] public float filterState2;
    [System.NonSerialized] public float bitCrushSteps;
    [System.NonSerialized] public float randomSpeedMult;
    [System.NonSerialized] public float randomAmpMult;
    [System.NonSerialized] public float delayCounter;
    [System.NonSerialized] public bool finished;

    public ClipEnvelope(
        float intensity = 0f,
        float playbackSpeed = 1f,
        float pitchBend = 0f,
        float decayRate = 0.99f,
        float attackRate = 0.01f,
        bool loop = false,
        float vibratoSpeed = 0f,
        float vibratoDepth = 0f,
        float tremoloSpeed = 0f,
        float tremoloDepth = 0f,
        float filterCutoff = 1f,
        float filterResonance = 0f,
        float bitCrush = 16f,
        float pitchRandomness = 0f,
        float ampRandomness = 0f,
        float startDelay = 0f
    )
    {
        this.intensity = intensity;
        this.playbackSpeed = playbackSpeed;
        this.pitchBend = pitchBend;
        this.decayRate = decayRate;
        this.attackRate = attackRate;
        this.loop = loop;
        this.vibratoSpeed = vibratoSpeed;
        this.vibratoDepth = vibratoDepth;
        this.tremoloSpeed = tremoloSpeed;
        this.tremoloDepth = tremoloDepth;
        this.filterCutoff = Mathf.Clamp01(filterCutoff);
        this.filterResonance = Mathf.Clamp01(filterResonance);
        this.bitCrush = Mathf.Clamp(bitCrush, 1f, 16f);
        this.pitchRandomness = Mathf.Clamp01(pitchRandomness);
        this.ampRandomness = Mathf.Clamp01(ampRandomness);
        this.startDelay = Mathf.Max(0f, startDelay);

        samples = null;
        clipSampleRate = 44100;
        clipSampleCount = 0;
        amp = 0f;
        playbackPosition = 0;
        currentSpeed = playbackSpeed;
        lfoPhase = 0f;
        filterState1 = 0f;
        filterState2 = 0f;
        randomSpeedMult = 1f;
        randomAmpMult = 1f;
        delayCounter = 0f;
        finished = true;
    }

    public void Reset()
    {
        amp = 0f;
        playbackPosition = 0;
        currentSpeed = playbackSpeed;
        lfoPhase = 0f;
        filterState1 = 0f;
        filterState2 = 0f;
        bitCrushSteps = bitCrush < 16f ? Mathf.Pow(2f, bitCrush) : 0f;
        delayCounter = startDelay;
        finished = false;

        randomSpeedMult = 1f + (Random.value * 2f - 1f) * pitchRandomness;
        randomAmpMult = 1f - Random.value * ampRandomness;
    }
}

#endregion

[RequireComponent(typeof(AudioSource))]
public class SFXEmitter : MonoBehaviour
{
    [HideInInspector] public SFXManager manager;

    // Incremented on every Activate so pooled safety timers can detect reuse.
    [HideInInspector] public int activationId;

    // ===== Intensity Modifier =====
    [Tooltip("Multiplies the intensity of all waves in real-time")]
    public float intensityModifier = 1.0f;
    volatile float currentIntensityModifier = 1.0f;  // Thread-safe copy for audio thread

    // ===== Global Volume (set by SFXManager) =====
    volatile float globalVolume = 1.0f;              // Thread-safe copy for audio thread
    volatile float listenerVolume = 1.0f;            // AudioListener.volume synced each frame
    volatile bool listenerPaused;                     // AudioListener.pause synced each frame

    // ===== Audio state =====
    double sampleRate;

    // ===== Waveforms =====
    readonly WaveEnvelope sine = new();
    readonly WaveEnvelope square = new();
    readonly WaveEnvelope noise = new();
    readonly WaveEnvelope saw = new();
    readonly WaveEnvelope triangle = new();
    readonly WaveEnvelope pulse = new();

    // ===== Clip layer =====
    readonly ClipEnvelope clip = new();

    // ===== Runtime =====
    volatile bool isActive;
    volatile bool sustainMode;  // When true, sound continues until Release() is called
    volatile bool releasing;    // When true, sound is in release phase
    volatile bool isPaused;     // When true, audio processing is suspended
    volatile bool appPaused;    // True when app loses focus (alt-tab)
    uint noiseSeed = 1;

    AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    // =========================================================
    // MAIN THREAD
    // =========================================================

    public void Activate(float intensityModifier)
    {
        activationId++;
        float baseFreq = 200f;
        this.intensityModifier = intensityModifier;

        sine.Reset(baseFreq);
        square.Reset(baseFreq);
        noise.Reset(baseFreq);
        saw.Reset(baseFreq);
        triangle.Reset(baseFreq);
        pulse.Reset(baseFreq);

        sine.intensity = square.intensity =
        noise.intensity = saw.intensity =
        triangle.intensity = pulse.intensity = 0f;

        clip.intensity = 0f;
        clip.finished = true;

        isActive = true;
        gameObject.SetActive(true);
        source.Play();
    }

    public void TriggerSounds((WaveLayer layer, WaveEnvelope env)[] waveSettings)
    {
        sustainMode = false;
        releasing = false;

        for (int i = 0; i < waveSettings.Length; i++)
        {
            var (layer, src) = waveSettings[i];
            switch (layer)
            {
                case WaveLayer.Sine:     Copy(src, sine);     break;
                case WaveLayer.Square:   Copy(src, square);   break;
                case WaveLayer.Saw:      Copy(src, saw);      break;
                case WaveLayer.Triangle: Copy(src, triangle); break;
                case WaveLayer.Pulse:    Copy(src, pulse);    break;
                case WaveLayer.Noise:    Copy(src, noise);    break;
            }
        }
    }

    public void TriggerSustainedSounds((WaveLayer layer, WaveEnvelope env)[] waveSettings)
    {
        sustainMode = true;
        releasing = false;

        for (int i = 0; i < waveSettings.Length; i++)
        {
            var (layer, src) = waveSettings[i];
            switch (layer)
            {
                case WaveLayer.Sine:     Copy(src, sine,     true); break;
                case WaveLayer.Square:   Copy(src, square,   true); break;
                case WaveLayer.Saw:      Copy(src, saw,      true); break;
                case WaveLayer.Triangle: Copy(src, triangle, true); break;
                case WaveLayer.Pulse:    Copy(src, pulse,    true); break;
                case WaveLayer.Noise:    Copy(src, noise,    true); break;
            }
        }
    }

    /// <summary>
    /// Trigger an AudioClip with optional synthesis-style effects.
    /// The clip's sample data is cached for the audio thread.
    /// Can be combined with wave triggers on the same emitter.
    /// </summary>
    public void TriggerClip(AudioClip audioClip, ClipEnvelope settings)
    {
        CacheClipData(audioClip);
        CopyClip(settings, clip, false);
    }

    /// <summary>
    /// Trigger a sustained AudioClip that loops until Release() is called.
    /// </summary>
    public void TriggerSustainedClip(AudioClip audioClip, ClipEnvelope settings)
    {
        CacheClipData(audioClip);
        CopyClip(settings, clip, true);
        sustainMode = true;
        releasing = false;
    }

    void CacheClipData(AudioClip audioClip)
    {
        int totalSamples = audioClip.samples * audioClip.channels;
        float[] raw = new float[totalSamples];
        audioClip.GetData(raw, 0);

        // Downmix to mono if stereo+
        int ch = audioClip.channels;
        if (ch > 1)
        {
            int monoCount = audioClip.samples;
            float[] mono = new float[monoCount];
            for (int i = 0; i < monoCount; i++)
            {
                float sum = 0f;
                for (int c = 0; c < ch; c++)
                    sum += raw[i * ch + c];
                mono[i] = sum / ch;
            }
            clip.samples = mono;
            clip.clipSampleCount = monoCount;
        }
        else
        {
            clip.samples = raw;
            clip.clipSampleCount = audioClip.samples;
        }

        clip.clipSampleRate = audioClip.frequency;
    }

    void CopyClip(ClipEnvelope from, ClipEnvelope to, bool sustained)
    {
        to.intensity = Mathf.Max(to.intensity, from.intensity);
        to.playbackSpeed = from.playbackSpeed;
        to.pitchBend = from.pitchBend;
        to.decayRate = sustained ? 1.0f : from.decayRate;
        to.attackRate = from.attackRate;
        to.loop = sustained ? true : from.loop;

        to.vibratoSpeed = from.vibratoSpeed;
        to.vibratoDepth = from.vibratoDepth;
        to.tremoloSpeed = from.tremoloSpeed;
        to.tremoloDepth = from.tremoloDepth;
        to.filterCutoff = from.filterCutoff;
        to.filterResonance = from.filterResonance;
        to.bitCrush = from.bitCrush;
        to.pitchRandomness = from.pitchRandomness;
        to.ampRandomness = from.ampRandomness;
        to.startDelay = from.startDelay;

        to.Reset();
    }

    public void Release(float releaseTime = 0.5f)
    {
        // Works on both sustained sounds (starts the fade-out) and fire-and-forget
        // sounds (accelerates the natural decay — used by SFXManager.StopAll).
        releasing = true;

        // Set all waves to decay based on release time
        float releaseDecay = Mathf.Pow(0.001f, 1f / (releaseTime * (float)sampleRate));

        sine.decayRate = releaseDecay;
        square.decayRate = releaseDecay;
        noise.decayRate = releaseDecay;
        saw.decayRate = releaseDecay;
        triangle.decayRate = releaseDecay;
        pulse.decayRate = releaseDecay;
        clip.decayRate = releaseDecay;
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Unpause()
    {
        isPaused = false;
    }

    void Copy(WaveEnvelope from, WaveEnvelope to, bool sustained = false)
    {
        to.intensity = Mathf.Max(to.intensity, from.intensity);
        to.pitchBend = from.pitchBend;
        to.decayRate = sustained ? 1.0f : from.decayRate;
        to.attackRate = from.attackRate;

        to.vibratoSpeed = from.vibratoSpeed;
        to.vibratoDepth = from.vibratoDepth;
        to.tremoloSpeed = from.tremoloSpeed;
        to.tremoloDepth = from.tremoloDepth;
        to.dutyCycle = from.dutyCycle;
        to.harmonics = from.harmonics;
        to.filterCutoff = from.filterCutoff;
        to.filterResonance = from.filterResonance;
        to.bitCrush = from.bitCrush;
        to.fmAmount = from.fmAmount;
        to.fmRatio = from.fmRatio;
        to.pitchRandomness = from.pitchRandomness;
        to.ampRandomness = from.ampRandomness;
        to.startDelay = from.startDelay;

        to.amp = 0f;
        to.freq = from.freq;
        to.phase = 0f;
        to.lfoPhase = 0f;
        to.bitCrushSteps = from.bitCrush < 16f ? Mathf.Pow(2f, from.bitCrush) : 0f;
        to.delayCounter = from.startDelay;

        // Apply randomization
        to.randomPitchOffset = (Random.value * 2f - 1f) * from.pitchRandomness * 200f;
        to.randomAmpMult = 1f - Random.value * from.ampRandomness;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        appPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        appPaused = pauseStatus;
    }

    void Update()
    {
        if (!isActive) return;

        // Sync main-thread state to audio thread
        currentIntensityModifier = intensityModifier;
        globalVolume = SFXManager.GlobalSFXVolume;
        listenerVolume = AudioListener.volume;
        listenerPaused = AudioListener.pause;

        bool clipDone = clip.finished || clip.intensity < 0.0001f;

        if (clipDone &&
            sine.intensity < 0.0001f &&
            square.intensity < 0.0001f &&
            noise.intensity < 0.0001f &&
            saw.intensity < 0.0001f &&
            triangle.intensity < 0.0001f &&
            pulse.intensity < 0.0001f)
        {
            isActive = false;
            source.Stop();
            manager.Reclaim(this);
        }
    }

    // =========================================================
    // AUDIO THREAD
    // =========================================================

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isActive || isPaused || appPaused || listenerPaused || listenerVolume < 0.001f)
        {
            System.Array.Clear(data, 0, data.Length);
            return;
        }

        // intensity can only decay on the audio thread, so a layer below threshold
        // at buffer start stays silent for the whole buffer. Snapshot once.
        const float EPS = 0.0001f;
        bool hasSine     = sine.intensity     > EPS;
        bool hasSquare   = square.intensity   > EPS;
        bool hasSaw      = saw.intensity      > EPS;
        bool hasTriangle = triangle.intensity > EPS;
        bool hasPulse    = pulse.intensity    > EPS;
        bool hasNoise    = noise.intensity    > EPS;
        bool hasClip     = !clip.finished && clip.intensity > EPS && clip.samples != null;

        if (!(hasSine | hasSquare | hasSaw | hasTriangle | hasPulse | hasNoise | hasClip))
        {
            System.Array.Clear(data, 0, data.Length);
            return;
        }

        float volumeScale = globalVolume * listenerVolume;

        double dt = 1.0 / sampleRate;
        const float baseFreq = 120f;

        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;
            if (hasSine)     sample += ProcessWave(sine,     baseFreq, dt, WaveType.Sine);
            if (hasSquare)   sample += ProcessWave(square,   baseFreq, dt, WaveType.Square);
            if (hasSaw)      sample += ProcessWave(saw,      baseFreq, dt, WaveType.Saw);
            if (hasTriangle) sample += ProcessWave(triangle, baseFreq, dt, WaveType.Triangle);
            if (hasPulse)    sample += ProcessWave(pulse,    baseFreq, dt, WaveType.Pulse);
            if (hasNoise)    sample += ProcessNoise(noise, dt);
            if (hasClip)     sample += ProcessClip(clip, dt);

            // Soft saturation = removes clicks + adds body
            sample = (float)System.Math.Tanh(sample * 2.5f);

            for (int c = 0; c < channels; c++)
                data[i + c] = sample * 0.4f * volumeScale;
        }
    }

    // =========================================================
    // SIN LUT
    // =========================================================

    const int SIN_LUT_SIZE = 4096;
    const int SIN_LUT_MASK = SIN_LUT_SIZE - 1;
    const float TWO_PI = 2f * Mathf.PI;
    const float INV_TWO_PI = 1f / TWO_PI;

    // +1 slot mirrors index 0 so linear interpolation at the wrap boundary
    // reads a valid neighbor without a branch.
    static readonly float[] sinLut = BuildSinLut();

    static float[] BuildSinLut()
    {
        var lut = new float[SIN_LUT_SIZE + 1];
        for (int i = 0; i < SIN_LUT_SIZE; i++)
            lut[i] = Mathf.Sin(i * TWO_PI / SIN_LUT_SIZE);
        lut[SIN_LUT_SIZE] = lut[0];
        return lut;
    }

    // Phase-wrapped sine via 4096-entry LUT with linear interpolation.
    // Max error ~3e-7 — inaudible. Accepts any float; negatives handled.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float FastSin(float x)
    {
        float t = x * INV_TWO_PI;
        t -= (int)t;
        if (t < 0f) t += 1f;
        float fIndex = t * SIN_LUT_SIZE;
        int iRaw = (int)fIndex;
        float frac = fIndex - iRaw;
        int i = iRaw & SIN_LUT_MASK;
        return sinLut[i] + (sinLut[i + 1] - sinLut[i]) * frac;
    }

    // =========================================================
    // UNIFIED WAVE PROCESSOR
    // =========================================================

    enum WaveType { Sine, Square, Saw, Triangle, Pulse }

    float ProcessWave(WaveEnvelope env, float baseFreq, double dt, WaveType waveType)
    {
        if (env.intensity <= 0.0001f) return 0f;

        // Handle start delay
        if (env.delayCounter > 0f)
        {
            env.delayCounter -= (float)dt;
            return 0f;
        }

        // Update envelope
        env.freq += env.pitchBend * (float)dt;
        env.amp = Mathf.MoveTowards(env.amp, env.intensity, env.attackRate);
        env.intensity *= env.decayRate;

        // Calculate modulations
        float vibratoMod = 0f;
        float tremoloMod = 1f;

        if (env.vibratoSpeed > 0f && env.vibratoDepth > 0f)
        {
            vibratoMod = FastSin(env.lfoPhase * env.vibratoSpeed) * env.vibratoDepth;
        }

        if (env.tremoloSpeed > 0f && env.tremoloDepth > 0f)
        {
            tremoloMod = 1f - (FastSin(env.lfoPhase * env.tremoloSpeed * 0.5f) * 0.5f + 0.5f) * env.tremoloDepth;
        }

        env.lfoPhase += (float)(2.0 * Mathf.PI * dt);

        // Calculate actual frequency with modulation and randomness
        float actualFreq = baseFreq + env.freq + vibratoMod + env.randomPitchOffset;

        // FM synthesis modulation
        float fmMod = 0f;
        if (env.fmAmount > 0f)
        {
            float modFreq = actualFreq * env.fmRatio;
            fmMod = FastSin(env.phase * modFreq / actualFreq) * env.fmAmount;
        }

        actualFreq += fmMod;

        // Update phase
        env.phase += actualFreq * (float)(2.0 * Mathf.PI * dt);
        if (env.phase > 2.0 * Mathf.PI * 100.0) env.phase -= (float)(2.0 * Mathf.PI * 100.0);

        // Generate base waveform
        float sample = GenerateWaveform(env.phase, waveType, env.dutyCycle);

        // Add harmonics
        if (env.harmonics > 0f)
        {
            sample += GenerateWaveform(env.phase * 2f, waveType, env.dutyCycle) * env.harmonics * 0.5f;
            sample += GenerateWaveform(env.phase * 3f, waveType, env.dutyCycle) * env.harmonics * 0.25f;
        }

        // Apply amplitude with tremolo, randomness, and intensity modifier
        sample *= env.amp * tremoloMod * env.randomAmpMult * currentIntensityModifier;

        // Apply bit crushing
        if (env.bitCrushSteps > 0f)
            sample = Mathf.Round(sample * env.bitCrushSteps) / env.bitCrushSteps;

        // Apply low-pass filter
        if (env.filterCutoff < 1f)
        {
            float cutoff = env.filterCutoff * 0.5f;
            float resonance = 1f + env.filterResonance * 10f;

            env.filterState1 += cutoff * (sample - env.filterState1 + resonance * (env.filterState1 - env.filterState2));
            env.filterState2 += cutoff * (env.filterState1 - env.filterState2);

            sample = env.filterState2;
        }

        return sample;
    }

    float GenerateWaveform(float phase, WaveType type, float dutyCycle)
    {
        float normalizedPhase = (phase % (2f * Mathf.PI)) / (2f * Mathf.PI);

        switch (type)
        {
            case WaveType.Sine:
                return FastSin(phase);

            case WaveType.Square:
                return normalizedPhase < 0.5f ? 1f : -1f;

            case WaveType.Saw:
                return normalizedPhase * 2f - 1f;

            case WaveType.Triangle:
                return Mathf.Abs(normalizedPhase * 4f - 2f) - 1f;

            case WaveType.Pulse:
                return normalizedPhase < dutyCycle ? 1f : -1f;

            default:
                return 0f;
        }
    }

    float ProcessNoise(WaveEnvelope env, double dt)
    {
        if (env.intensity <= 0.0001f) return 0f;

        // Handle start delay
        if (env.delayCounter > 0f)
        {
            env.delayCounter -= (float)dt;
            return 0f;
        }

        env.amp = Mathf.MoveTowards(env.amp, env.intensity, env.attackRate);
        env.intensity *= env.decayRate;

        // Tremolo modulation for noise
        float tremoloMod = 1f;
        if (env.tremoloSpeed > 0f && env.tremoloDepth > 0f)
        {
            tremoloMod = 1f - (FastSin(env.lfoPhase * env.tremoloSpeed * 0.5f) * 0.5f + 0.5f) * env.tremoloDepth;
            env.lfoPhase += (float)(2.0 * Mathf.PI * dt);
        }

        // Apply amplitude with tremolo, randomness, and intensity modifier
        float sample = Noise() * env.amp * tremoloMod * env.randomAmpMult * currentIntensityModifier;

        // Apply filter to noise
        if (env.filterCutoff < 1f)
        {
            float cutoff = env.filterCutoff * 0.5f;
            float resonance = 1f + env.filterResonance * 10f;

            env.filterState1 += cutoff * (sample - env.filterState1 + resonance * (env.filterState1 - env.filterState2));
            env.filterState2 += cutoff * (env.filterState1 - env.filterState2);

            sample = env.filterState2;
        }

        // Apply bit crushing to noise
        if (env.bitCrushSteps > 0f)
            sample = Mathf.Round(sample * env.bitCrushSteps) / env.bitCrushSteps;

        return sample;
    }

    // =========================================================
    // CLIP PROCESSOR
    // =========================================================

    float ProcessClip(ClipEnvelope env, double dt)
    {
        if (env.finished || env.intensity <= 0.0001f || env.samples == null) return 0f;

        // Handle start delay
        if (env.delayCounter > 0f)
        {
            env.delayCounter -= (float)dt;
            return 0f;
        }

        // Update envelope
        env.currentSpeed += env.pitchBend * (float)dt;
        env.amp = Mathf.MoveTowards(env.amp, env.intensity, env.attackRate);
        env.intensity *= env.decayRate;

        // Vibrato modulates playback speed
        float vibratoMod = 0f;
        if (env.vibratoSpeed > 0f && env.vibratoDepth > 0f)
        {
            vibratoMod = FastSin(env.lfoPhase * env.vibratoSpeed) * env.vibratoDepth;
        }

        // Tremolo
        float tremoloMod = 1f;
        if (env.tremoloSpeed > 0f && env.tremoloDepth > 0f)
        {
            tremoloMod = 1f - (FastSin(env.lfoPhase * env.tremoloSpeed * 0.5f) * 0.5f + 0.5f) * env.tremoloDepth;
        }

        env.lfoPhase += (float)(2.0 * Mathf.PI * dt);

        // Calculate effective playback speed (clip sample rate ratio * speed * vibrato * randomness)
        double speedRatio = (double)env.clipSampleRate / sampleRate;
        double effectiveSpeed = speedRatio * (env.currentSpeed + vibratoMod) * env.randomSpeedMult;

        // Advance playback position
        env.playbackPosition += effectiveSpeed;

        // Handle end of clip
        if (env.playbackPosition >= env.clipSampleCount)
        {
            if (env.loop)
            {
                env.playbackPosition %= env.clipSampleCount;
            }
            else
            {
                env.finished = true;
                return 0f;
            }
        }

        if (env.playbackPosition < 0)
            env.playbackPosition = 0;

        // Read sample with linear interpolation
        int idx = (int)env.playbackPosition;
        float frac = (float)(env.playbackPosition - idx);

        float s0 = env.samples[idx];
        float s1 = (idx + 1 < env.clipSampleCount) ? env.samples[idx + 1]
                  : (env.loop ? env.samples[0] : s0);

        float sample = s0 + (s1 - s0) * frac;

        // Apply amplitude with tremolo, randomness, and intensity modifier
        sample *= env.amp * tremoloMod * env.randomAmpMult * currentIntensityModifier;

        // Apply bit crushing
        if (env.bitCrushSteps > 0f)
            sample = Mathf.Round(sample * env.bitCrushSteps) / env.bitCrushSteps;

        // Apply low-pass filter
        if (env.filterCutoff < 1f)
        {
            float cutoff = env.filterCutoff * 0.5f;
            float resonance = 1f + env.filterResonance * 10f;

            env.filterState1 += cutoff * (sample - env.filterState1 + resonance * (env.filterState1 - env.filterState2));
            env.filterState2 += cutoff * (env.filterState1 - env.filterState2);

            sample = env.filterState2;
        }

        return sample;
    }

    // =========================================================
    // NOISE
    // =========================================================

    float Noise()
    {
        noiseSeed = noiseSeed * 1664525u + 1013904223u;
        return ((noiseSeed >> 9) & 0x7FFFFF) / (float)0x7FFFFF * 2f - 1f;
    }
}
