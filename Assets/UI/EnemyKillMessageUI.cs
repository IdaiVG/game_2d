
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Mensaje de enemigo eliminado. Vive dentro del Canvas de la interfaz,
// asi que se queda fijo en pantalla aunque la camara siga al jugador.
public class EnemyKillMessageUI : MonoBehaviour
{
    public static EnemyKillMessageUI Instance { get; private set; }

    [Header("Referencias")]
    public CanvasGroup group;
    public Text label;

    [Header("Mensaje")]
    public string message = "¡Has matado un enemigo!";
    public float visibleDuration = 2f;
    public float fadeDuration = 0.35f;

    private Coroutine showRoutine;

    void Awake()
    {
        Instance = this;

        if (group == null)
            group = GetComponent<CanvasGroup>();

        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();

        // El mensaje nunca debe bloquear botones ni clics
        group.interactable = false;
        group.blocksRaycasts = false;

        if (label == null)
            label = GetComponentInChildren<Text>(true);

        if (label != null)
            label.text = message;

        SetAlpha(0f);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Punto de entrada para cualquier enemigo que muera
    public static void Show()
    {
        if (Instance == null)
            Instance = FindAnyObjectByType<EnemyKillMessageUI>();

        if (Instance != null)
            Instance.Display();
    }

    public void Display()
    {
        // Si el mensaje ya estaba visible se reinicia el tiempo:
        // un enemigo nuevo no pierde el suyo por culpa del anterior
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        if (label != null)
            label.text = message;

        SetAlpha(1f);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));

            yield return null;
        }

        SetAlpha(0f);

        showRoutine = null;
    }

    void SetAlpha(float alpha)
    {
        if (group != null)
            group.alpha = alpha;
    }
}
