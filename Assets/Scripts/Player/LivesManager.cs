using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("Vidas")]
    [Min(1)] public int maxLives = 3;

    [Header("Interfaz existente (Canvas VictoryCanvas)")]
    public Text livesText;
    public Text messageText;
    public GameObject gameOverPanel;

    [Header("Escenas")]
    public string mainMenuScene = "MainMenu";

    [Header("Mensajes de muerte")]
    [Min(0.1f)] public float messageDuration = 2.5f;
    public string firstDeathTitle = "¡UNA VIDA MENOS!";
    public string firstDeathMessage = "El cementerio empieza a reclamarte...";
    public string firstDeathRemaining = "TE QUEDAN 2 OPORTUNIDADES";
    public string secondDeathTitle = "¡CUIDADO, VIAJERO!";
    public string secondDeathMessage = "Las sombras están cada vez más cerca...";
    public string secondDeathRemaining = "TE QUEDA 1 OPORTUNIDAD";

    public static int CurrentLives { get; private set; } = 3;
    private static bool initialized;

    // Escena descargada: distingue reinicio (misma escena) de partida nueva (menu)
    private static string lastUnloadedScene;

    // Mensaje que debe seguir siendo legible despues de recargar el nivel
    private static string pendingMessage;
    private static bool hasPendingMessage;

    private Coroutine messageRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        CurrentLives = 3;
        initialized = false;
        lastUnloadedScene = null;
        pendingMessage = null;
        hasPendingMessage = false;
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void HookSceneTracking()
    {
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    private static void HandleSceneUnloaded(Scene scene)
    {
        lastUnloadedScene = scene.name;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Si la escena anterior era otra (MainMenu), esto es una partida nueva.
        string sceneName = gameObject.scene.name;
        bool newGame = !string.IsNullOrEmpty(lastUnloadedScene) && lastUnloadedScene != sceneName;
        lastUnloadedScene = null;

        if (!initialized || newGame)
        {
            CurrentLives = maxLives;
            initialized = true;

            if (newGame)
            {
                pendingMessage = null;
                hasPendingMessage = false;
            }

            Debug.Log($"[VIDAS] Iniciaste con {CurrentLives} vidas.");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    private void Start()
    {
        // El mensaje de la muerte se vuelve a mostrar aqui para que sea legible
        // despues del reinicio automatico del nivel.
        if (hasPendingMessage && CurrentLives > 0)
        {
            ShowMessage(pendingMessage);
        }
        else if (messageText != null)
        {
            messageText.text = "";
        }

        hasPendingMessage = false;
        pendingMessage = null;
    }

    private void Update()
    {
        // En Game Over la R sigue siendo un reinicio manual completo
        if (CurrentLives <= 0 && gameOverPanel != null && gameOverPanel.activeSelf
            && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void LoseLife()
    {
        // Un segundo aviso con las vidas ya agotadas no descuenta nada
        if (CurrentLives <= 0)
            return;

        CurrentLives--;
        UpdateUI();

        if (CurrentLives == 0)
        {
            // La tercera muerte no muestra mensaje flotante: muestra Game Over
            pendingMessage = null;
            hasPendingMessage = false;
            ClearMessage();
            Debug.Log("[GAME OVER] Los muertos te han reclamado. No quedan vidas.");
            return;
        }

        string title;
        string body;
        string remaining;

        if (CurrentLives == 1)
        {
            title = secondDeathTitle;
            body = secondDeathMessage;
            remaining = secondDeathRemaining;
            Debug.Log("[VIDAS] Has perdido una vida. Te queda 1 oportunidad.");
        }
        else
        {
            title = firstDeathTitle;
            body = firstDeathMessage;
            remaining = CurrentLives == 2
                ? firstDeathRemaining
                : $"TE QUEDAN {CurrentLives} OPORTUNIDADES";
            Debug.Log($"[VIDAS] Has perdido una vida. Te quedan {CurrentLives} oportunidades.");
        }

        string full = BuildMessage(title, body, remaining);

        // Se guarda para repetirlo tras el reinicio automatico del nivel
        pendingMessage = full;
        hasPendingMessage = true;

        ShowMessage(full);
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
        Debug.Log("[RECOMPENSA] Has encontrado una vida extra.");
        return true;
    }

    public void ShowGameOver()
    {
        ClearMessage();
        pendingMessage = null;
        hasPendingMessage = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogWarning("[GAME OVER] Falta asignar GameOverPanel en el Inspector.");
    }

    // Titulo + mensaje + oportunidades en un unico Text del Canvas existente
    private static string BuildMessage(string title, string body, string remaining)
    {
        return $"<size=46><b>{title}</b></size>\n{body}\n\n<b>{remaining}</b>";
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

    private void ClearMessage()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }

        if (messageText != null)
            messageText.text = "";
    }

    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSecondsRealtime(messageDuration);

        if (messageText != null)
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
        pendingMessage = null;
        hasPendingMessage = false;

        if (Instance != null)
        {
            Instance.ClearMessage();

            if (Instance.gameOverPanel != null)
                Instance.gameOverPanel.SetActive(false);

            Instance.UpdateUI();
        }

        Debug.Log($"[VIDAS] Partida reiniciada: {CurrentLives} vidas.");
    }

    // Boton REINTENTAR del Game Over: tres vidas, cero huesitos, nivel limpio
    public void RestartGame()
    {
        ResetLives();
        LevelRestarter.RestartCurrentLevel();
    }

    // Boton MENU PRINCIPAL del Game Over: deja el sistema listo para otra partida
    public void GoToMenu()
    {
        ResetLives();
        LevelRestarter.LoadScene(mainMenuScene);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
