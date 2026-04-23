using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    static readonly Color ColorPanel       = new(0.14f, 0.14f, 0.18f, 1f);
    static readonly Color ColorPanelHeader = new(0.18f, 0.18f, 0.22f, 1f);
    static readonly Color ColorWidget      = new(0.22f, 0.22f, 0.28f, 1f);
    static readonly Color ColorAccent      = new(0.34f, 0.68f, 0.96f, 1f);
    static readonly Color ColorAccentDim   = new(0.24f, 0.48f, 0.70f, 1f);
    static readonly Color ColorDanger      = new(0.90f, 0.35f, 0.35f, 1f);
    static readonly Color ColorText        = Color.white;
    static readonly Color ColorPlaceholder = new(1f, 1f, 1f, 0.4f);

    static readonly Color[] CategoryColors =
    {
        new(0.34f, 0.68f, 0.96f, 1f), // Core       — cyan
        new(0.86f, 0.40f, 0.86f, 1f), // Modulation — magenta
        new(0.96f, 0.68f, 0.28f, 1f), // Timbre     — amber
        new(0.40f, 0.86f, 0.50f, 1f), // Variance   — green
    };

    // ===== Runtime state =====
    Dictionary<string, (WaveLayer, WaveEnvelope)[][]> presets;
    Dictionary<string, (WaveLayer, WaveEnvelope)[]> userPresets; // raw names (no prefix)
    List<string> presetKeys;
    float loopTimer;

    const string UserPrefix = "★ ";
    const string UserPrefsKey = "SFXLab.UserPresets";

    SFXLabLayerPanel[] panels;
    TMP_Dropdown presetDropdown;
    Slider variationSlider;
    TMP_Text variationLabel;
    Toggle loopToggle;
    Slider loopIntervalSlider;
    TMP_Text loopIntervalLabel;
    TMP_InputField outputFolderField;
    TMP_InputField presetNameField;
    Button deleteButton;
    TMP_Text statusText;

    Sprite whiteSprite;
    Sprite roundedLargeSprite; // panels / bars: more visible radius
    Sprite roundedSmallSprite; // buttons / toggles / sliders: tighter radius
    TMP_FontAsset uiFont;
    TMP_DefaultControls.Resources uiResources;

    // Tune aesthetics here. First value is the source texture size; second is the corner radius
    // in native pixels. With 9-slice rendering, the corner stays that many pixels at any widget
    // size — so raise/lower these to make the whole UI feel more/less rounded.
    const int LargeSpriteSize   = 80;
    const int LargeCornerRadius = 22;
    const int SmallSpriteSize   = 48;
    const int SmallCornerRadius = 12;

    void Awake()
    {
        whiteSprite = CreateWhiteSprite();
        roundedLargeSprite = CreateRoundedSprite(LargeSpriteSize, LargeCornerRadius);
        roundedSmallSprite = CreateRoundedSprite(SmallSpriteSize, SmallCornerRadius);
        uiFont = LoadDefaultTMPFont();
        if (uiFont == null)
        {
            Debug.LogError("[SFXLab] No TMP font asset found. Open Window → TextMeshPro → Import TMP Essential Resources and re-enter Play mode.");
        }
        uiResources = new TMP_DefaultControls.Resources
        {
            standard = whiteSprite, background = whiteSprite, inputField = whiteSprite,
            knob = whiteSprite, checkmark = whiteSprite, dropdown = whiteSprite, mask = whiteSprite,
        };

        EnsureEventSystem();
        EnsureAudio();

        presets = SFXExamples.GetExamples();
        userPresets = new Dictionary<string, (WaveLayer, WaveEnvelope)[]>();
        LoadUserPresets();
        RebuildPresetKeys();

        BuildUI();

        foreach (var p in panels) p.Setup(p.Layer);

        variationSlider.onValueChanged.AddListener(v => variationLabel.text = $"Variation: {(int)v}");
        loopIntervalSlider.onValueChanged.AddListener(v => loopIntervalLabel.text = $"Loop: {v:0.00}s");
    }

    void Update()
    {
        if (loopToggle != null && loopToggle.isOn)
        {
            loopTimer -= Time.deltaTime;
            if (loopTimer <= 0f)
            {
                Play();
                loopTimer = loopIntervalSlider.value;
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
        // TMP_Settings.defaultFontAsset throws NullReferenceException (not returns null)
        // when TMP Essentials hasn't been imported, because the TMP_Settings singleton
        // itself is missing from Resources.
        try { return TMP_Settings.defaultFontAsset; }
        catch (NullReferenceException) { return null; }
    }

    // ================================================================
    // Infrastructure
    // ================================================================

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
        scaler.referenceResolution = new Vector2(1080, 1920);
        // 0.5 (log-mean) keeps the portrait layout readable on landscape desktop
        // screens — matchWidth=0 scaled everything ~1.78× on 1920×1080, blowing
        // widgets out of their min-width budgets and overflowing button text.
        scaler.matchWidthOrHeight = 0.5f;

        var rootGO = MakeRect("Root", canvasGO.transform);
        // Centered max-width column — HTML `max-width: 960px; margin: 0 auto` style.
        // Prevents slider rows from stretching the full width of a 1920-wide desktop.
        var rootRT = rootGO.GetComponent<RectTransform>();
        rootRT.anchorMin = new Vector2(0.5f, 0);
        rootRT.anchorMax = new Vector2(0.5f, 1);
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.sizeDelta = new Vector2(960, 0);
        rootRT.anchoredPosition = Vector2.zero;
        var rootVLG = rootGO.AddComponent<VerticalLayoutGroup>();
        rootVLG.spacing = 0f;
        rootVLG.padding = new RectOffset(0, 0, 0, 0);
        rootVLG.childControlWidth = true;
        rootVLG.childForceExpandWidth = true;
        rootVLG.childControlHeight = true;
        rootVLG.childForceExpandHeight = false;

        BuildScrollArea(rootGO.transform);
        BuildBottomBar(rootGO.transform);
    }

    void BuildScrollArea(Transform root)
    {
        var scrollGO = MakeRect("Scroll", root);
        LayoutElem(scrollGO, flexibleHeight: 1f, minHeight: 400f);
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
        vlg.padding = new RectOffset(16, 16, 16, 16);
        vlg.spacing = 6f;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        BuildControls(contentGO.transform);
        BuildLayerPanels(contentGO.transform);
    }

    void BuildBottomBar(Transform root)
    {
        var barGO = MakeRect("BottomBar", root);
        LayoutElem(barGO, minHeight: 56f);
        var barBg = barGO.AddComponent<Image>();
        barBg.sprite = whiteSprite;
        barBg.color = ColorPanelHeader;

        var hlg = barGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(12, 12, 6, 6);
        hlg.spacing = 8f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        // No flex on children → controls sit at their min widths, clustered in
        // the middle of the bar instead of Play bloating to fill the row.
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        var playBtn = BuildButton(barGO.transform, "▶ PLAY", 20, ColorAccent);
        LayoutElem(playBtn.gameObject, minWidth: 160f, minHeight: 44f);
        playBtn.onClick.AddListener(Play);

        loopToggle = BuildToggle(barGO.transform, "", sizePx: 36);
        LayoutElem(loopToggle.gameObject, minWidth: 44f, minHeight: 44f);
        StyleLoopToggle(loopToggle);

        var stopBtn = BuildButton(barGO.transform, "■ STOP", 18, ColorDanger);
        LayoutElem(stopBtn.gameObject, minWidth: 120f, minHeight: 44f);
        stopBtn.onClick.AddListener(Stop);
    }

    void StyleLoopToggle(Toggle t)
    {
        var glyph = Label(t.transform, "↺", 22, TextAlignmentOptions.Center);
        var rt = glyph.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        glyph.raycastTarget = false;
    }

    // ================================================================
    // Scrollable content sections
    // ================================================================

    void BuildControls(Transform parent)
    {
        SectionHeader(parent, "Presets");

        presetDropdown = BuildDropdown(parent, presetKeys);
        LayoutElem(presetDropdown.gameObject, minHeight: 54f);
        presetDropdown.onValueChanged.AddListener(_ => UpdateDeleteButtonState());

        {
            var row = HorizontalRow(parent, 48f);
            variationLabel = Label(row, "Variation: 0", 20);
            LayoutElem(variationLabel.gameObject, minWidth: 180f);
            variationSlider = BuildSlider(row, 0, 24, 0, whole: true);
            LayoutElem(variationSlider.gameObject, flexibleWidth: 1f, minHeight: 40f);
        }

        {
            var row = HorizontalRow(parent, 56f);
            var loadBtn = BuildButton(row, "Load Preset", 22, ColorAccentDim);
            LayoutElem(loadBtn.gameObject, flexibleWidth: 2f, minWidth: 180f, minHeight: 56f);
            loadBtn.onClick.AddListener(LoadPreset);
            deleteButton = BuildButton(row, "Delete", 20, ColorDanger);
            LayoutElem(deleteButton.gameObject, flexibleWidth: 1f, minWidth: 120f, minHeight: 56f);
            deleteButton.onClick.AddListener(DeleteSelectedPreset);
        }
        UpdateDeleteButtonState();

        Spacer(parent, 10f);
        SectionHeader(parent, "Playback");

        {
            var row = HorizontalRow(parent, 48f);
            loopIntervalLabel = Label(row, "Loop: 0.50s", 20);
            LayoutElem(loopIntervalLabel.gameObject, minWidth: 180f);
            loopIntervalSlider = BuildSlider(row, 0.05f, 2f, 0.5f);
            LayoutElem(loopIntervalSlider.gameObject, flexibleWidth: 1f, minHeight: 40f);
        }

        Spacer(parent, 10f);
        SectionHeader(parent, "Export");

        {
            var row = HorizontalRow(parent, 52f);
            var folderLabel = Label(row, "Folder:", 20);
            LayoutElem(folderLabel.gameObject, minWidth: 100f);
            outputFolderField = BuildInputField(row, Application.persistentDataPath);
            LayoutElem(outputFolderField.gameObject, flexibleWidth: 1f, minHeight: 52f);
        }

        {
            var row = HorizontalRow(parent, 56f);
            var exportBtn = BuildButton(row, "Export", 22, ColorAccentDim);
            LayoutElem(exportBtn.gameObject, flexibleWidth: 2f, minWidth: 180f, minHeight: 56f);
            exportBtn.onClick.AddListener(Export);
            var clearBtn = BuildButton(row, "Clear", 20, ColorWidget);
            LayoutElem(clearBtn.gameObject, flexibleWidth: 1f, minWidth: 120f, minHeight: 56f);
            clearBtn.onClick.AddListener(Clear);
        }

        {
            var row = HorizontalRow(parent, 52f);
            presetNameField = BuildInputField(row, "");
            LayoutElem(presetNameField.gameObject, flexibleWidth: 2f, minHeight: 52f);
            var ph = presetNameField.placeholder as TMP_Text;
            if (ph != null) ph.text = "Preset name...";
            var saveBtn = BuildButton(row, "Save", 20, ColorAccentDim);
            LayoutElem(saveBtn.gameObject, flexibleWidth: 1f, minWidth: 120f, minHeight: 52f);
            saveBtn.onClick.AddListener(SaveCurrentAsPreset);
        }

        statusText = Label(parent, "Ready", 18);
        LayoutElem(statusText.gameObject, minHeight: 28f);

        Spacer(parent, 14f);
        SectionHeader(parent, "Layers");
    }

    // ================================================================
    // Layer panels (collapsible) with category groups (also collapsible)
    // ================================================================

    void BuildLayerPanels(Transform parent)
    {
        var list = new List<SFXLabLayerPanel>();
        foreach (WaveLayer layer in Enum.GetValues(typeof(WaveLayer)))
            list.Add(BuildFlatLayerSliders(parent, layer));
        panels = list.ToArray();
    }

    // Flat layout: every slider for every layer lives directly in the scroll
    // content, one after another, each row labeled "<Layer>: <param>". No
    // panel, no toggle, no collapse — just rows. We still need an
    // SFXLabLayerPanel MonoBehaviour per layer so the controller's Play/Clear/
    // LoadPreset logic keeps working; it rides on a zero-sized stub object.
    SFXLabLayerPanel BuildFlatLayerSliders(Transform parent, WaveLayer layer)
    {
        var sliders = new SFXLabSlider[SFXLabParamRanges.FieldOrder.Length];
        for (int i = 0; i < SFXLabParamRanges.FieldEntries.Length; i++)
        {
            var entry = SFXLabParamRanges.FieldEntries[i];
            sliders[i] = BuildSliderRow(parent, entry.Name, $"{layer}: {entry.Name}");
        }

        var stubGO = new GameObject($"LayerStub_{layer}", typeof(RectTransform));
        stubGO.transform.SetParent(parent, false);
        stubGO.AddComponent<LayoutElement>().ignoreLayout = true;

        var panel = stubGO.AddComponent<SFXLabLayerPanel>();
        panel.Layer = layer;
        panel.LayerLabel = null;
        panel.EnableToggle = null;
        panel.Sliders = sliders;
        panel.PanelCollapsible = null;
        panel.CategoryCollapsibles = Array.Empty<SFXLabCollapsible>();
        return panel;
    }

    SFXLabSlider BuildSliderRow(Transform parent, string paramName, string labelText = null)
    {
        var row = HorizontalRow(parent, 40f);
        var label = Label(row, labelText ?? paramName, 18);
        LayoutElem(label.gameObject, minWidth: 230f);

        var range = SFXLabParamRanges.Ranges[paramName];
        var slider = BuildSlider(row, range.Min, range.Max, range.Default);
        LayoutElem(slider.gameObject, flexibleWidth: 1f, minHeight: 32f);

        var valueText = Label(row, "0", 18, TextAlignmentOptions.MidlineRight);
        LayoutElem(valueText.gameObject, minWidth: 80f);

        var c = row.gameObject.AddComponent<SFXLabSlider>();
        c.Label = label;
        c.Slider = slider;
        c.ValueText = valueText;

        // Long-press the slider handle/track to reset that param to its default.
        var lp = slider.gameObject.AddComponent<SFXLabLongPress>();
        lp.OnLongPress = () =>
        {
            c.ResetToDefault();
            SetStatus($"Reset {paramName}");
        };
        return c;
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
                // Signed-distance function for a rounded rectangle centered in the texture.
                float dx = Mathf.Abs(x - cx) - (halfSize - r);
                float dy = Mathf.Abs(y - cy) - (halfSize - r);
                float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                float inside  = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float sd = outside + inside - r;

                // 1-pixel antialiased edge: alpha 1 deep inside, 0 outside, linear across the edge.
                float alpha = Mathf.Clamp01(0.5f - sd);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        var border = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    public enum Rounding { None, Small, Large }

    void StyleBg(Image img, Color color, Rounding rounding = Rounding.Small)
    {
        img.sprite = rounding switch
        {
            Rounding.Large => roundedLargeSprite,
            Rounding.Small => roundedSmallSprite,
            _              => whiteSprite,
        };
        img.type  = rounding == Rounding.None ? Image.Type.Simple : Image.Type.Sliced;
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

    static void Spacer(Transform parent, float height)
    {
        var go = MakeRect("Spacer", parent);
        LayoutElem(go, minHeight: height);
    }

    void SectionHeader(Transform parent, string text)
    {
        var t = Label(parent, text, 26, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        t.color = ColorAccent;
        LayoutElem(t.gameObject, minHeight: 40f);
    }

    Transform HorizontalRow(Transform parent, float minHeight)
    {
        var go = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(parent, false);
        var hlg = go.GetComponent<HorizontalLayoutGroup>();
        // childControlWidth must be true for LayoutElement.minWidth / flexibleWidth to be
        // honored. Without it, children keep their default RectTransform size (100 units) and
        // buttons clip their text, sliders become un-draggable ribbons.
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.spacing = 12f;
        LayoutElem(go, minHeight: minHeight);
        return go.transform;
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
        t.overflowMode = TextOverflowModes.Overflow;
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
        // Inset the label so text can't touch the rounded corners, and let TMP
        // shrink the font if the button is narrower than the text needs.
        trt.offsetMin = new Vector2(14, 6);
        trt.offsetMax = new Vector2(-14, -6);
        t.enableAutoSizing = true;
        t.fontSizeMin = Mathf.Max(14f, fontSize * 0.55f);
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

    Slider BuildSlider(Transform parent, float min, float max, float value, bool whole = false)
    {
        var go = new GameObject("Slider", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var bg = go.GetComponent<Image>();
        StyleBg(bg, ColorWidget);

        var fillArea = MakeRect("FillArea", go.transform);
        var far = fillArea.GetComponent<RectTransform>();
        // Thicker track (40% of slider height) so it reads as a real bar, not a hairline.
        far.anchorMin = new Vector2(0, 0.3f);
        far.anchorMax = new Vector2(1, 0.7f);
        far.offsetMin = new Vector2(22, 0);
        far.offsetMax = new Vector2(-22, 0);

        var fillGO = MakeRect("Fill", fillArea.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        AnchorFill(fillRT);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.sprite = whiteSprite;
        fillImg.color = ColorAccent;

        var handleArea = MakeRect("HandleArea", go.transform);
        var hart = handleArea.GetComponent<RectTransform>();
        hart.anchorMin = Vector2.zero;
        hart.anchorMax = Vector2.one;
        hart.offsetMin = new Vector2(22, 0);
        hart.offsetMax = new Vector2(-22, 0);

        var handleGO = MakeRect("Handle", handleArea.transform);
        var handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(0, 1);
        // Handle protrudes 5px above/below the track for a comfortable grab zone
        // without blowing out the row spacing.
        handleRT.sizeDelta = new Vector2(28, 10);
        var handleImg = handleGO.AddComponent<Image>();
        StyleBg(handleImg, Color.white);

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = whole;
        slider.value = value;
        return slider;
    }

    Toggle BuildToggle(Transform parent, string text, int sizePx = 80)
    {
        var go = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        var bgGO = MakeRect("Bg", go.transform);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.5f);
        bgRT.anchorMax = new Vector2(0, 0.5f);
        bgRT.pivot = new Vector2(0, 0.5f);
        bgRT.sizeDelta = new Vector2(sizePx, sizePx);
        bgRT.anchoredPosition = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        StyleBg(bgImg, ColorWidget);

        var markGO = MakeRect("Check", bgGO.transform);
        var mrt = markGO.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0.18f, 0.18f);
        mrt.anchorMax = new Vector2(0.82f, 0.82f);
        mrt.offsetMin = Vector2.zero;
        mrt.offsetMax = Vector2.zero;
        var markImg = markGO.AddComponent<Image>();
        StyleBg(markImg, ColorAccent);

        if (!string.IsNullOrEmpty(text))
        {
            var l = Label(go.transform, text, 32);
            var lrt = l.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(sizePx + 20, 0);
            lrt.offsetMax = Vector2.zero;
        }

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = bgImg;
        toggle.graphic = markImg;
        toggle.isOn = false;
        return toggle;
    }

    TMP_Dropdown BuildDropdown(Transform parent, List<string> options)
    {
        var ddGO = TMP_DefaultControls.CreateDropdown(uiResources);
        ddGO.transform.SetParent(parent, false);

        var img = ddGO.GetComponent<Image>();
        img.color = ColorWidget;

        foreach (var t in ddGO.GetComponentsInChildren<TMP_Text>(includeInactive: true))
        {
            if (uiFont != null) t.font = uiFont;
            t.fontSize = 20;
            t.color = ColorText;
        }
        var template = ddGO.transform.Find("Template");
        if (template != null)
        {
            var tImg = template.GetComponent<Image>();
            if (tImg != null) tImg.color = ColorPanel;
            var itemBg = template.Find("Viewport/Content/Item/Item Background");
            if (itemBg != null) itemBg.GetComponent<Image>().color = ColorPanel;
            var itemCheck = template.Find("Viewport/Content/Item/Item Checkmark");
            if (itemCheck != null) itemCheck.GetComponent<Image>().color = ColorAccent;
            var tr = template.GetComponent<RectTransform>();
            if (tr != null) tr.sizeDelta = new Vector2(tr.sizeDelta.x, 320f);
            var item = template.Find("Viewport/Content/Item");
            if (item != null) item.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 48);
        }

        var dd = ddGO.GetComponent<TMP_Dropdown>();
        dd.ClearOptions();
        dd.AddOptions(options);
        return dd;
    }

    TMP_InputField BuildInputField(Transform parent, string initial)
    {
        var fieldGO = TMP_DefaultControls.CreateInputField(uiResources);
        fieldGO.transform.SetParent(parent, false);

        var img = fieldGO.GetComponent<Image>();
        img.color = ColorWidget;

        foreach (var t in fieldGO.GetComponentsInChildren<TMP_Text>(includeInactive: true))
        {
            if (uiFont != null) t.font = uiFont;
            t.fontSize = 18;
            t.color = t.gameObject.name.Contains("Placeholder") ? ColorPlaceholder : ColorText;
        }

        var field = fieldGO.GetComponent<TMP_InputField>();
        field.text = initial;
        return field;
    }

    // ================================================================
    // Actions
    // ================================================================

    public void Play()
    {
        var tuples = BuildTuples();
        if (tuples.Length == 0) { SetStatus("No layers enabled"); return; }
        SFXManager.Instance.Emit(tuples);
        SetStatus($"Played {tuples.Length} layer(s)");
    }

    public void Stop()
    {
        // Stop is a hard kill: flip Loop off first so the next loop tick can't re-trigger
        // right after StopAll fades out current sounds. Without this, the user would hear
        // silence for ~1 tick of the loop interval and then the sound would come back.
        if (loopToggle != null) loopToggle.isOn = false;
        SFXManager.Instance.StopAll(0.08f);
        SetStatus("Stopped");
    }

    public void LoadPreset()
    {
        int idx = presetDropdown.value;
        if (idx < 0 || idx >= presetKeys.Count) return;
        string key = presetKeys[idx];
        int variationIdx = Mathf.Clamp((int)variationSlider.value, 0, presets[key].Length - 1);
        var variation = presets[key][variationIdx];

        foreach (var p in panels)
        {
            p.Clear();
            p.SetCollapsed(true);
        }

        foreach (var (layer, env) in variation)
        {
            var panel = GetPanel(layer);
            if (panel == null) continue;
            panel.LoadFrom(env);
            panel.SetCollapsed(false);
        }
        SetStatus($"Loaded {key} [{variationIdx}]");
    }

    public void Export()
    {
        var tuples = BuildTuples();
        string code = BuildCodeString(tuples);
        GUIUtility.systemCopyBuffer = code;

        string folder = outputFolderField.text;
        if (!string.IsNullOrWhiteSpace(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
                string stamp = DateTime.Now.ToString("MMdd_HHmmss");
                string path = Path.Combine(folder, $"sfx_export_{stamp}.txt");
                File.WriteAllText(path, code);
                SetStatus($"Clipboard + {Path.GetFileName(path)}");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                SetStatus($"Clipboard only ({e.Message})");
                return;
            }
        }
        SetStatus("Copied to clipboard");
    }

    public void Clear()
    {
        foreach (var p in panels) p.Clear();
        SetStatus("Cleared");
    }

    SFXLabLayerPanel GetPanel(WaveLayer layer)
    {
        foreach (var p in panels) if (p.Layer == layer) return p;
        return null;
    }

    (WaveLayer, WaveEnvelope)[] BuildTuples()
    {
        var list = new List<(WaveLayer, WaveEnvelope)>();
        foreach (var p in panels)
            if (p.Enabled) list.Add((p.Layer, p.BuildEnvelope()));
        return list.ToArray();
    }

    string BuildCodeString((WaveLayer, WaveEnvelope)[] tuples)
    {
        if (tuples.Length == 0) return "// no layers enabled";
        var sb = new StringBuilder();
        sb.AppendLine("SFXManager.Instance.Emit(");
        for (int i = 0; i < tuples.Length; i++)
        {
            var (layer, env) = tuples[i];
            string suffix = i < tuples.Length - 1 ? "," : ");";
            sb.AppendLine($"    (WaveLayer.{layer}, new({FormatEnvelope(env)})){suffix}");
        }
        return sb.ToString();
    }

    static string FormatEnvelope(WaveEnvelope env)
    {
        var parts = new List<string>();
        if (env.intensity       != 0f)    parts.Add($"intensity: {env.intensity}f");
        if (env.pitchBend       != 0f)    parts.Add($"pitchBend: {env.pitchBend}f");
        if (env.decayRate       != 0.99f) parts.Add($"decayRate: {env.decayRate}f");
        if (env.attackRate      != 0.01f) parts.Add($"attackRate: {env.attackRate}f");
        if (env.vibratoSpeed    != 0f)    parts.Add($"vibratoSpeed: {env.vibratoSpeed}f");
        if (env.vibratoDepth    != 0f)    parts.Add($"vibratoDepth: {env.vibratoDepth}f");
        if (env.tremoloSpeed    != 0f)    parts.Add($"tremoloSpeed: {env.tremoloSpeed}f");
        if (env.tremoloDepth    != 0f)    parts.Add($"tremoloDepth: {env.tremoloDepth}f");
        if (env.dutyCycle       != 0.5f)  parts.Add($"dutyCycle: {env.dutyCycle}f");
        if (env.harmonics       != 0f)    parts.Add($"harmonics: {env.harmonics}f");
        if (env.filterCutoff    != 1f)    parts.Add($"filterCutoff: {env.filterCutoff}f");
        if (env.filterResonance != 0f)    parts.Add($"filterResonance: {env.filterResonance}f");
        if (env.bitCrush        != 16f)   parts.Add($"bitCrush: {env.bitCrush}f");
        if (env.fmAmount        != 0f)    parts.Add($"fmAmount: {env.fmAmount}f");
        if (env.fmRatio         != 1f)    parts.Add($"fmRatio: {env.fmRatio}f");
        if (env.pitchRandomness != 0f)    parts.Add($"pitchRandomness: {env.pitchRandomness}f");
        if (env.ampRandomness   != 0f)    parts.Add($"ampRandomness: {env.ampRandomness}f");
        if (env.startDelay      != 0f)    parts.Add($"startDelay: {env.startDelay}f");
        return string.Join(", ", parts);
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    // ================================================================
    // User presets (persisted via PlayerPrefs)
    // ================================================================

    [Serializable]
    class SavedPreset
    {
        public string name;
        public int[] layers;
        public WaveEnvelope[] envelopes;
    }

    [Serializable]
    class UserPresetDB
    {
        public List<SavedPreset> presets = new();
    }

    void RebuildPresetKeys()
    {
        presetKeys = presets.Keys.ToList();
        presetKeys.Sort((a, b) =>
        {
            bool aUser = a.StartsWith(UserPrefix);
            bool bUser = b.StartsWith(UserPrefix);
            if (aUser != bUser) return aUser ? 1 : -1; // built-ins first, users at the bottom
            return string.Compare(a, b, StringComparison.Ordinal);
        });
    }

    void LoadUserPresets()
    {
        string json = PlayerPrefs.GetString(UserPrefsKey, "");
        if (string.IsNullOrEmpty(json)) return;

        UserPresetDB db = null;
        try { db = JsonUtility.FromJson<UserPresetDB>(json); }
        catch (Exception e) { Debug.LogWarning($"[SFXLab] Corrupt user-preset store: {e.Message}"); return; }
        if (db == null || db.presets == null) return;

        foreach (var sp in db.presets)
        {
            if (sp.layers == null || sp.envelopes == null || sp.layers.Length != sp.envelopes.Length) continue;
            var tuples = new (WaveLayer, WaveEnvelope)[sp.layers.Length];
            for (int i = 0; i < sp.layers.Length; i++)
                tuples[i] = ((WaveLayer)sp.layers[i], sp.envelopes[i]);
            userPresets[sp.name] = tuples;
            presets[UserPrefix + sp.name] = new[] { tuples };
        }
    }

    void PersistUserPresets()
    {
        var db = new UserPresetDB();
        foreach (var kv in userPresets)
        {
            var tuples = kv.Value;
            var sp = new SavedPreset
            {
                name = kv.Key,
                layers = new int[tuples.Length],
                envelopes = new WaveEnvelope[tuples.Length],
            };
            for (int i = 0; i < tuples.Length; i++)
            {
                sp.layers[i] = (int)tuples[i].Item1;
                sp.envelopes[i] = tuples[i].Item2;
            }
            db.presets.Add(sp);
        }
        PlayerPrefs.SetString(UserPrefsKey, JsonUtility.ToJson(db));
        PlayerPrefs.Save();
    }

    void UpdateDeleteButtonState()
    {
        if (deleteButton == null || presetDropdown == null) return;
        int idx = presetDropdown.value;
        bool isUser = idx >= 0 && idx < presetKeys.Count && presetKeys[idx].StartsWith(UserPrefix);
        deleteButton.interactable = isUser;
    }

    public void DeleteSelectedPreset()
    {
        int idx = presetDropdown.value;
        if (idx < 0 || idx >= presetKeys.Count) return;
        string key = presetKeys[idx];
        if (!key.StartsWith(UserPrefix)) return;
        string rawName = key.Substring(UserPrefix.Length);

        userPresets.Remove(rawName);
        presets.Remove(key);
        PersistUserPresets();

        RebuildPresetKeys();
        presetDropdown.ClearOptions();
        presetDropdown.AddOptions(presetKeys);
        presetDropdown.value = 0;
        UpdateDeleteButtonState();

        SetStatus($"Deleted '{rawName}'");
    }

    public void SaveCurrentAsPreset()
    {
        string name = presetNameField != null ? presetNameField.text?.Trim() : null;
        if (string.IsNullOrWhiteSpace(name)) { SetStatus("Name required"); return; }
        if (presets.ContainsKey(name) && !presets.ContainsKey(UserPrefix + name))
        {
            SetStatus($"Name collides with built-in '{name}'");
            return;
        }

        var tuples = BuildTuples();
        if (tuples.Length == 0) { SetStatus("No layers enabled"); return; }

        userPresets[name] = tuples;
        presets[UserPrefix + name] = new[] { tuples };
        PersistUserPresets();

        RebuildPresetKeys();
        int newIdx = presetKeys.IndexOf(UserPrefix + name);
        if (presetDropdown != null)
        {
            presetDropdown.ClearOptions();
            presetDropdown.AddOptions(presetKeys);
            if (newIdx >= 0) presetDropdown.value = newIdx;
        }
        UpdateDeleteButtonState();

        SetStatus($"Saved '{name}' ({tuples.Length} layer(s))");
    }
}
