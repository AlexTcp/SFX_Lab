using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class SFXLabController : MonoBehaviour
{
    // ===== Theme =====
    static readonly Color ColorBg          = new(0.07f, 0.07f, 0.09f, 1f);
    static readonly Color ColorPanelHeader = new(0.18f, 0.18f, 0.22f, 1f);
    static readonly Color ColorWidget      = new(0.22f, 0.22f, 0.28f, 1f);
    static readonly Color ColorWidgetHi    = new(0.28f, 0.28f, 0.36f, 1f);
    static readonly Color ColorAccent      = new(0.34f, 0.68f, 0.96f, 1f);
    static readonly Color ColorDanger      = new(0.90f, 0.35f, 0.35f, 1f);
    static readonly Color ColorText        = Color.white;
    static readonly Color ColorRadioRing   = new(0.45f, 0.45f, 0.52f, 1f);

    const int SmallSpriteSize     = 48;
    const int SmallCornerRadius   = 12;
    const int CircleSpriteSize    = 64;
    const float LoopIntervalSeconds = 0.5f;

    // ===== Tunables =====
    // Single multiplier applied to every UI vertical dimension (row heights,
    // padding, spacing, slider tracks, handles, etc). 1 = HTML mockup spec.
    [SerializeField] float uiScale = 1f;

    // ===== Runtime state =====
    Dictionary<string, (WaveLayer, WaveEnvelope)[]> presets;
    List<string> presetKeys;
    string selectedKey;
    float loopTimer;
    Toggle loopToggle;

    // ===== Sustain state =====
    const float DefaultVolume      = 0.8f;
    const float DefaultReleaseTime = 0.5f;

    float volume       = DefaultVolume;
    float releaseTime  = DefaultReleaseTime;

    SFXEmitter activeEmitter;

    float H(float h)  => h * uiScale;
    int   Hi(float h) => Mathf.RoundToInt(h * uiScale);

    Sprite whiteSprite;
    Sprite roundedSprite;
    Sprite circleSprite;
    TMP_FontAsset uiFont;

    void Awake()
    {
        whiteSprite = CreateWhiteSprite();
        roundedSprite = CreateRoundedSprite(SmallSpriteSize, SmallCornerRadius);
        circleSprite = CreateRoundedSprite(CircleSpriteSize, CircleSpriteSize / 2);
        uiFont = LoadDefaultTMPFont();
        if (uiFont == null)
            Debug.LogError("[SFXLab] No TMP font asset found. Open Window → TextMeshPro → Import TMP Essential Resources and re-enter Play mode.");

        EnsureEventSystem();
        EnsureAudio();
        SFXManager.SetGlobalVolume(DefaultVolume);

        presets = SFXExamples.FullList();
        presetKeys = presets.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
        selectedKey = presetKeys.FirstOrDefault();

        BuildUI();
    }

    void Update()
    {
        if (loopToggle != null && loopToggle.isOn)
        {
            loopTimer -= Time.deltaTime;
            if (loopTimer <= 0f)
            {
                Play();
                loopTimer = LoopIntervalSeconds;
            }
        }
        else
        {
            loopTimer = 0f;
        }
    }

    static TMP_FontAsset LoadDefaultTMPFont()
    {
        var asset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (asset != null) return asset;
        try { return TMP_Settings.defaultFontAsset; }
        catch (NullReferenceException) { return null; }
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    void EnsureAudio()
    {
        if (FindFirstObjectByType<AudioListener>() == null)
            new GameObject("AudioListener", typeof(AudioListener));
        _ = SFXManager.Instance;
    }

    // ================================================================
    // Top-level UI: Canvas → Root (VLG) → [Scroll, BottomBar]
    // ================================================================

    void BuildUI()
    {
        var canvasGO = new GameObject("SFXLabCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // Canvas reference matches the HTML mockup's phone-pixel space
        // (380 wide), so HTML px values translate 1:1 to source units.
        scaler.referenceResolution = new Vector2(380, 800);
        scaler.matchWidthOrHeight = 0f;

        var rootGO = MakeRect("Root", canvasGO.transform);
        var rootRT = rootGO.GetComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0, 0);
        rootRT.anchorMax = new Vector2(1, 1);
        rootRT.pivot     = new Vector2(0.5f, 0.5f);
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;
        var rootVLG = rootGO.AddComponent<VerticalLayoutGroup>();
        rootVLG.childControlWidth = true;
        rootVLG.childForceExpandWidth = true;
        rootVLG.childControlHeight = true;
        rootVLG.childForceExpandHeight = false;

        BuildPresetList(rootGO.transform);
        BuildBottomBar(rootGO.transform);
    }

    void BuildPresetList(Transform root)
    {
        var scrollGO = MakeRect("Scroll", root);
        LayoutElem(scrollGO, flexibleHeight: 1f, minHeight: H(400f));
        var scrollBg = scrollGO.AddComponent<Image>();
        scrollBg.sprite = whiteSprite;
        scrollBg.color = ColorBg;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 40f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;

        var viewportGO = MakeRect("Viewport", scrollGO.transform);
        AnchorFill(viewportGO.GetComponent<RectTransform>());
        viewportGO.AddComponent<RectMask2D>();
        scrollRect.viewport = viewportGO.GetComponent<RectTransform>();

        var contentGO = MakeRect("Content", viewportGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.spacing = 4f;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        var group = contentGO.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;

        for (int i = 0; i < presetKeys.Count; i++)
            BuildPresetRow(contentGO.transform, presetKeys[i], group, isInitial: i == 0);
    }

    void BuildPresetRow(Transform parent, string key, ToggleGroup group, bool isInitial)
    {
        var rowGO = new GameObject("PresetRow", typeof(RectTransform), typeof(Image), typeof(Toggle));
        rowGO.transform.SetParent(parent, false);
        LayoutElem(rowGO, minHeight: H(44f));

        var bg = rowGO.GetComponent<Image>();
        StyleBg(bg, ColorWidget);

        // Radio ring (always visible, left-aligned)
        var ringGO = MakeRect("Ring", rowGO.transform);
        var ringRT = ringGO.GetComponent<RectTransform>();
        ringRT.anchorMin = new Vector2(0, 0.5f);
        ringRT.anchorMax = new Vector2(0, 0.5f);
        ringRT.pivot = new Vector2(0, 0.5f);
        ringRT.sizeDelta = new Vector2(H(16f), H(16f));
        ringRT.anchoredPosition = new Vector2(14, 0);
        var ringImg = ringGO.AddComponent<Image>();
        ringImg.sprite = circleSprite;
        ringImg.type = Image.Type.Sliced;
        ringImg.color = ColorRadioRing;
        ringImg.raycastTarget = false;

        // Selected dot (toggled by Toggle.graphic — shown only when isOn)
        var dotGO = MakeRect("Dot", ringGO.transform);
        var dotRT = dotGO.GetComponent<RectTransform>();
        dotRT.anchorMin = new Vector2(0.22f, 0.22f);
        dotRT.anchorMax = new Vector2(0.78f, 0.78f);
        dotRT.offsetMin = Vector2.zero;
        dotRT.offsetMax = Vector2.zero;
        var dotImg = dotGO.AddComponent<Image>();
        dotImg.sprite = circleSprite;
        dotImg.type = Image.Type.Sliced;
        dotImg.color = ColorAccent;
        dotImg.raycastTarget = false;

        // Label
        var label = Label(rowGO.transform, key, 14);
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(42, 0);
        lrt.offsetMax = new Vector2(-14, 0);
        label.raycastTarget = false;

        var toggle = rowGO.GetComponent<Toggle>();
        toggle.targetGraphic = bg;
        toggle.graphic = dotImg;
        var colors = toggle.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor     = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.selectedColor    = Color.white;
        toggle.colors = colors;
        toggle.group = group;
        toggle.isOn = isInitial;
        toggle.onValueChanged.AddListener(on =>
        {
            if (on)
            {
                selectedKey = key;
                bg.color = ColorWidgetHi;
                Play();
            }
            else
            {
                bg.color = ColorWidget;
            }
        });
        if (isInitial) bg.color = ColorWidgetHi;
    }

    // ================================================================
    // Bottom area: live modifier sliders, then Play/Loop/Stop row.
    // ================================================================

    void BuildBottomBar(Transform root)
    {
        var barGO = MakeRect("BottomBar", root);
        LayoutElem(barGO, minHeight: H(170f));
        var barBg = barGO.AddComponent<Image>();
        barBg.sprite = whiteSprite;
        barBg.color = ColorPanelHeader;

        var vlg = barGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(18, 18, Hi(14f), Hi(14f));
        vlg.spacing = H(6f);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        BuildSectionHeader(barGO.transform, "SUSTAIN");

        BuildModifierRow(barGO.transform, "VOLUME",  0f,    1f,  volume,      false,
            FormatPercent, v => { volume = v; SFXManager.SetGlobalVolume(v); });
        BuildModifierRow(barGO.transform, "RELEASE", 0.05f, 2f,  releaseTime, false,
            FormatSeconds, v => releaseTime = v);

        BuildSpacer(barGO.transform, 8f);
        BuildActionsRow(barGO.transform);
    }

    void BuildSectionHeader(Transform parent, string text)
    {
        var rowGO = MakeRect("SectionHeader", parent);
        LayoutElem(rowGO, minHeight: H(18f));

        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var lbl = Label(rowGO.transform, text, 10, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        lbl.color = new Color(1f, 1f, 1f, 0.55f);
        lbl.characterSpacing = 15f;
        LayoutElem(lbl.gameObject, minWidth: 100f);
        lbl.raycastTarget = false;

        var rule = MakeRect("Rule", rowGO.transform);
        LayoutElem(rule, flexibleWidth: 1f, minHeight: 1f);
        var ruleRT = rule.GetComponent<RectTransform>();
        ruleRT.anchorMin = new Vector2(0, 0.5f);
        ruleRT.anchorMax = new Vector2(1, 0.5f);
        var ruleImg = rule.AddComponent<Image>();
        ruleImg.sprite = whiteSprite;
        ruleImg.color = new Color(1f, 1f, 1f, 0.08f);
        ruleImg.raycastTarget = false;
    }

    void BuildSpacer(Transform parent, float height)
    {
        var go = MakeRect("Spacer", parent);
        LayoutElem(go, minHeight: H(height));
    }

    void BuildActionsRow(Transform parent)
    {
        var rowGO = MakeRect("Actions", parent);
        LayoutElem(rowGO, minHeight: H(48f));

        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        var playBtn = BuildButton(rowGO.transform, "▶ PLAY", 15, ColorAccent);
        LayoutElem(playBtn.gameObject, flexibleWidth: 1f, minWidth: 140f, minHeight: H(44f));
        playBtn.onClick.AddListener(Play);

        loopToggle = BuildLoopToggle(rowGO.transform);
        LayoutElem(loopToggle.gameObject, minWidth: 56f, minHeight: H(44f));

        var stopBtn = BuildButton(rowGO.transform, "■ STOP", 15, ColorDanger);
        LayoutElem(stopBtn.gameObject, minWidth: 96f, minHeight: H(44f));
        stopBtn.onClick.AddListener(Stop);
    }

    Toggle BuildLoopToggle(Transform parent)
    {
        var go = new GameObject("LoopToggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
        go.transform.SetParent(parent, false);
        var bg = go.GetComponent<Image>();
        StyleBg(bg, ColorWidget);

        var markGO = MakeRect("Check", go.transform);
        var mrt = markGO.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0.18f, 0.18f);
        mrt.anchorMax = new Vector2(0.82f, 0.82f);
        mrt.offsetMin = Vector2.zero;
        mrt.offsetMax = Vector2.zero;
        var markImg = markGO.AddComponent<Image>();
        StyleBg(markImg, ColorAccent);
        markImg.raycastTarget = false;

        var glyph = Label(go.transform, "↺", 20, TextAlignmentOptions.Center);
        var rt = glyph.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        glyph.raycastTarget = false;

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bg;
        toggle.graphic = markImg;
        toggle.isOn = false;
        return toggle;
    }

    void BuildModifierRow(Transform parent, string label, float min, float max, float initial,
                          bool wholeNumbers, Func<float, string> formatter, Action<float> onChange)
    {
        var rowGO = MakeRect("Mod_" + label, parent);
        LayoutElem(rowGO, minHeight: H(26f));
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var lbl = Label(rowGO.transform, label, 11, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        lbl.color = new Color(1f, 1f, 1f, 0.78f);
        lbl.characterSpacing = 10f;
        LayoutElem(lbl.gameObject, minWidth: 70f);
        lbl.raycastTarget = false;

        var slider = BuildSlider(rowGO.transform, min, max, initial, wholeNumbers);
        LayoutElem(slider.gameObject, flexibleWidth: 1f, minHeight: H(24f));

        var valueLabel = Label(rowGO.transform, formatter(initial), 12, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        valueLabel.color = ColorAccent;
        LayoutElem(valueLabel.gameObject, minWidth: 48f);
        valueLabel.raycastTarget = false;

        slider.onValueChanged.AddListener(v =>
        {
            valueLabel.text = formatter(v);
            onChange(v);
        });
    }

    static string FormatPercent(float v) => Mathf.RoundToInt(v * 100f) + "%";
    static string FormatSeconds(float v) => v.ToString("0.00") + "s";

    Slider BuildSlider(Transform parent, float min, float max, float initial, bool wholeNumbers)
    {
        var go = new GameObject("Slider", typeof(RectTransform), typeof(Image), typeof(Slider));
        go.transform.SetParent(parent, false);
        // Slim container as the touch target — kept transparent so the visible
        // track is just the inner strip below.
        var hitArea = go.GetComponent<Image>();
        hitArea.color = new Color(0f, 0f, 0f, 0f);
        hitArea.raycastTarget = true;

        // Visual track: thin centered strip
        var trackGO = MakeRect("Track", go.transform);
        var trackRT = trackGO.GetComponent<RectTransform>();
        trackRT.anchorMin = new Vector2(0f, 0.5f);
        trackRT.anchorMax = new Vector2(1f, 0.5f);
        trackRT.pivot = new Vector2(0.5f, 0.5f);
        trackRT.sizeDelta = new Vector2(-20f, H(4f));
        var trackImg = trackGO.AddComponent<Image>();
        // Flat sprite at 4px tall — sliced rounding's borders overlap and stretch
        // ugly at this height; rounding is invisible here anyway.
        trackImg.sprite = whiteSprite;
        trackImg.type = Image.Type.Simple;
        trackImg.color = ColorWidget;
        trackImg.raycastTarget = false;

        var fillArea = MakeRect("Fill Area", trackGO.transform);
        AnchorFill(fillArea.GetComponent<RectTransform>());

        var fillGO = MakeRect("Fill", fillArea.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite = whiteSprite;
        fillImg.type = Image.Type.Simple;
        fillImg.color = ColorAccent;
        fillImg.raycastTarget = false;

        var handleArea = MakeRect("Handle Slide Area", go.transform);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(10f, 0f);
        haRT.offsetMax = new Vector2(-10f, 0f);

        var handleGO = MakeRect("Handle", handleArea.transform);
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(H(16f), H(16f));
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.sprite = circleSprite;
        handleImg.type = Image.Type.Sliced;
        handleImg.color = Color.white;

        var slider = go.GetComponent<Slider>();
        slider.targetGraphic = handleImg;
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.value = initial;
        return slider;
    }

    // ================================================================
    // Actions
    // ================================================================

    public void Play()
    {
        if (string.IsNullOrEmpty(selectedKey)) return;
        if (!presets.TryGetValue(selectedKey, out var tuples) || tuples == null || tuples.Length == 0) return;

        // Crossfade: release any currently-active sustained sound quickly so we
        // don't stack two sustained emitters on top of each other.
        if (activeEmitter != null)
            SFXManager.Instance.ReleaseSustained(activeEmitter, 0.05f);

        activeEmitter = SFXManager.Instance.EmitSustained(tuples);
    }

    public void Stop()
    {
        if (loopToggle != null) loopToggle.isOn = false;
        if (activeEmitter != null)
        {
            SFXManager.Instance.ReleaseSustained(activeEmitter, releaseTime);
            activeEmitter = null;
        }
    }

    // ================================================================
    // UI primitives
    // ================================================================

    static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(2, 2);
        var px = new Color[] { Color.white, Color.white, Color.white, Color.white };
        tex.SetPixels(px);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
    }

    // 9-slice rounded rectangle. Corners stay crisp at any size the image is stretched to.
    static Sprite CreateRoundedSprite(int size, int cornerRadius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
        var pixels = new Color[size * size];
        float r = cornerRadius;
        float cx = (size - 1) * 0.5f;
        float cy = (size - 1) * 0.5f;
        float halfSize = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - cx) - (halfSize - r);
                float dy = Mathf.Abs(y - cy) - (halfSize - r);
                float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                float inside  = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float sd = outside + inside - r;
                float alpha = Mathf.Clamp01(0.5f - sd);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        var border = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    void StyleBg(Image img, Color color)
    {
        img.sprite = roundedSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
    }

    static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void AnchorFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static LayoutElement LayoutElem(GameObject go, float minWidth = -1, float minHeight = -1, float flexibleWidth = -1, float flexibleHeight = -1)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (minWidth       >= 0) le.minWidth       = minWidth;
        if (minHeight      >= 0) le.minHeight      = minHeight;
        if (flexibleWidth  >= 0) le.flexibleWidth  = flexibleWidth;
        if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
        return le;
    }

    TextMeshProUGUI Label(Transform parent, string text, int fontSize,
                          TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft,
                          FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (uiFont != null) t.font = uiFont;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.text = text;
        t.alignment = align;
        t.color = ColorText;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    Button BuildButton(Transform parent, string text, int fontSize, Color bgColor)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        StyleBg(img, bgColor);

        var t = Label(go.transform, text, fontSize, TextAlignmentOptions.Center, FontStyles.Bold);
        var trt = t.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12, 4);
        trt.offsetMax = new Vector2(-12, -4);
        t.enableAutoSizing = true;
        t.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
        t.fontSizeMax = fontSize;
        t.overflowMode = TextOverflowModes.Ellipsis;
        t.raycastTarget = false;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.15f);
        colors.pressedColor     = Color.Lerp(bgColor, Color.black, 0.15f);
        btn.colors = colors;
        return btn;
    }
}
