using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#region SFXStep

/// <summary>
/// A single step in a sound sequence. Set waves, clip, or both.
/// </summary>
public class SFXStep
{
    /// <summary>Wave layers to trigger (null to skip synthesis).</summary>
    public (WaveLayer layer, WaveEnvelope env)[] waves;

    /// <summary>AudioClip to play (null to skip clip playback).</summary>
    public AudioClip clip;

    /// <summary>Settings for the clip layer (required if clip is set).</summary>
    public ClipEnvelope clipSettings;

    /// <summary>Seconds to wait BEFORE this step plays.</summary>
    public float delay;

    /// <summary>
    /// If > 0, the sound is sustained for this many seconds then released.
    /// If 0 or negative, the sound is fire-and-forget (plays its natural envelope).
    /// </summary>
    public float duration;

    /// <summary>Release fade-out time in seconds (only used when duration > 0).</summary>
    public float releaseTime = 0.5f;

    public SFXStep() { }

    /// <summary>Shorthand: wave-only step.</summary>
    public SFXStep((WaveLayer layer, WaveEnvelope env)[] waves, float delay = 0f, float duration = 0f, float releaseTime = 0.5f)
    {
        this.waves = waves;
        this.delay = delay;
        this.duration = duration;
        this.releaseTime = releaseTime;
    }

    /// <summary>Shorthand: clip-only step.</summary>
    public SFXStep(AudioClip clip, ClipEnvelope clipSettings, float delay = 0f, float duration = 0f, float releaseTime = 0.5f)
    {
        this.clip = clip;
        this.clipSettings = clipSettings;
        this.delay = delay;
        this.duration = duration;
        this.releaseTime = releaseTime;
    }
}

#endregion

#region SFXSequenceHandle

/// <summary>
/// Handle returned by EmitSequence. Use to check status or stop a running sequence.
/// </summary>
public class SFXSequenceHandle
{
    internal Coroutine coroutine;
    internal SFXEmitter activeEmitter;

    /// <summary>True while the sequence coroutine is running.</summary>
    public bool IsPlaying { get; internal set; }
}

#endregion

public class SFXManager : MonoBehaviour
{
    static SFXManager instance;
    public static SFXManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SFXManager>();
                if (instance == null)
                {
                    GameObject obj = new();
                    instance = obj.AddComponent<SFXManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Global SFX volume (0-1). Applied after saturation for linear scaling.
    /// Use SetGlobalVolume() with a 0-1 slider value for perceptual scaling.
    /// </summary>
    public static volatile float GlobalSFXVolume = 1.0f;

    /// <summary>
    /// Set global SFX volume from a 0-1 slider value.
    /// Uses a power curve so 0.5 sounds like half volume.
    /// </summary>
    public static void SetGlobalVolume(float sliderValue)
    {
        GlobalSFXVolume = Mathf.Pow(Mathf.Clamp01(sliderValue), 2.2f);
    }

    private AudioListener audioListener;
    private SFXEmitter emitterPrefab;

    private int poolSize = 64;

    readonly Queue<SFXEmitter> pool = new();
    readonly List<SFXEmitter> allEmitters = new();

    const float MIN_DROPOFF = 1f;
    const float MAX_DROPOFF = 3000f;

    void Awake()
    {
        emitterPrefab = Resources.Load<SFXEmitter>("Prefabs/SFX_Emitter");
        audioListener = FindFirstObjectByType<AudioListener>();

        for (int i = 0; i < poolSize; i++)
        {
            var emitter = Instantiate(emitterPrefab, transform);
            emitter.manager = this;

            emitter.gameObject.SetActive(false);
            pool.Enqueue(emitter);
            allEmitters.Add(emitter);
        }
    }

    #region Spawning

    public void Emit(params (WaveLayer layer, WaveEnvelope env)[] waves)
    {
        var emitter = GetEmitter();
        if (emitter) emitter.TriggerSounds(waves);
    }

    // Emit a sustained sound for a fixed duration, then release automatically.
    // Can't use params here — C# requires params to be the last parameter.
    public void Emit((WaveLayer layer, WaveEnvelope env)[] waves, float duration, float releaseTime = 0.5f)
    {
        var emitter = GetEmitter();
        if (emitter)
        {
            emitter.TriggerSustainedSounds(waves);
            StartCoroutine(ReleaseAfterDuration(emitter, duration, releaseTime));
        }
    }

    // Start a sustained sound that continues until released, or until maxDuration
    // elapses as a safety net (set maxDuration <= 0 to disable the timeout).
    public SFXEmitter EmitSustained((WaveLayer layer, WaveEnvelope env)[] waves, float maxDuration = 30f, float releaseTime = 0.5f)
    {
        var emitter = GetEmitter();
        if (emitter)
        {
            emitter.TriggerSustainedSounds(waves);
            if (maxDuration > 0f)
                StartCoroutine(SafetyRelease(emitter, emitter.activationId, maxDuration, releaseTime));
        }
        return emitter;
    }

    // ===== SPAWNING WITH DROPOFF =====

    public void Emit(Vector3 position, params (WaveLayer layer, WaveEnvelope env)[] waves)
    {
        var emitter = GetEmitter(GetDropoff(position));
        if (emitter) emitter.TriggerSounds(waves);
    }

    // Emit a sustained sound for a fixed duration, then release automatically.
    public void Emit(Vector3 position, (WaveLayer layer, WaveEnvelope env)[] waves, float duration, float releaseTime = 0.5f)
    {
        var emitter = GetEmitter(GetDropoff(position));
        if (emitter)
        {
            emitter.TriggerSustainedSounds(waves);
            StartCoroutine(ReleaseAfterDuration(emitter, duration, releaseTime));
        }
    }

    // Start a sustained sound that continues until released, or until maxDuration
    // elapses as a safety net (set maxDuration <= 0 to disable the timeout).
    public SFXEmitter EmitSustained(Vector3 position, (WaveLayer layer, WaveEnvelope env)[] waves, float maxDuration = 30f, float releaseTime = 0.5f)
    {
        var emitter = GetEmitter(GetDropoff(position));
        if (emitter)
        {
            emitter.TriggerSustainedSounds(waves);
            if (maxDuration > 0f)
                StartCoroutine(SafetyRelease(emitter, emitter.activationId, maxDuration, releaseTime));
        }
        return emitter;
    }

    // Release a sustained sound with optional release time
    public void ReleaseSustained(SFXEmitter emitter, float releaseTime = 0.5f)
    {
        if (emitter != null) emitter.Release(releaseTime);
    }

    // Coroutine to release a sustained sound after a duration
    IEnumerator ReleaseAfterDuration(SFXEmitter emitter, float duration, float releaseTime)
    {
        yield return new WaitForSeconds(duration);
        if (emitter != null) emitter.Release(releaseTime);
    }

    // Safety timeout for EmitSustained. Only releases if the emitter hasn't been
    // reclaimed and reused for a new sound (tracked via activationId).
    IEnumerator SafetyRelease(SFXEmitter emitter, int activationId, float maxDuration, float releaseTime)
    {
        yield return new WaitForSeconds(maxDuration);
        if (emitter != null && emitter.activationId == activationId)
            emitter.Release(releaseTime);
    }

    #endregion

    #region Sequences

    /// <summary>Play a list of sound steps one after another.</summary>
    public SFXSequenceHandle EmitSequence(List<SFXStep> steps)
    {
        var handle = new SFXSequenceHandle { IsPlaying = true };
        handle.coroutine = StartCoroutine(RunSequence(steps, 1f, handle));
        return handle;
    }

    /// <summary>Play a sequence with spatial dropoff based on position.</summary>
    public SFXSequenceHandle EmitSequence(Vector3 position, List<SFXStep> steps)
    {
        var handle = new SFXSequenceHandle { IsPlaying = true };
        handle.coroutine = StartCoroutine(RunSequence(steps, GetDropoff(position), handle));
        return handle;
    }

    /// <summary>Stop a running sequence and release any active sustained emitter.</summary>
    public void StopSequence(SFXSequenceHandle handle)
    {
        if (handle == null || !handle.IsPlaying) return;

        if (handle.coroutine != null)
            StopCoroutine(handle.coroutine);

        if (handle.activeEmitter != null)
            handle.activeEmitter.Release(0.1f);

        handle.IsPlaying = false;
    }

    IEnumerator RunSequence(List<SFXStep> steps, float intensityMod, SFXSequenceHandle handle)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];

            // Pre-step delay
            if (step.delay > 0f)
                yield return new WaitForSeconds(step.delay);

            var emitter = GetEmitter(intensityMod);
            if (emitter == null) continue;

            bool sustained = step.duration > 0f;

            // Trigger waves
            if (step.waves != null)
            {
                if (sustained)
                    emitter.TriggerSustainedSounds(step.waves);
                else
                    emitter.TriggerSounds(step.waves);
            }

            // Trigger clip
            if (step.clip != null && step.clipSettings != null)
            {
                if (sustained)
                    emitter.TriggerSustainedClip(step.clip, step.clipSettings);
                else
                    emitter.TriggerClip(step.clip, step.clipSettings);
            }

            // If sustained, hold then release before moving on
            if (sustained)
            {
                handle.activeEmitter = emitter;
                yield return new WaitForSeconds(step.duration);
                emitter.Release(step.releaseTime);
                handle.activeEmitter = null;
            }
        }

        handle.IsPlaying = false;
    }

    #endregion

    public float GetDropoff(Vector3 position)
    {
        float distance = Vector3.Distance(position, audioListener.transform.position);

        if (distance <= MIN_DROPOFF)
            return 1f;

        if (distance >= MAX_DROPOFF)
            return 0f;

        // Linear interpolation between min and max
        return 1f - ((distance - MIN_DROPOFF) / (MAX_DROPOFF - MIN_DROPOFF));
    }

    SFXEmitter GetEmitter(float intensityModify = 1f)
    {
        if (pool.Count == 0)
            return null; // or steal oldest

        var emitter = pool.Dequeue();
        emitter.Activate(intensityModify);
        return emitter;
    }

    // ===== RECLAIM =====

    public void Reclaim(SFXEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        pool.Enqueue(emitter);
    }

    // ===== PAUSE CONTROL =====

    public void PauseAllSustainedSounds()
    {
        foreach (var emitter in allEmitters)
        {
            if (emitter.gameObject.activeSelf)
            {
                emitter.Pause();
            }
        }
    }

    public void UnpauseAllSustainedSounds()
    {
        foreach (var emitter in allEmitters)
        {
            if (emitter.gameObject.activeSelf)
            {
                emitter.Unpause();
            }
        }
    }

    // Release every active emitter immediately. Sustained sounds fade out over
    // releaseTime; fire-and-forget sounds also get a release envelope so their
    // tails don't ring past the intended stop.
    public void StopAll(float releaseTime = 0.1f)
    {
        foreach (var emitter in allEmitters)
        {
            if (emitter.gameObject.activeSelf)
                emitter.Release(releaseTime);
        }
    }
}
