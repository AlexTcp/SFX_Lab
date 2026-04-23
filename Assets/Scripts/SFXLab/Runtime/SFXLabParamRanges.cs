using System.Collections.Generic;

public static class SFXLabParamRanges
{
    public enum Category { Core, Modulation, Timbre, Variance }

    public readonly struct Range
    {
        public readonly float Min;
        public readonly float Max;
        public readonly float Default;
        public Range(float min, float max, float def) { Min = min; Max = max; Default = def; }
    }

    // Ordered by category (Core → Modulation → Timbre → Variance).
    // Sliders[] in SFXLabLayerPanel is index-aligned to this.
    public static readonly (string Name, Category Category)[] FieldEntries =
    {
        ("intensity",       Category.Core),
        ("decayRate",       Category.Core),
        ("attackRate",      Category.Core),
        ("pitchBend",       Category.Core),

        ("vibratoSpeed",    Category.Modulation),
        ("vibratoDepth",    Category.Modulation),
        ("tremoloSpeed",    Category.Modulation),
        ("tremoloDepth",    Category.Modulation),
        ("fmAmount",        Category.Modulation),
        ("fmRatio",         Category.Modulation),

        ("dutyCycle",       Category.Timbre),
        ("harmonics",       Category.Timbre),
        ("filterCutoff",    Category.Timbre),
        ("filterResonance", Category.Timbre),
        ("bitCrush",        Category.Timbre),

        ("pitchRandomness", Category.Variance),
        ("ampRandomness",   Category.Variance),
        ("startDelay",      Category.Variance),
    };

    public static readonly string[] FieldOrder = BuildNames();
    static string[] BuildNames()
    {
        var names = new string[FieldEntries.Length];
        for (int i = 0; i < FieldEntries.Length; i++) names[i] = FieldEntries[i].Name;
        return names;
    }

    public static readonly IReadOnlyDictionary<string, Range> Ranges = new Dictionary<string, Range>
    {
        ["intensity"]       = new(0f,     2f,      0f),
        ["pitchBend"]       = new(-2000f, 2000f,   0f),
        ["decayRate"]       = new(0.85f,  0.9999f, 0.99f),
        ["attackRate"]      = new(0.001f, 1f,      0.01f),
        ["vibratoSpeed"]    = new(0f,     50f,     0f),
        ["vibratoDepth"]    = new(0f,     200f,    0f),
        ["tremoloSpeed"]    = new(0f,     50f,     0f),
        ["tremoloDepth"]    = new(0f,     1f,      0f),
        ["dutyCycle"]       = new(0.1f,   0.9f,    0.5f),
        ["harmonics"]       = new(0f,     1f,      0f),
        ["filterCutoff"]    = new(0f,     1f,      1f),
        ["filterResonance"] = new(0f,     1f,      0f),
        ["bitCrush"]        = new(1f,     16f,     16f),
        ["fmAmount"]        = new(0f,     1000f,   0f),
        ["fmRatio"]         = new(0.5f,   5f,      1f),
        ["pitchRandomness"] = new(0f,     1f,      0f),
        ["ampRandomness"]   = new(0f,     1f,      0f),
        ["startDelay"]      = new(0f,     0.3f,    0f),
    };
}
