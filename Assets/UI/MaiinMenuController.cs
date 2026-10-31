
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainGroup;
    public GameObject controlsPanel;
    public GameObject settingsPanel;

    [Header("Opciones")]
    public Slider volumeSlider;
    public Text volumeLabel;

    [Header("Animacion de entrada")]
    public CanvasGroup backgroundGroup;
    public CanvasGroup titleGroup;
    public CanvasGroup subtitleGroup;
    public float introDuration = 1.1f;

    private Button[] introButtons;
    private CanvasGroup[] buttonGroups;

    private const float BackgroundFade = 0.35f;
    private const float TitleDelay = 0.15f;
    private const float TitleFade = 0.40f;
    private const float ButtonsDelay = 0.40f;
    private const float ButtonFade = 0.35f;
    private const float ButtonStagger = 0.08f;

    void Awake()
    {
        // Evita que el menu aparezca un frame antes de la animacion
        SetAlpha(backgroundGroup, 0f);
        SetAlpha(titleGroup, 0f);
        SetAlpha(subtitleGroup, 0f);

        CacheButtons();
    }

    void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(volumeSlider.value);
        }

        StartCoroutine(PlayIntro());
    }

    void CacheButtons()
    {
        introButtons = mainGroup != null
            ? mainGroup.GetComponentsInChildren<Button>(true)
            : new Button[0];

        buttonGroups = new CanvasGroup[introButtons.Length];

        for (int i = 0; i < introButtons.Length; i++)
        {
            buttonGroups[i] = EnsureGroup(introButtons[i].gameObject);
            SetAlpha(buttonGroups[i], 0f);
        }
    }

    static CanvasGroup EnsureGroup(GameObject go)
    {
        if (go == null)
            return null;

        CanvasGroup group = go.GetComponent<CanvasGroup>();

        if (group == null)
            group = go.AddComponent<CanvasGroup>();

        return group;
    }

    static void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
            group.alpha = alpha;
    }

    static float EaseOut(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    IEnumerator PlayIntro()
    {
        int count = introButtons != null ? introButtons.Length : 0;

        // Los botones no responden hasta que termina la animacion
        for (int i = 0; i < count; i++)
        {
            if (introButtons[i] != null)
                introButtons[i].interactable = false;
        }

        float buttonsEnd = ButtonsDelay +
            ButtonStagger * Mathf.Max(0, count - 1) + ButtonFade;

        float lastEvent = Mathf.Max(
            buttonsEnd,
            Mathf.Max(BackgroundFade, TitleDelay + TitleFade)
        );

        float total = introDuration > 0f
            ? Mathf.Min(introDuration, lastEvent)
            : lastEvent;

        float startTime = Time.unscaledTime;

        while (true)
        {
            float t = Time.unscaledTime - startTime;

            // 1. El fondo aparece primero
            SetAlpha(backgroundGroup, Mathf.Clamp01(t / BackgroundFade));

            // 2. El titulo entra suavemente
            float titleK = EaseOut(Mathf.Clamp01((t - TitleDelay) / TitleFade));

            SetAlpha(titleGroup, titleK);
            SetAlpha(subtitleGroup, titleK);

            if (titleGroup != null)
            {
                titleGroup.transform.localScale = Vector3.Lerp(
                    new Vector3(0.92f, 0.92f, 1f),
                    Vector3.one,
                    titleK
                );
            }

            // 3. Los botones aparecen con fade-in y ligera ampliacion
            for (int i = 0; i < count; i++)
            {
                float k = EaseOut(Mathf.Clamp01(
                    (t - ButtonsDelay - ButtonStagger * i) / ButtonFade
                ));

                SetAlpha(buttonGroups[i], k);

                if (buttonGroups[i] != null)
                {
                    buttonGroups[i].transform.localScale = Vector3.Lerp(
                        new Vector3(0.9f, 0.9f, 1f),
                        Vector3.one,
                        k
                    );
                }
            }

            if (t >= total)
                break;

            yield return null;
        }

        // 4. Estado final: todo visible e interactuable
        SetAlpha(backgroundGroup, 1f);
        SetAlpha(titleGroup, 1f);
        SetAlpha(subtitleGroup, 1f);

        if (titleGroup != null)
            titleGroup.transform.localScale = Vector3.one;

        for (int i = 0; i < count; i++)
        {
            SetAlpha(buttonGroups[i], 1f);

            if (buttonGroups[i] != null)
                buttonGroups[i].transform.localScale = Vector3.one;

            if (introButtons[i] != null)
                introButtons[i].interactable = true;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level_01");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowControls()
    {
        if (mainGroup != null)
            mainGroup.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (mainGroup != null)
            mainGroup.SetActive(true);
    }

    public void ShowSettings()
    {
        if (mainGroup != null)
            mainGroup.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainGroup != null)
            mainGroup.SetActive(true);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);

        if (volumeLabel != null)
            volumeLabel.text = Mathf.RoundToInt(AudioListener.volume * 100f) + "%";
    }
}
