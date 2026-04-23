using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SFXLabLayerPanel : MonoBehaviour
{
    public WaveLayer Layer;
    public TMP_Text LayerLabel;
    public Toggle EnableToggle;
    public SFXLabSlider[] Sliders;          // aligned with SFXLabParamRanges.FieldOrder

    // Phase 1 additions
    public SFXLabCollapsible PanelCollapsible;                         // whole panel collapse/expand
    public SFXLabCollapsible[] CategoryCollapsibles;                   // one per Category enum value

    WaveEnvelope current = new();

    static readonly FieldInfo[] fields = BuildFieldInfos();

    static FieldInfo[] BuildFieldInfos()
    {
        var t = typeof(WaveEnvelope);
        var names = SFXLabParamRanges.FieldOrder;
        var result = new FieldInfo[names.Length];
        for (int i = 0; i < names.Length; i++)
            result[i] = t.GetField(names[i], BindingFlags.Instance | BindingFlags.Public);
        return result;
    }

    public bool Enabled
    {
        get
        {
            if (EnableToggle != null) return EnableToggle.isOn;
            // Flat-slider UI has no per-layer toggle — infer "this layer is in
            // use" from its intensity slider. FieldOrder[0] is always intensity.
            return fields.Length > 0 && (float)fields[0].GetValue(current) > 0.0001f;
        }
    }

    public void Setup(WaveLayer layer)
    {
        Layer = layer;
        if (LayerLabel != null) LayerLabel.text = layer.ToString();

        for (int i = 0; i < fields.Length; i++)
        {
            int idx = i;
            var name = SFXLabParamRanges.FieldOrder[idx];
            var range = SFXLabParamRanges.Ranges[name];
            fields[idx].SetValue(current, range.Default);
            Sliders[idx].Init(name, range.Default, range.Min, range.Max, v => fields[idx].SetValue(current, v));
        }

        // Auto-expand the panel when the enable toggle is flipped on — makes the
        // act of enabling a layer also reveal its sliders in one tap.
        if (EnableToggle != null)
        {
            EnableToggle.onValueChanged.AddListener(on =>
            {
                if (on && PanelCollapsible != null && PanelCollapsible.Collapsed)
                    PanelCollapsible.SetCollapsed(false);
            });
        }
    }

    public WaveEnvelope BuildEnvelope()
    {
        var env = new WaveEnvelope();
        for (int i = 0; i < fields.Length; i++)
            fields[i].SetValue(env, (float)fields[i].GetValue(current));
        return env;
    }

    public void LoadFrom(WaveEnvelope src)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            float v = (float)fields[i].GetValue(src);
            Sliders[i].SetValue(v);
        }
        if (EnableToggle != null) EnableToggle.isOn = src.intensity > 0.0001f;
    }

    public void Clear()
    {
        for (int i = 0; i < fields.Length; i++)
        {
            var range = SFXLabParamRanges.Ranges[SFXLabParamRanges.FieldOrder[i]];
            Sliders[i].SetValue(range.Default);
        }
        if (EnableToggle != null) EnableToggle.isOn = false;
    }

    public void SetCollapsed(bool collapsed)
    {
        if (PanelCollapsible != null) PanelCollapsible.SetCollapsed(collapsed);
    }
}
