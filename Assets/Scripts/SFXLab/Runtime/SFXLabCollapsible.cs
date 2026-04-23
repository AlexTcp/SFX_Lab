using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SFXLabCollapsible : MonoBehaviour
{
    public GameObject Content;
    public TMP_Text Arrow;
    public bool Collapsed { get; private set; }

    public void Init(GameObject content, TMP_Text arrow, Button button, bool initiallyCollapsed)
    {
        Content = content;
        Arrow = arrow;
        if (button != null) button.onClick.AddListener(Toggle);
        SetCollapsed(initiallyCollapsed);
    }

    public void Toggle() => SetCollapsed(!Collapsed);

    public void SetCollapsed(bool collapsed)
    {
        Collapsed = collapsed;
        if (Content != null) Content.SetActive(!collapsed);
        if (Arrow != null) Arrow.text = collapsed ? "▸" : "▾";
    }
}
