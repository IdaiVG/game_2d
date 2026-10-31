using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Vidas")]
    [Min(1)] public int maxLives = 3;

    [Header("Interfaz existente (opcional)")]
    public Text livesText;
    public Text messageText;
    public GameObject gameOverPanel;
    [Min(0f)] public float messageDuration = 2.5f;

    public static int CurrentLives { get; private set; } = 3;
    private static bool initialized;
    private static bool showRespawnMessage;
    private Coroutine messageRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CurrentLives = 3;
        initialized = false;
        showRespawnMessage = false;
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!initialized)
        {
            CurrentLives = maxLives;
            initialized = true;
            Debug.Log($"[VIDAS] Iniciaste con {CurrentLives} vidas.");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void Start()
    {
        if (showRespawnMessage && CurrentLives > 0)
        {
            ShowMessage(CurrentLives == 1
                ? "¡TE QUEDA 1 OPORTUNIDAD!"
                : $"¡TE QUEDAN {CurrentLives} OPORTUNIDADES!");
        }
        else if (messageText != null)
        {
            messageText.text = "";
        }
        showRespawnMessage = false;
    }

    public void LoseLife()
    {
        if (CurrentLives <= 0)
            return;

        CurrentLives--;
        UpdateUI();
        showRespawnMessage = CurrentLives > 0;

        if (CurrentLives == 0)
            Debug.Log("[GAME OVER] ¡LOS MUERTOS TE HAN RECLAMADO!");
        else if (CurrentLives == 1)
            Debug.LogWarning("[VIDAS] ¡Cuidado! Te queda 1 oportunidad.");
        else
            Debug.LogWarning($"[VIDAS] ¡Has perdido una vida! Te quedan {CurrentLives} oportunidades.");
    }

    public bool AddLife()
    {
        if (CurrentLives >= maxLives)
        {
            Debug.Log("[VIDAS] Ya tienes el máximo de vidas.");
            return false;
        }

        CurrentLives++;
        UpdateUI();
        Debug.Log($"[RECOMPENSA] ¡Conseguiste una vida extra! Vidas: {CurrentLives}/{maxLives}");
        ShowMessage("¡ENCONTRASTE UNA VIDA EXTRA!");
        return true;
    }

    public void ShowGameOver()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }
        if (messageText != null)
            messageText.text = "LOS MUERTOS TE HAN RECLAMADO...";
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning("[GAME OVER] Falta asignar GameOverPanel en el Inspector.");
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);
        messageText.text = message;
        messageRoutine = StartCoroutine(ClearMessageAfterDelay());
    }

    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSecondsRealtime(messageDuration);
        if (messageText != null && CurrentLives > 0)
            messageText.text = "";
        messageRoutine = null;
    }

    private void UpdateUI()
    {
        if (livesText != null)
            livesText.text = $"VIDAS: {CurrentLives}/{maxLives}";
    }

    public static void ResetLives()
    {
        CurrentLives = Instance != null ? Instance.maxLives : 3;
        initialized = true;
        showRespawnMessage = false;
        if (Instance != null)
            Instance.UpdateUI();
        Debug.Log($"[VIDAS] Partida reiniciada: {CurrentLives} vidas.");
    }

    public void RestartGame()
    {
        ResetLives();
        BoneScoreManager.ResetScore();
        SceneManager.LoadScene("Level_01");
    }

    public void GoToMenu()
    {
        ResetLives();
        BoneScoreManager.ResetScore();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
