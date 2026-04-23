using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SFXLabSlider : MonoBehaviour
{
    public TMP_Text Label;
    public Slider Slider;
    public TMP_Text ValueText;

    float defaultValue;
    Action<float> onValueChanged;

    public void Init(string label, float value, float min, float max, Action<float> callback)
    {
        if (Label != null) Label.text = label;
        Slider.minValue = min;
        Slider.maxValue = max;
        Slider.value = value;
        defaultValue = value;
        if (ValueText != null) ValueText.text = FormatValue(value);
        onValueChanged = callback;
        Slider.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(float v)
    {
        if (ValueText != null) ValueText.text = FormatValue(v);
        onValueChanged?.Invoke(v);
    }

    public void SetValue(float v) => Slider.value = v;
    public void ResetToDefault() => Slider.value = defaultValue;
    public float Value => Slider.value;

    static string FormatValue(float v) => v.ToString("0.####");
}
