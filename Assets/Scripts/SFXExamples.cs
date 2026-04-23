using UnityEngine;
using System.Collections.Generic;

public static class SFXExamples
{

    public static Dictionary<string, (WaveLayer, WaveEnvelope)[][]> GetExamples()
    {
        return GenerateVariations(FullList());
    }

    // Generate 24 variations for each example (plus the original at index 0)
    static Dictionary<string, (WaveLayer, WaveEnvelope)[][]> GenerateVariations(Dictionary<string, (WaveLayer, WaveEnvelope)[]> baseExamples)
    {
        var result = new Dictionary<string, (WaveLayer, WaveEnvelope)[][]>();

        foreach (var kvp in baseExamples)
        {
            var baseWaves = kvp.Value;
            var variations = new (WaveLayer, WaveEnvelope)[25][];
            variations[0] = baseWaves;

            for (int i = 1; i < variations.Length; i++)
            {
                var variant = new (WaveLayer, WaveEnvelope)[baseWaves.Length];

                for (int j = 0; j < baseWaves.Length; j++)
                {
                    var (layer, orig) = baseWaves[j];

                    WaveEnvelope varied = new WaveEnvelope(
                        intensity: VaryParam(orig.intensity, 0.1f, 1.0f, 0.15f),
                        pitchBend: VaryParam(orig.pitchBend, -2000f, 2000f, 30f),
                        decayRate: VaryParam(orig.decayRate, 0.85f, 0.9999f, 0.005f),
                        attackRate: VaryParam(orig.attackRate, 0.001f, 0.1f, 0.01f),
                        vibratoSpeed: VaryParam(orig.vibratoSpeed, 0f, 50f, 3f),
                        vibratoDepth: VaryParam(orig.vibratoDepth, 0f, 200f, 15f),
                        tremoloSpeed: VaryParam(orig.tremoloSpeed, 0f, 50f, 3f),
                        tremoloDepth: VaryParam(orig.tremoloDepth, 0f, 1f, 0.1f),
                        dutyCycle: VaryParam(orig.dutyCycle, 0.1f, 0.9f, 0.1f),
                        harmonics: VaryParam(orig.harmonics, 0f, 1f, 0.15f),
                        filterCutoff: VaryParam(orig.filterCutoff, 0f, 1f, 0.1f),
                        filterResonance: VaryParam(orig.filterResonance, 0f, 1f, 0.1f),
                        bitCrush: VaryParam(orig.bitCrush, 1f, 16f, 2f),
                        fmAmount: VaryParam(orig.fmAmount, 0f, 500f, 30f),
                        fmRatio: VaryParam(orig.fmRatio, 0.5f, 5f, 0.3f),
                        pitchRandomness: VaryParam(orig.pitchRandomness, 0f, 1f, 0.1f),
                        ampRandomness: VaryParam(orig.ampRandomness, 0f, 1f, 0.1f),
                        startDelay: VaryParam(orig.startDelay, 0f, 0.3f, 0.02f)
                    );

                    variant[j] = (layer, varied);
                }

                variations[i] = variant;
            }

            result[kvp.Key] = variations;
        }

        return result;
    }

    // Vary a parameter with minor random changes
    static float VaryParam(float original, float min, float max, float variationAmount)
    {
        float variation = UnityEngine.Random.Range(-variationAmount, variationAmount);
        float newValue = original + variation;
        return Mathf.Clamp(newValue, min, max);
    }

    public static Dictionary<string, (WaveLayer, WaveEnvelope)[]> FullList()
    {
        return new Dictionary<string, (WaveLayer, WaveEnvelope)[]>
        {
        // ===== EXISTING EXAMPLES FROM SFXMANAGER =====

        // Emit dramatic boss arrival sound
        ["BOSS_1"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 1.5f, pitchBend: -3000f, decayRate: 0.9995f, fmAmount: 1000f, fmRatio: 0.2f, tremoloSpeed: 30f, tremoloDepth: 0.9f)),
            (WaveLayer.Saw, new(intensity: 1.2f, pitchBend: -2000f, decayRate: 0.9993f, filterCutoff: 0.1f, harmonics: 1f)),
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: -1000f, decayRate: 0.9996f, filterCutoff: 0.2f, ampRandomness: 0.9f))
        },

        ["BOSS_2"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 1.5f, pitchBend: -3000f, decayRate: 0.9995f, fmAmount: 1000f, fmRatio: 0.2f, tremoloSpeed: 30f, tremoloDepth: 0.9f)),
            (WaveLayer.Saw, new(intensity: 1.2f, pitchBend: -2000f, decayRate: 0.9993f, filterCutoff: 0.1f, harmonics: 1f)),
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: -1000f, decayRate: 0.9996f, filterCutoff: 0.2f, ampRandomness: 0.9f))
        },

        ["Explosion"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.92f, attackRate: 0.001f, filterCutoff: 0.3f, bitCrush: 6f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -100f, decayRate: 0.95f, filterCutoff: 0.2f, harmonics: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.98f, tremoloSpeed: 30f, tremoloDepth: 0.5f))
        },

        ["PowerUp"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 500f, decayRate: 0.9995f, vibratoSpeed: 6f, vibratoDepth: 10f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 500f, decayRate: 0.9995f, startDelay: 0.05f))
        },

        ["Glitch"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: -800f, decayRate: 0.94f, bitCrush: 4f, fmAmount: 200f, fmRatio: 2.5f, pitchRandomness: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.4f, decayRate: 0.93f, bitCrush: 3f, filterCutoff: 0.5f))
        },

        ["Coin"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.995f, harmonics: 0.3f)),
            (WaveLayer.Pulse, new(intensity: 0.4f, pitchBend: 200f, decayRate: 0.995f, startDelay: 0.05f, dutyCycle: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 400f, decayRate: 0.993f, startDelay: 0.1f))
        },

        ["Siren"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 2f, vibratoDepth: 200f, attackRate: 0.01f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 2f, vibratoDepth: 200f, attackRate: 0.01f))
        },

        ["Shield"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.97f, filterCutoff: 0.6f, filterResonance: 0.8f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 100f, decayRate: 0.98f, vibratoSpeed: 15f, vibratoDepth: 30f))
        },

        ["Jump"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -600f, decayRate: 0.985f, attackRate: 0.02f, harmonics: 0.2f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -600f, decayRate: 0.99f))
        },

        ["Sparkle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.994f, vibratoSpeed: 12f, vibratoDepth: 50f, pitchRandomness: 0.2f, ampRandomness: 0.3f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 1200f, decayRate: 0.99f, startDelay: 0.03f, harmonics: 0.5f))
        },

        ["DeepBass"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: -30f, decayRate: 0.995f, fmAmount: 100f, fmRatio: 0.5f, harmonics: 0.3f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -30f, decayRate: 0.996f, filterCutoff: 0.3f, filterResonance: 0.5f))
        },

        ["RandomVariation"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.995f, pitchRandomness: 0.5f, ampRandomness: 0.3f, dutyCycle: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.3f, decayRate: 0.97f, filterCutoff: 0.4f, pitchRandomness: 0.2f))
        },

        ["Damage"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -50f, decayRate: 0.9995f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.9995f))
        },

        ["Zap"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: -0f, decayRate: 0.96f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: -300f, decayRate: 0.98f))
        },

        ["Beep"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.999f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.999f))
        },

        // ===== 25 NEW EXAMPLES =====

        ["Teleport"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 1000f, decayRate: 0.993f, vibratoSpeed: 25f, vibratoDepth: 100f, bitCrush: 5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.7f))
        },

        ["LaserCharge"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: 800f, decayRate: 0.9997f, attackRate: 0.02f, harmonics: 0.6f, tremoloSpeed: 15f, tremoloDepth: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.9997f, dutyCycle: 0.3f))
        },

        ["MenuSelect"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 300f, decayRate: 0.99f, harmonics: 0.2f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 300f, decayRate: 0.992f, startDelay: 0.02f))
        },

        ["Footstep"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.93f, filterCutoff: 0.25f, attackRate: 0.005f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -80f, decayRate: 0.94f))
        },

        ["Alarm"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 4f, tremoloDepth: 0.8f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 4f, tremoloDepth: 0.8f))
        },

        ["Lightning"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.89f, bitCrush: 7f, pitchRandomness: 0.5f, ampRandomness: 0.6f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -500f, decayRate: 0.91f, bitCrush: 5f))
        },

        ["HealthPickup"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 400f, decayRate: 0.995f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 700f, decayRate: 0.993f, startDelay: 0.06f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1000f, decayRate: 0.99f, startDelay: 0.12f, dutyCycle: 0.5f))
        },

        ["BossDeath"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.985f, filterCutoff: 0.2f, tremoloSpeed: 10f, tremoloDepth: 0.7f)),
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: -200f, decayRate: 0.99f, fmAmount: 150f, fmRatio: 3f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.995f, vibratoSpeed: 8f, vibratoDepth: 50f))
        },

        ["Ricochet"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 600f, decayRate: 0.985f, pitchRandomness: 0.4f, harmonics: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.5f))
        },

        ["WaterSplash"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.5f, filterResonance: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -100f, decayRate: 0.97f, tremoloSpeed: 25f, tremoloDepth: 0.5f))
        },

        ["RocketLaunch"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.9995f, filterCutoff: 0.3f, attackRate: 0.02f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 200f, decayRate: 0.9997f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 100f, decayRate: 0.9998f, tremoloSpeed: 20f, tremoloDepth: 0.4f))
        },

        ["Freeze"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -800f, decayRate: 0.992f, vibratoSpeed: 10f, vibratoDepth: 80f, bitCrush: 4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: -800f, decayRate: 0.994f, filterCutoff: 0.4f))
        },

        ["Whoosh"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: -600f, decayRate: 0.98f, filterCutoff: 0.6f, filterResonance: 0.3f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: -600f, decayRate: 0.985f))
        },

        ["EnemyHit"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: -200f, decayRate: 0.99f, bitCrush: 5f, dutyCycle: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.4f))
        },

        ["LevelComplete"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, harmonics: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 200f, decayRate: 0.998f, startDelay: 0.1f, harmonics: 0.4f, dutyCycle: 0.5f)),
            (WaveLayer.Pulse, new(intensity: 0.5f, pitchBend: 400f, decayRate: 0.998f, startDelay: 0.2f, dutyCycle: 0.3f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 600f, decayRate: 0.995f, startDelay: 0.3f))
        },

        ["Bubble"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -400f, decayRate: 0.988f, vibratoSpeed: 20f, vibratoDepth: 40f, filterCutoff: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: -400f, decayRate: 0.99f, startDelay: 0.02f))
        },

        ["MachineGun"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.88f, filterCutoff: 0.3f, bitCrush: 6f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: -150f, decayRate: 0.9f, dutyCycle: 0.2f))
        },

        ["SpringBounce"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: -700f, decayRate: 0.988f, tremoloSpeed: 30f, tremoloDepth: 0.6f, harmonics: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -700f, decayRate: 0.992f, vibratoSpeed: 15f, vibratoDepth: 50f))
        },

        ["ComputerBeep"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, dutyCycle: 0.5f, bitCrush: 3f)),
            (WaveLayer.Pulse, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.997f, dutyCycle: 0.25f))
        },

        ["EngineHum"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.35f, tremoloSpeed: 12f, tremoloDepth: 0.3f, harmonics: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.25f))
        },

        ["Magic"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 500f, decayRate: 0.995f, vibratoSpeed: 8f, vibratoDepth: 60f, fmAmount: 100f, fmRatio: 3.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 500f, decayRate: 0.993f, harmonics: 0.6f, pitchRandomness: 0.2f))
        },

        ["ErrorBuzz"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.998f, dutyCycle: 0.5f, tremoloSpeed: 20f, tremoloDepth: 0.9f, bitCrush: 4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, harmonics: 0.3f))
        },

        ["Thump"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: -50f, decayRate: 0.985f, attackRate: 0.005f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.2f))
        },

        // ===== 25 ADDITIONAL EXAMPLES =====

        ["Crash"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.5f, pitchRandomness: 0.6f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -80f, decayRate: 0.97f, harmonics: 0.4f))
        },

        ["ChestOpen"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 200f, decayRate: 0.997f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 400f, decayRate: 0.995f, startDelay: 0.08f, vibratoSpeed: 5f, vibratoDepth: 20f))
        },

        ["WindGust"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: -200f, decayRate: 0.998f, filterCutoff: 0.6f, filterResonance: 0.4f, tremoloSpeed: 8f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: -200f, decayRate: 0.998f, filterCutoff: 0.5f))
        },

        ["TimeStop"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: -2000f, decayRate: 0.992f, bitCrush: 10f, fmAmount: 200f, fmRatio: 0.25f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -2000f, decayRate: 0.994f, dutyCycle: 0.5f, bitCrush: 8f))
        },

        ["Punch"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.88f, filterCutoff: 0.3f, attackRate: 0.003f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.91f, attackRate: 0.005f))
        },

        ["DoubleJump"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -400f, decayRate: 0.98f, harmonics: 0.4f, vibratoSpeed: 10f, vibratoDepth: 30f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: -400f, decayRate: 0.985f, dutyCycle: 0.3f))
        },

        ["GlassBreak"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.8f, filterResonance: 0.7f, pitchRandomness: 0.7f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 800f, decayRate: 0.96f, harmonics: 0.6f))
        },

        ["Achievement"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 600f, decayRate: 0.997f, vibratoSpeed: 6f, vibratoDepth: 30f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 900f, decayRate: 0.995f, startDelay: 0.1f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1200f, decayRate: 0.993f, startDelay: 0.2f, dutyCycle: 0.5f))
        },

        ["Hover"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 15f, tremoloDepth: 0.5f, filterCutoff: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 10f, vibratoDepth: 20f))
        },

        ["KeyPickup"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 500f, decayRate: 0.994f, dutyCycle: 0.5f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 800f, decayRate: 0.992f, startDelay: 0.05f))
        },

        ["MetalCling"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 400f, decayRate: 0.993f, filterResonance: 0.8f, harmonics: 0.7f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 1200f, decayRate: 0.99f, startDelay: 0.03f))
        },

        ["PortalOpen"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 1200f, decayRate: 0.997f, fmAmount: 300f, fmRatio: 4f, vibratoSpeed: 12f, vibratoDepth: 80f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.7f, tremoloSpeed: 20f, tremoloDepth: 0.6f))
        },

        ["Reload"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 100f, decayRate: 0.96f, startDelay: 0.05f, dutyCycle: 0.3f))
        },

        ["HeartBeat"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.96f, attackRate: 0.008f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.94f, startDelay: 0.15f, attackRate: 0.005f))
        },

        ["Dash"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: -300f, decayRate: 0.93f, filterCutoff: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -300f, decayRate: 0.95f, harmonics: 0.3f))
        },

        ["ShieldBreak"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.6f, pitchRandomness: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.97f, harmonics: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.98f, vibratoSpeed: 20f, vibratoDepth: 40f))
        },

        ["XPGain"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 300f, decayRate: 0.996f, dutyCycle: 0.5f, harmonics: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 600f, decayRate: 0.994f, startDelay: 0.04f))
        },

        ["Revive"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -200f, decayRate: 0.998f, vibratoSpeed: 8f, vibratoDepth: 60f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 400f, decayRate: 0.997f, startDelay: 0.15f, harmonics: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.995f, startDelay: 0.3f, dutyCycle: 0.5f))
        },

        ["Countdown"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.997f, dutyCycle: 0.5f, bitCrush: 3f)),
            (WaveLayer.Pulse, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, dutyCycle: 0.25f))
        },

        ["Stealth"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -100f, decayRate: 0.998f, vibratoSpeed: 3f, vibratoDepth: 15f, filterCutoff: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.2f, pitchBend: -100f, decayRate: 0.999f, harmonics: 0.2f))
        },

        ["QuickMenu"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 200f, decayRate: 0.99f, harmonics: 0.2f)),
            (WaveLayer.Square, new(intensity: 0.2f, pitchBend: 200f, decayRate: 0.992f, dutyCycle: 0.5f))
        },

        ["AmmoDrop"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.994f, dutyCycle: 0.3f, harmonics: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.4f))
        },

        ["Crumble"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.985f, filterCutoff: 0.4f, pitchRandomness: 0.4f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -120f, decayRate: 0.99f, harmonics: 0.3f))
        },

        ["Invincible"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 10f, tremoloDepth: 0.7f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 200f, decayRate: 0.9999f, vibratoSpeed: 8f, vibratoDepth: 40f))
        },

        ["Swipe"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -500f, decayRate: 0.975f, harmonics: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.5f))
        },

        // ===== 25 MORE EXAMPLES =====

        ["Warp"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: 1500f, decayRate: 0.991f, fmAmount: 400f, fmRatio: 5f, bitCrush: 6f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 1500f, decayRate: 0.993f, dutyCycle: 0.4f, pitchRandomness: 0.3f))
        },

        ["Sizzle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.7f, tremoloSpeed: 40f, tremoloDepth: 0.8f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9995f, filterCutoff: 0.6f, harmonics: 0.2f))
        },

        ["Earthquake"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.997f, tremoloSpeed: 3f, tremoloDepth: 0.9f, attackRate: 0.02f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.2f, ampRandomness: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -20f, decayRate: 0.998f, filterCutoff: 0.25f))
        },

        ["Whistle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 5f, vibratoDepth: 30f, attackRate: 0.03f)),
            (WaveLayer.Triangle, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.9985f, harmonics: 0.3f))
        },

        ["BombTick"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.95f, dutyCycle: 0.5f, bitCrush: 4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.96f))
        },

        ["SpaceAmbience"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, vibratoSpeed: 0.5f, vibratoDepth: 20f, harmonics: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 0.3f, vibratoDepth: 15f))
        },

        ["Squeak"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.985f, vibratoSpeed: 20f, vibratoDepth: 100f, harmonics: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.99f, vibratoSpeed: 20f, vibratoDepth: 100f))
        },

        ["Zap2"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: -700f, decayRate: 0.94f, dutyCycle: 0.2f, bitCrush: 5f, pitchRandomness: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.4f))
        },

        ["Victory"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, harmonics: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 300f, decayRate: 0.998f, startDelay: 0.15f, harmonics: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 500f, decayRate: 0.998f, startDelay: 0.3f, dutyCycle: 0.5f, harmonics: 0.4f)),
            (WaveLayer.Pulse, new(intensity: 0.4f, pitchBend: 700f, decayRate: 0.995f, startDelay: 0.45f, dutyCycle: 0.3f))
        },

        ["Gong"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.998f, tremoloSpeed: 8f, tremoloDepth: 0.4f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9985f, vibratoSpeed: 4f, vibratoDepth: 20f, harmonics: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.3f))
        },

        ["DrillLoop"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 25f, tremoloDepth: 0.6f, harmonics: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 25f, tremoloDepth: 0.3f))
        },

        ["LaserBeam"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.3f, tremoloSpeed: 50f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.4f))
        },

        ["Click"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.85f, filterCutoff: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 100f, decayRate: 0.9f, dutyCycle: 0.2f))
        },

        ["Stomp"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.98f, attackRate: 0.003f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.25f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: -60f, decayRate: 0.96f))
        },

        ["Blip"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.97f, dutyCycle: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.98f))
        },

        ["EngineStart"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9995f, filterCutoff: 0.3f, attackRate: 0.05f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 300f, decayRate: 0.9997f, harmonics: 0.4f, attackRate: 0.03f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 150f, decayRate: 0.9998f, tremoloSpeed: 10f, tremoloDepth: 0.3f))
        },

        ["BossSummon"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: -500f, decayRate: 0.997f, fmAmount: 250f, fmRatio: 2f, vibratoSpeed: 6f, vibratoDepth: 40f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -500f, decayRate: 0.998f, harmonics: 0.5f, filterCutoff: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.996f, filterCutoff: 0.5f, tremoloSpeed: 8f, tremoloDepth: 0.5f))
        },

        ["Bounce"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -500f, decayRate: 0.98f, harmonics: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -500f, decayRate: 0.985f))
        },

        ["Scanner"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.996f, dutyCycle: 0.5f, vibratoSpeed: 8f, vibratoDepth: 50f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 1000f, decayRate: 0.997f, vibratoSpeed: 8f, vibratoDepth: 50f))
        },

        ["Slice"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -400f, decayRate: 0.96f, harmonics: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.6f))
        },

        ["ForceField"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 12f, vibratoDepth: 40f, fmAmount: 80f, fmRatio: 2.5f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.6f, tremoloSpeed: 15f, tremoloDepth: 0.5f))
        },

        ["Powerdown"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -1500f, decayRate: 0.996f, harmonics: 0.4f, filterCutoff: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -1500f, decayRate: 0.997f, dutyCycle: 0.5f, bitCrush: 6f))
        },

        ["CrystalChime"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.996f, harmonics: 0.7f, vibratoSpeed: 6f, vibratoDepth: 25f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 600f, decayRate: 0.994f, startDelay: 0.04f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1200f, decayRate: 0.992f, startDelay: 0.08f, dutyCycle: 0.5f))
        },

        ["Notification"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.996f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 400f, decayRate: 0.994f, startDelay: 0.05f))
        },

        // ===== 25 ADDITIONAL EXAMPLES =====

        ["Deflect"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 700f, decayRate: 0.985f, filterResonance: 0.7f, harmonics: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.6f))
        },

        ["ChainLightning"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: -600f, decayRate: 0.92f, dutyCycle: 0.2f, bitCrush: 6f, pitchRandomness: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9f, filterCutoff: 0.5f, ampRandomness: 0.7f))
        },

        ["ComboHit"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 200f, decayRate: 0.993f, dutyCycle: 0.5f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 400f, decayRate: 0.99f, startDelay: 0.03f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 600f, decayRate: 0.987f, startDelay: 0.06f))
        },

        ["SwordUnsheathe"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -300f, decayRate: 0.985f, harmonics: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.975f, filterCutoff: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 500f, decayRate: 0.99f, startDelay: 0.08f))
        },

        ["UpgradeStation"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 5f, vibratoDepth: 30f, fmAmount: 120f, fmRatio: 3f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.5f, tremoloSpeed: 8f, tremoloDepth: 0.4f))
        },

        ["CriticalHit"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.4f, bitCrush: 5f)),
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 300f, decayRate: 0.94f, dutyCycle: 0.3f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 600f, decayRate: 0.96f, startDelay: 0.03f))
        },

        ["DoorSlam"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.3f, attackRate: 0.005f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -80f, decayRate: 0.97f, tremoloSpeed: 15f, tremoloDepth: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -40f, decayRate: 0.975f))
        },

        ["Lockpick"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.94f, harmonics: 0.6f, vibratoSpeed: 15f, vibratoDepth: 40f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.96f, dutyCycle: 0.3f))
        },

        ["TrapActivate"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: -400f, decayRate: 0.988f, dutyCycle: 0.5f, bitCrush: 4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.98f, filterCutoff: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -400f, decayRate: 0.99f, harmonics: 0.3f))
        },

        ["PoisonBubble"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -300f, decayRate: 0.992f, vibratoSpeed: 18f, vibratoDepth: 60f, filterCutoff: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: -300f, decayRate: 0.994f, harmonics: 0.4f, startDelay: 0.02f)),
            (WaveLayer.Noise, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.7f))
        },

        ["Fireball"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.4f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.97f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.98f, vibratoSpeed: 12f, vibratoDepth: 30f))
        },

        ["IceSpike"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 800f, decayRate: 0.988f, harmonics: 0.6f, filterResonance: 0.7f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 1200f, decayRate: 0.985f, startDelay: 0.02f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.975f, filterCutoff: 0.7f))
        },

        ["EnergyShield"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 10f, vibratoDepth: 50f, fmAmount: 100f, fmRatio: 2f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 12f, tremoloDepth: 0.5f))
        },

        ["MissileWarning"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 6f, tremoloDepth: 0.9f, bitCrush: 3f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 6f, tremoloDepth: 0.9f))
        },

        ["TurretDeploy"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.4f, attackRate: 0.02f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 300f, decayRate: 0.993f, harmonics: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 600f, decayRate: 0.996f, startDelay: 0.1f, dutyCycle: 0.5f))
        },

        ["GrappleHook"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 400f, decayRate: 0.995f, harmonics: 0.5f, attackRate: 0.01f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 800f, decayRate: 0.992f, startDelay: 0.05f))
        },

        ["HologramAppear"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 1000f, decayRate: 0.996f, fmAmount: 200f, fmRatio: 4f, vibratoSpeed: 15f, vibratoDepth: 80f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.997f, dutyCycle: 0.3f, pitchRandomness: 0.3f, bitCrush: 5f))
        },

        ["ClimbLedge"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.97f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 200f, decayRate: 0.98f, startDelay: 0.08f, harmonics: 0.3f))
        },

        ["TeleportBeacon"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 8f, vibratoDepth: 40f, harmonics: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 500f, decayRate: 0.997f, startDelay: 0.1f, harmonics: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.995f, startDelay: 0.2f, dutyCycle: 0.5f))
        },

        ["RobotStep"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.3f, bitCrush: 4f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.94f, dutyCycle: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -30f, decayRate: 0.96f))
        },

        ["DroneHover"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 18f, tremoloDepth: 0.5f, harmonics: 0.4f, filterCutoff: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 18f, tremoloDepth: 0.3f))
        },

        ["HackingLoop"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, bitCrush: 3f, tremoloSpeed: 10f, tremoloDepth: 0.6f)),
            (WaveLayer.Pulse, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.25f, pitchRandomness: 0.2f))
        },

        ["DataTransfer"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.997f, dutyCycle: 0.5f, bitCrush: 4f, pitchRandomness: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 800f, decayRate: 0.998f, vibratoSpeed: 20f, vibratoDepth: 60f))
        },

        ["MineBoom"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.93f, filterCutoff: 0.3f, bitCrush: 7f)),
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: -150f, decayRate: 0.96f, harmonics: 0.4f, filterCutoff: 0.25f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -80f, decayRate: 0.98f, tremoloSpeed: 25f, tremoloDepth: 0.6f))
        },

        ["SpectralVoice"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 3f, vibratoDepth: 25f, fmAmount: 80f, fmRatio: 1.5f, tremoloSpeed: 4f, tremoloDepth: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9985f, filterCutoff: 0.5f, harmonics: 0.6f))
        },

        // ===== 25 MORE ADDITIONAL EXAMPLES =====

        ["PlasmaRifle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: 200f, decayRate: 0.94f, dutyCycle: 0.3f, bitCrush: 5f, fmAmount: 150f, fmRatio: 3f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 200f, decayRate: 0.96f, harmonics: 0.4f))
        },

        ["AncientRune"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 4f, vibratoDepth: 40f, harmonics: 0.6f, fmAmount: 120f, fmRatio: 2.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 500f, decayRate: 0.997f, startDelay: 0.12f, harmonics: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.995f, startDelay: 0.24f, dutyCycle: 0.5f))
        },

        ["SlimeJump"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -400f, decayRate: 0.986f, vibratoSpeed: 25f, vibratoDepth: 60f, filterCutoff: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: -400f, decayRate: 0.99f, harmonics: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.95f, filterCutoff: 0.7f))
        },

        ["NeonFlicker"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.996f, dutyCycle: 0.5f, tremoloSpeed: 35f, tremoloDepth: 0.8f, bitCrush: 4f, pitchRandomness: 0.3f)),
            (WaveLayer.Pulse, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.997f, dutyCycle: 0.25f, ampRandomness: 0.4f))
        },

        ["BlackHoleSuck"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: -1200f, decayRate: 0.998f, fmAmount: 300f, fmRatio: 0.5f, tremoloSpeed: 10f, tremoloDepth: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -1200f, decayRate: 0.9985f, filterCutoff: 0.4f, harmonics: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9995f, filterCutoff: 0.3f, ampRandomness: 0.5f))
        },

        ["CrystalShatter"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: 1000f, decayRate: 0.96f, harmonics: 0.8f, filterResonance: 0.7f, pitchRandomness: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 1500f, decayRate: 0.97f, startDelay: 0.02f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.8f, ampRandomness: 0.6f))
        },

        ["GhostWail"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.997f, vibratoSpeed: 2f, vibratoDepth: 80f, fmAmount: 150f, fmRatio: 1.2f, tremoloSpeed: 5f, tremoloDepth: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.5f, harmonics: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9985f, filterCutoff: 0.4f))
        },

        ["JetpackBoost"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 30f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 400f, decayRate: 0.9999f, harmonics: 0.4f, attackRate: 0.02f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 200f, decayRate: 0.9999f, vibratoSpeed: 15f, vibratoDepth: 30f))
        },

        ["ChainBreak"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.4f, pitchRandomness: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 300f, decayRate: 0.96f, harmonics: 0.6f, filterResonance: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.97f, startDelay: 0.05f))
        },

        ["ElectricFence"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.2f, tremoloSpeed: 45f, tremoloDepth: 0.7f, bitCrush: 5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.5f, tremoloSpeed: 45f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.3f))
        },

        ["LaserSword"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.3f, tremoloSpeed: 60f, tremoloDepth: 0.4f, harmonics: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 8f, vibratoDepth: 20f, harmonics: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, fmAmount: 80f, fmRatio: 2f))
        },

        ["MeteorImpact"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.93f, filterCutoff: 0.3f, bitCrush: 7f, ampRandomness: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: -100f, decayRate: 0.96f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -150f, decayRate: 0.97f, harmonics: 0.4f, filterCutoff: 0.25f))
        },

        ["PhaseShift"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 800f, decayRate: 0.994f, fmAmount: 350f, fmRatio: 4.5f, vibratoSpeed: 20f, vibratoDepth: 100f, bitCrush: 6f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 800f, decayRate: 0.996f, dutyCycle: 0.4f, pitchRandomness: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.997f, startDelay: 0.04f))
        },

        ["ArmorClank"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: -100f, decayRate: 0.95f, harmonics: 0.7f, filterResonance: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.93f, filterCutoff: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -80f, decayRate: 0.96f, startDelay: 0.03f))
        },

        ["DimensionRift"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: -1500f, decayRate: 0.996f, fmAmount: 400f, fmRatio: 0.3f, vibratoSpeed: 12f, vibratoDepth: 120f, bitCrush: 8f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -1500f, decayRate: 0.997f, harmonics: 0.6f, filterCutoff: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.5f, ampRandomness: 0.6f))
        },

        ["BoulderRoll"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.3f, tremoloSpeed: 8f, tremoloDepth: 0.5f, ampRandomness: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 8f, tremoloDepth: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: -30f, decayRate: 0.9999f, filterCutoff: 0.35f))
        },

        ["TeleportFail"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 500f, decayRate: 0.988f, dutyCycle: 0.5f, bitCrush: 6f, pitchRandomness: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.98f, filterCutoff: 0.6f, ampRandomness: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -500f, decayRate: 0.99f, startDelay: 0.1f))
        },

        ["PowerSurge"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: 1200f, decayRate: 0.994f, harmonics: 0.6f, tremoloSpeed: 30f, tremoloDepth: 0.7f)),
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 1200f, decayRate: 0.996f, dutyCycle: 0.3f, bitCrush: 5f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 1200f, decayRate: 0.997f, vibratoSpeed: 15f, vibratoDepth: 60f))
        },

        ["ZombieGroan"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.997f, filterCutoff: 0.4f, vibratoSpeed: 3f, vibratoDepth: 40f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -200f, decayRate: 0.998f, tremoloSpeed: 4f, tremoloDepth: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.3f))
        },

        ["CoinInsert"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.97f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.98f, startDelay: 0.04f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.99f, startDelay: 0.08f, dutyCycle: 0.5f))
        },

        ["FlameIgnite"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.5f, attackRate: 0.01f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.97f, harmonics: 0.4f, tremoloSpeed: 20f, tremoloDepth: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -50f, decayRate: 0.98f, vibratoSpeed: 15f, vibratoDepth: 30f))
        },

        ["RocketJump"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.35f, bitCrush: 6f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 300f, decayRate: 0.96f, harmonics: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.97f, startDelay: 0.05f))
        },

        ["HealingWave"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 600f, decayRate: 0.997f, vibratoSpeed: 6f, vibratoDepth: 35f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 900f, decayRate: 0.996f, startDelay: 0.08f, harmonics: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1200f, decayRate: 0.994f, startDelay: 0.16f, dutyCycle: 0.5f))
        },

        ["BatSwarm"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 30f, vibratoDepth: 80f, harmonics: 0.4f, pitchRandomness: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, filterCutoff: 0.6f, ampRandomness: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 30f, vibratoDepth: 80f))
        },

        // ===== 25 FINAL EXAMPLES =====

        ["VortexSpin"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 15f, vibratoDepth: 100f, harmonics: 0.5f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, fmAmount: 200f, fmRatio: 3f, vibratoSpeed: 15f, vibratoDepth: 100f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.5f, tremoloSpeed: 20f, tremoloDepth: 0.4f))
        },

        ["AcidSplash"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.975f, filterCutoff: 0.6f, filterResonance: 0.7f, pitchRandomness: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.98f, vibratoSpeed: 20f, vibratoDepth: 70f, tremoloSpeed: 25f, tremoloDepth: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: -200f, decayRate: 0.985f, harmonics: 0.4f))
        },

        ["CloakActivate"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -800f, decayRate: 0.995f, fmAmount: 250f, fmRatio: 1.5f, vibratoSpeed: 10f, vibratoDepth: 50f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: -800f, decayRate: 0.996f, dutyCycle: 0.3f, bitCrush: 5f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.6f))
        },

        ["SonarPing"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.996f, harmonics: 0.3f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, startDelay: 0.05f, harmonics: 0.2f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.998f, startDelay: 0.1f, dutyCycle: 0.5f))
        },

        ["LaserGatling"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 100f, decayRate: 0.9f, dutyCycle: 0.2f, bitCrush: 5f, tremoloSpeed: 100f, tremoloDepth: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 100f, decayRate: 0.92f, harmonics: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.88f, filterCutoff: 0.4f))
        },

        ["GravityWell"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: -1000f, decayRate: 0.9999f, fmAmount: 300f, fmRatio: 0.75f, vibratoSpeed: 5f, vibratoDepth: 60f, tremoloSpeed: 8f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -1000f, decayRate: 0.9999f, filterCutoff: 0.4f, harmonics: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.35f, ampRandomness: 0.4f))
        },

        ["SwordClash"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: 500f, decayRate: 0.98f, harmonics: 0.7f, filterResonance: 0.8f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.6f, pitchRandomness: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 1000f, decayRate: 0.985f, startDelay: 0.02f))
        },

        ["QuakeSlam"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.988f, tremoloSpeed: 5f, tremoloDepth: 0.9f, attackRate: 0.003f)),
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.25f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -40f, decayRate: 0.993f, harmonics: 0.4f, filterCutoff: 0.3f))
        },

        ["EnergyBarrier"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 14f, tremoloDepth: 0.6f, harmonics: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 10f, vibratoDepth: 40f, fmAmount: 90f, fmRatio: 2.5f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.6f, tremoloSpeed: 14f, tremoloDepth: 0.4f))
        },

        ["TimeBomb"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.96f, dutyCycle: 0.5f, bitCrush: 4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.97f, attackRate: 0.01f)),
            (WaveLayer.Pulse, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.98f, dutyCycle: 0.25f))
        },

        ["NuclearSiren"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 1.5f, vibratoDepth: 300f, harmonics: 0.5f, attackRate: 0.02f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, vibratoSpeed: 1.5f, vibratoDepth: 300f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 1.5f, vibratoDepth: 300f))
        },

        ["SpiderScuttle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.5f, pitchRandomness: 0.6f, ampRandomness: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.96f, harmonics: 0.3f, vibratoSpeed: 25f, vibratoDepth: 40f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.97f, dutyCycle: 0.2f, bitCrush: 4f))
        },

        ["CrystalResonance"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.999f, vibratoSpeed: 6f, vibratoDepth: 30f, harmonics: 0.7f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 700f, decayRate: 0.998f, startDelay: 0.08f, harmonics: 0.6f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1400f, decayRate: 0.997f, startDelay: 0.16f, dutyCycle: 0.5f, harmonics: 0.5f))
        },

        ["IonCannon"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.8f, pitchBend: 1000f, decayRate: 0.996f, harmonics: 0.6f, attackRate: 0.02f, tremoloSpeed: 25f, tremoloDepth: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 1000f, decayRate: 0.997f, dutyCycle: 0.3f, bitCrush: 6f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.994f, filterCutoff: 0.5f))
        },

        ["QuantumFlux"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.997f, fmAmount: 400f, fmRatio: 5f, vibratoSpeed: 20f, vibratoDepth: 100f, pitchRandomness: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, dutyCycle: 0.5f, bitCrush: 7f, pitchRandomness: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, harmonics: 0.6f, pitchRandomness: 0.3f))
        },

        ["SawBlade"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 40f, tremoloDepth: 0.7f, harmonics: 0.6f, filterCutoff: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.3f, tremoloSpeed: 40f, tremoloDepth: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 40f, tremoloDepth: 0.3f))
        },

        ["VampireBite"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.4f, attackRate: 0.005f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -150f, decayRate: 0.96f, tremoloSpeed: 20f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.97f, harmonics: 0.3f, startDelay: 0.05f))
        },

        ["AntiGravity"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 1500f, decayRate: 0.997f, vibratoSpeed: 8f, vibratoDepth: 80f, fmAmount: 200f, fmRatio: 3.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 1500f, decayRate: 0.998f, harmonics: 0.5f, vibratoSpeed: 8f, vibratoDepth: 80f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.996f, filterCutoff: 0.7f, tremoloSpeed: 12f, tremoloDepth: 0.5f))
        },

        ["SpaceDebris"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.5f, pitchRandomness: 0.6f, ampRandomness: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.97f, harmonics: 0.5f, filterResonance: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 200f, decayRate: 0.98f, startDelay: 0.04f, harmonics: 0.4f))
        },

        ["OrbitalStrike"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.9f, pitchBend: 2000f, decayRate: 0.93f, tremoloSpeed: 30f, tremoloDepth: 0.8f)),
            (WaveLayer.Noise, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.3f, bitCrush: 7f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: -200f, decayRate: 0.95f, harmonics: 0.5f, filterCutoff: 0.25f))
        },

        ["MagneticPull"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -600f, decayRate: 0.998f, vibratoSpeed: 12f, vibratoDepth: 80f, harmonics: 0.5f, filterCutoff: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -600f, decayRate: 0.9985f, fmAmount: 150f, fmRatio: 2f, vibratoSpeed: 12f, vibratoDepth: 80f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.997f, filterCutoff: 0.6f, tremoloSpeed: 15f, tremoloDepth: 0.5f))
        },

        ["RobotTransform"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.4f, bitCrush: 5f, attackRate: 0.03f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 400f, decayRate: 0.996f, harmonics: 0.5f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 800f, decayRate: 0.997f, dutyCycle: 0.3f, startDelay: 0.1f))
        },

        ["PsychicBlast"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.994f, fmAmount: 350f, fmRatio: 4f, vibratoSpeed: 18f, vibratoDepth: 90f, tremoloSpeed: 15f, tremoloDepth: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.996f, harmonics: 0.6f, vibratoSpeed: 18f, vibratoDepth: 90f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.6f, pitchRandomness: 0.4f))
        },

        ["DragonRoar"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.8f, pitchBend: -300f, decayRate: 0.997f, filterCutoff: 0.4f, vibratoSpeed: 4f, vibratoDepth: 60f, harmonics: 0.6f, tremoloSpeed: 6f, tremoloDepth: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -300f, decayRate: 0.998f, fmAmount: 120f, fmRatio: 1.5f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.996f, filterCutoff: 0.35f, ampRandomness: 0.5f))
        },

        ["SystemReboot"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -1000f, decayRate: 0.996f, dutyCycle: 0.5f, bitCrush: 6f, attackRate: 0.02f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 500f, decayRate: 0.998f, startDelay: 0.15f, harmonics: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.997f, startDelay: 0.3f, harmonics: 0.5f))
        },

        // ===== 25 NEWEST EXAMPLES =====

        ["BlacksmithHammer"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.3f, attackRate: 0.003f)),
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: -50f, decayRate: 0.96f, harmonics: 0.7f, filterResonance: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -30f, decayRate: 0.97f, startDelay: 0.02f))
        },

        ["WindChimes"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, vibratoSpeed: 8f, vibratoDepth: 40f, harmonics: 0.7f, pitchRandomness: 0.3f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 500f, decayRate: 0.996f, startDelay: 0.05f, harmonics: 0.6f)),
            (WaveLayer.Square, new(intensity: 0.2f, pitchBend: 1000f, decayRate: 0.994f, startDelay: 0.1f, dutyCycle: 0.5f))
        },

        ["ThunderClap"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.985f, filterCutoff: 0.4f, attackRate: 0.001f, ampRandomness: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: -150f, decayRate: 0.99f, harmonics: 0.5f, tremoloSpeed: 20f, tremoloDepth: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -100f, decayRate: 0.993f, vibratoSpeed: 10f, vibratoDepth: 50f))
        },

        ["HelicopterRotor"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 12f, tremoloDepth: 0.7f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 12f, tremoloDepth: 0.6f, harmonics: 0.3f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.3f, tremoloSpeed: 12f, tremoloDepth: 0.5f))
        },

        ["SubmarineSonar"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.997f, harmonics: 0.2f, attackRate: 0.01f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, startDelay: 0.1f, harmonics: 0.15f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.999f, startDelay: 0.2f, dutyCycle: 0.5f))
        },

        ["Railgun"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.8f, pitchBend: 1500f, decayRate: 0.992f, harmonics: 0.6f, tremoloSpeed: 50f, tremoloDepth: 0.6f, attackRate: 0.005f)),
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: 1500f, decayRate: 0.994f, dutyCycle: 0.2f, bitCrush: 6f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.5f))
        },

        ["HolographicGlitch"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.994f, dutyCycle: 0.5f, bitCrush: 8f, pitchRandomness: 0.7f, ampRandomness: 0.6f)),
            (WaveLayer.Pulse, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.996f, dutyCycle: 0.25f, pitchRandomness: 0.6f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.6f, ampRandomness: 0.7f))
        },

        ["TimeRewind"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 1000f, decayRate: 0.995f, fmAmount: 300f, fmRatio: 0.5f, vibratoSpeed: 15f, vibratoDepth: 100f, bitCrush: 7f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 1000f, decayRate: 0.996f, harmonics: 0.5f, vibratoSpeed: 15f, vibratoDepth: 100f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.997f, dutyCycle: 0.4f, bitCrush: 6f))
        },

        ["ShieldRegen"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 300f, decayRate: 0.998f, vibratoSpeed: 10f, vibratoDepth: 40f, harmonics: 0.5f, attackRate: 0.02f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 600f, decayRate: 0.997f, startDelay: 0.1f, harmonics: 0.4f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 900f, decayRate: 0.996f, startDelay: 0.2f, dutyCycle: 0.5f))
        },

        ["LavaBubble"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: -300f, decayRate: 0.987f, vibratoSpeed: 15f, vibratoDepth: 50f, tremoloSpeed: 20f, tremoloDepth: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: -300f, decayRate: 0.99f, harmonics: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.985f, filterCutoff: 0.6f))
        },

        ["IceCrack"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 600f, decayRate: 0.98f, harmonics: 0.6f, filterResonance: 0.7f, pitchRandomness: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.97f, filterCutoff: 0.7f, pitchRandomness: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 1000f, decayRate: 0.985f, startDelay: 0.02f))
        },

        ["TreeFall"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.35f, attackRate: 0.01f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.993f, harmonics: 0.4f, filterCutoff: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -60f, decayRate: 0.995f, tremoloSpeed: 10f, tremoloDepth: 0.4f))
        },

        ["RockSlide"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.995f, filterCutoff: 0.4f, tremoloSpeed: 15f, tremoloDepth: 0.6f, ampRandomness: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: -150f, decayRate: 0.997f, harmonics: 0.4f, pitchRandomness: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.998f, filterCutoff: 0.35f))
        },

        ["Whirlwind"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.6f, vibratoSpeed: 10f, vibratoDepth: 50f, tremoloSpeed: 15f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 10f, vibratoDepth: 50f, harmonics: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 10f, vibratoDepth: 50f))
        },

        ["LightningStrike"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.91f, filterCutoff: 0.5f, bitCrush: 7f, pitchRandomness: 0.7f, ampRandomness: 0.7f)),
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: -400f, decayRate: 0.93f, dutyCycle: 0.2f, bitCrush: 6f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -200f, decayRate: 0.95f, harmonics: 0.5f))
        },

        ["PortalClose"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -1200f, decayRate: 0.995f, fmAmount: 300f, fmRatio: 4f, vibratoSpeed: 12f, vibratoDepth: 80f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -1200f, decayRate: 0.996f, dutyCycle: 0.4f, bitCrush: 5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.6f, tremoloSpeed: 20f, tremoloDepth: 0.6f))
        },

        ["EnergyDrain"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -800f, decayRate: 0.996f, fmAmount: 200f, fmRatio: 1.5f, tremoloSpeed: 15f, tremoloDepth: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -800f, decayRate: 0.997f, filterCutoff: 0.4f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: -800f, decayRate: 0.998f, harmonics: 0.4f))
        },

        ["PowerCore"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 6f, vibratoDepth: 30f, fmAmount: 150f, fmRatio: 2.5f, tremoloSpeed: 10f, tremoloDepth: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 10f, tremoloDepth: 0.4f, harmonics: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.6f, vibratoSpeed: 6f, vibratoDepth: 30f))
        },

        ["LaserGrid"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 8f, tremoloDepth: 0.8f, bitCrush: 4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 8f, vibratoDepth: 30f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.3f, tremoloSpeed: 8f, tremoloDepth: 0.6f))
        },

        ["SonicBoom"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.9f, pitchBend: 0f, decayRate: 0.94f, filterCutoff: 0.4f, attackRate: 0.002f, bitCrush: 6f)),
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: -100f, decayRate: 0.96f, harmonics: 0.5f, tremoloSpeed: 30f, tremoloDepth: 0.7f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -50f, decayRate: 0.97f, fmAmount: 150f, fmRatio: 2f))
        },

        ["StealthDeactivate"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 800f, decayRate: 0.995f, fmAmount: 200f, fmRatio: 3f, vibratoSpeed: 12f, vibratoDepth: 60f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.996f, harmonics: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.6f, tremoloSpeed: 15f, tremoloDepth: 0.5f))
        },

        ["TurretFire"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9f, filterCutoff: 0.35f, bitCrush: 5f)),
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: -100f, decayRate: 0.92f, dutyCycle: 0.3f, bitCrush: 4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.94f, harmonics: 0.4f))
        },

        ["CyberAttack"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.993f, dutyCycle: 0.5f, bitCrush: 8f, tremoloSpeed: 40f, tremoloDepth: 0.8f, pitchRandomness: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.99f, filterCutoff: 0.5f, bitCrush: 7f, ampRandomness: 0.6f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.995f, harmonics: 0.5f, pitchRandomness: 0.4f))
        },

        // ===== 25 ULTIMATE EXAMPLES =====

        ["NanobotSwarm"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 50f, tremoloDepth: 0.7f, bitCrush: 5f, pitchRandomness: 0.5f)),
            (WaveLayer.Noise, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.6f, tremoloSpeed: 50f, tremoloDepth: 0.5f, ampRandomness: 0.6f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 25f, vibratoDepth: 60f))
        },

        ["OceanWave"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.5f, vibratoSpeed: 2f, vibratoDepth: 30f, tremoloSpeed: 3f, tremoloDepth: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -100f, decayRate: 0.9985f, vibratoSpeed: 2f, vibratoDepth: 40f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: -50f, decayRate: 0.999f, harmonics: 0.3f))
        },

        ["GlassHarmonica"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 5f, vibratoDepth: 25f, harmonics: 0.8f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 500f, decayRate: 0.997f, startDelay: 0.06f, harmonics: 0.7f, vibratoSpeed: 5f, vibratoDepth: 25f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1000f, decayRate: 0.996f, startDelay: 0.12f, dutyCycle: 0.5f, harmonics: 0.6f))
        },

        ["AstralProjection"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 600f, decayRate: 0.997f, fmAmount: 250f, fmRatio: 3.5f, vibratoSpeed: 7f, vibratoDepth: 50f, tremoloSpeed: 5f, tremoloDepth: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 600f, decayRate: 0.998f, harmonics: 0.6f, vibratoSpeed: 7f, vibratoDepth: 50f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.996f, filterCutoff: 0.7f, tremoloSpeed: 5f, tremoloDepth: 0.3f))
        },

        ["FlamethrowerLoop"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.4f, tremoloSpeed: 35f, tremoloDepth: 0.6f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.4f, tremoloSpeed: 35f, tremoloDepth: 0.4f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: -50f, decayRate: 0.9999f, vibratoSpeed: 20f, vibratoDepth: 40f))
        },

        ["ElectromagneticPulse"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.94f, dutyCycle: 0.5f, bitCrush: 6f, tremoloSpeed: 60f, tremoloDepth: 0.9f)),
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.93f, filterCutoff: 0.5f, bitCrush: 5f, ampRandomness: 0.7f)),
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: -200f, decayRate: 0.95f, harmonics: 0.5f))
        },

        ["ChainRattle"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.96f, filterCutoff: 0.5f, pitchRandomness: 0.6f, ampRandomness: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.97f, harmonics: 0.6f, filterResonance: 0.7f, vibratoSpeed: 20f, vibratoDepth: 40f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.98f, dutyCycle: 0.3f, bitCrush: 4f))
        },

        ["MagicPortal"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, vibratoSpeed: 8f, vibratoDepth: 60f, fmAmount: 180f, fmRatio: 3f, tremoloSpeed: 10f, tremoloDepth: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.6f, vibratoSpeed: 8f, vibratoDepth: 60f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, tremoloSpeed: 10f, tremoloDepth: 0.4f))
        },

        ["SteamVent"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.997f, filterCutoff: 0.6f, tremoloSpeed: 25f, tremoloDepth: 0.6f, attackRate: 0.01f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.5f, harmonics: 0.3f)),
            (WaveLayer.Sine, new(intensity: 0.3f, pitchBend: -80f, decayRate: 0.9985f, vibratoSpeed: 12f, vibratoDepth: 30f))
        },

        ["CrystalSing"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.999f, vibratoSpeed: 4f, vibratoDepth: 20f, harmonics: 0.9f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 800f, decayRate: 0.9985f, startDelay: 0.1f, harmonics: 0.8f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1600f, decayRate: 0.998f, startDelay: 0.2f, dutyCycle: 0.5f, harmonics: 0.7f))
        },

        ["WarHorn"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.998f, filterCutoff: 0.4f, vibratoSpeed: 5f, vibratoDepth: 40f, harmonics: 0.6f)),
            (WaveLayer.Triangle, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9985f, harmonics: 0.5f, vibratoSpeed: 5f, vibratoDepth: 40f)),
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.999f, tremoloSpeed: 4f, tremoloDepth: 0.3f))
        },

        ["BioElectric"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.996f, dutyCycle: 0.3f, tremoloSpeed: 30f, tremoloDepth: 0.7f, bitCrush: 5f, pitchRandomness: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.994f, filterCutoff: 0.5f, tremoloSpeed: 30f, tremoloDepth: 0.5f)),
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, vibratoSpeed: 15f, vibratoDepth: 50f))
        },

        ["MechanicalClock"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Triangle, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.94f, harmonics: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.96f, dutyCycle: 0.3f, bitCrush: 3f)),
            (WaveLayer.Noise, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.4f))
        },

        ["ArcaneExplosion"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: 800f, decayRate: 0.93f, fmAmount: 350f, fmRatio: 4f, vibratoSpeed: 20f, vibratoDepth: 100f)),
            (WaveLayer.Triangle, new(intensity: 0.7f, pitchBend: 800f, decayRate: 0.95f, harmonics: 0.7f, vibratoSpeed: 20f, vibratoDepth: 100f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.92f, filterCutoff: 0.5f, bitCrush: 6f))
        },

        ["UnderwaterBubbles"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.99f, vibratoSpeed: 18f, vibratoDepth: 50f, filterCutoff: 0.6f, pitchRandomness: 0.4f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.992f, harmonics: 0.3f, pitchRandomness: 0.3f)),
            (WaveLayer.Noise, new(intensity: 0.2f, pitchBend: 0f, decayRate: 0.985f, filterCutoff: 0.7f, ampRandomness: 0.4f))
        },

        ["TeslaCoil"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Square, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.2f, tremoloSpeed: 45f, tremoloDepth: 0.8f, bitCrush: 6f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.5f, tremoloSpeed: 45f, tremoloDepth: 0.6f, pitchRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, harmonics: 0.5f, tremoloSpeed: 45f, tremoloDepth: 0.5f))
        },

        ["AlienLanguage"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.996f, fmAmount: 300f, fmRatio: 2.7f, vibratoSpeed: 12f, vibratoDepth: 80f, pitchRandomness: 0.4f)),
            (WaveLayer.Saw, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.997f, filterCutoff: 0.5f, harmonics: 0.6f, pitchRandomness: 0.3f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.998f, harmonics: 0.5f, vibratoSpeed: 12f, vibratoDepth: 80f))
        },

        ["GearsGrinding"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Saw, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, tremoloSpeed: 22f, tremoloDepth: 0.7f, harmonics: 0.5f, filterCutoff: 0.4f)),
            (WaveLayer.Noise, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.35f, tremoloSpeed: 22f, tremoloDepth: 0.5f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.3f, bitCrush: 4f))
        },

        ["SpiritWisp"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.998f, vibratoSpeed: 8f, vibratoDepth: 50f, harmonics: 0.6f, tremoloSpeed: 6f, tremoloDepth: 0.5f)),
            (WaveLayer.Triangle, new(intensity: 0.3f, pitchBend: 300f, decayRate: 0.9975f, harmonics: 0.5f, vibratoSpeed: 8f, vibratoDepth: 50f)),
            (WaveLayer.Square, new(intensity: 0.2f, pitchBend: 600f, decayRate: 0.997f, dutyCycle: 0.5f, startDelay: 0.05f))
        },

        ["HydraulicPress"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.7f, pitchBend: 0f, decayRate: 0.993f, filterCutoff: 0.3f, attackRate: 0.02f)),
            (WaveLayer.Sine, new(intensity: 0.6f, pitchBend: -100f, decayRate: 0.995f, tremoloSpeed: 15f, tremoloDepth: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -80f, decayRate: 0.996f, harmonics: 0.4f, filterCutoff: 0.35f))
        },

        ["RadioStatic"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.6f, tremoloSpeed: 35f, tremoloDepth: 0.7f, bitCrush: 5f)),
            (WaveLayer.Square, new(intensity: 0.4f, pitchBend: 0f, decayRate: 0.9999f, dutyCycle: 0.5f, bitCrush: 6f, pitchRandomness: 0.5f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.3f, pitchBend: 0f, decayRate: 0.9999f, filterCutoff: 0.5f, pitchRandomness: 0.4f))
        },

        ["CelestialHarmony"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.5f, pitchBend: 0f, decayRate: 0.999f, vibratoSpeed: 3f, vibratoDepth: 15f, harmonics: 0.8f)),
            (WaveLayer.Triangle, new(intensity: 0.4f, pitchBend: 700f, decayRate: 0.9985f, startDelay: 0.12f, harmonics: 0.7f, vibratoSpeed: 3f, vibratoDepth: 15f)),
            (WaveLayer.Square, new(intensity: 0.3f, pitchBend: 1200f, decayRate: 0.998f, startDelay: 0.24f, dutyCycle: 0.5f, harmonics: 0.6f)),
            (WaveLayer.Pulse, new(intensity: 0.2f, pitchBend: 1900f, decayRate: 0.9975f, startDelay: 0.36f, dutyCycle: 0.3f))
        },

        ["VolcanicRumble"] = new (WaveLayer, WaveEnvelope)[]
        {
            (WaveLayer.Sine, new(intensity: 0.8f, pitchBend: 0f, decayRate: 0.998f, tremoloSpeed: 4f, tremoloDepth: 0.8f, attackRate: 0.03f)),
            (WaveLayer.Noise, new(intensity: 0.6f, pitchBend: 0f, decayRate: 0.9985f, filterCutoff: 0.25f, tremoloSpeed: 4f, tremoloDepth: 0.6f, ampRandomness: 0.5f)),
            (WaveLayer.Saw, new(intensity: 0.5f, pitchBend: -30f, decayRate: 0.999f, filterCutoff: 0.3f, harmonics: 0.4f))
        }
    };
    }
}
