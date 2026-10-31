using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Contador de huesitos del HUD y textos flotantes de recompensa.
// Vive dentro del Canvas existente (VictoryCanvas), asi que no crea otro Canvas.
public class BoneHudUI : MonoBehaviour
{
    public static BoneHudUI Instance { get; private set; }

    [Header("Contador")]
    public Text counterLabel;
    public RectTransform pulseTarget;
    public string prefix = "HUESITOS: ";
    public string numberFormat = "000";

    [Header("Animacion del contador")]
    public float pulseScale = 1.2f;
    public float pulseDuration = 0.24f;

    [Header("Textos flotantes")]
    public RectTransform popupParent;
    public Font font;
    public int popupFontSize = 34;
    public Color popupColor = new Color(1f, 0.95f, 0.6f, 1f);
    public float popupRise = 62f;
    public float popupDuration = 0.9f;

    private Vector3 baseScale = Vector3.one;
    private Coroutine pulseRoutine;
    private int lastShown = -1;

    void Awake()
    {
        Instance = this;

        if (pulseTarget == null && counterLabel != null)
            pulseTarget = counterLabel.rectTransform;

        if (pulseTarget != null)
            baseScale = pulseTarget.localScale;

        if (font == null && counterLabel != null)
            font = counterLabel.font;

        if (font == null)
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (popupParent == null)
            popupParent = transform as RectTransform;
    }

    void OnEnable()
    {
        BoneScoreManager.BonesChanged += HandleBonesChanged;
        HandleBonesChanged(BoneScoreManager.TotalBones);
    }

    void OnDisable()
    {
        BoneScoreManager.BonesChanged -= HandleBonesChanged;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void HandleBonesChanged(int total)
    {
        if (counterLabel != null)
            counterLabel.text = prefix + total.ToString(numberFormat);

        // Solo late cuando el contador sube, nunca al reiniciarse a cero
        if (lastShown >= 0 && total > lastShown)
            Pulse();

        lastShown = total;
    }

    private void Pulse()
    {
        if (pulseTarget == null)
            return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float half = Mathf.Max(pulseDuration * 0.5f, 0.01f);

        float t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;

            pulseTarget.localScale = Vector3.Lerp(
                baseScale,
                baseScale * pulseScale,
                Mathf.Clamp01(t / half));

            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.unscaledDeltaTime;

            pulseTarget.localScale = Vector3.Lerp(
                baseScale * pulseScale,
                baseScale,
                Mathf.Clamp01(t / half));

            yield return null;
        }

        pulseTarget.localScale = baseScale;
        pulseRoutine = null;
    }

    // ------------------- Textos flotantes -------------------

    // Muestra "+N" junto al objeto que concedio la recompensa
    public static void ShowPopup(Vector3 worldPosition, string text)
    {
        ShowPopup(worldPosition, text, Color.clear);
    }

    public static void ShowPopup(Vector3 worldPosition, string text, Color color)
    {
        if (Instance == null)
            Instance = FindAnyObjectByType<BoneHudUI>(FindObjectsInactive.Include);

        if (Instance == null)
            return;

        Color tint = color.a <= 0f ? Instance.popupColor : color;

        Instance.SpawnPopup(worldPosition, text, tint);
    }

    private void SpawnPopup(Vector3 worldPosition, string text, Color color)
    {
        Camera cam = Camera.main;

        if (cam == null || font == null)
            return;

        GameObject go = new GameObject("BonePopup", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();

        rt.SetParent(popupParent != null ? popupParent : transform, false);
        rt.sizeDelta = new Vector2(340f, 70f);
        rt.position = cam.WorldToScreenPoint(worldPosition);
        rt.SetAsLastSibling();

        Text txt = go.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = popupFontSize;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        txt.raycastTarget = false;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.text = text;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        StartCoroutine(PopupRoutine(rt, txt, color));
    }

    private IEnumerator PopupRoutine(RectTransform rt, Text txt, Color color)
    {
        Vector3 start = rt.position;
        float elapsed = 0f;

        while (elapsed < popupDuration && rt != null)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / popupDuration);

            rt.position = start + Vector3.up * (popupRise * Mathf.SmoothStep(0f, 1f, t));
            rt.localScale = Vector3.one * Mathf.Lerp(1.18f, 1f, Mathf.Clamp01(t / 0.2f));

            float fade = 1f - Mathf.Clamp01((t - 0.55f) / 0.45f);

            Color c = color;
            c.a = color.a * fade;
            txt.color = c;

            yield return null;
        }

        if (rt != null)
            Destroy(rt.gameObject);
    }
}
