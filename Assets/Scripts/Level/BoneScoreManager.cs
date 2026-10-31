using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Administrador central de huesitos. Es la unica fuente de verdad del puntaje:
// los coleccionables, las cajas, las arañas y la meta llaman aqui.
// Uso: BoneScoreManager.AddBones(5) / BoneScoreManager.AddBonesOnce("crate_A", 5)
public class BoneScoreManager : MonoBehaviour
{
    [Header("Recompensas configurables")]
    public int boneCollectibleReward = 1;
    public int crateReward = 5;
    public int spiderReward = 10;
    public int levelCompleteReward = 25;

    [Header("Puntuacion")]
    [Tooltip("Si esta activo, el contador y las claves vuelven a 0 al reiniciar el nivel (muerte, R o REINICIAR).")]
    public bool resetOnLevelRestart = true;

    [Tooltip("Si esta activo, el contador vuelve a 0 al empezar una partida nueva desde el menu.")]
    public bool resetOnNewGame = true;

    public static BoneScoreManager Instance { get; private set; }

    // El puntaje vive en un campo estatico para sobrevivir a la recarga de la escena.
    public static int TotalBones { get; private set; }

    // Se dispara cada vez que cambia el total. La interfaz se suscribe aqui.
    public static event Action<int> BonesChanged;

    // Claves de recompensas ya entregadas: evita duplicar puntos por la misma accion.
    private static readonly HashSet<string> awardedKeys = new HashSet<string>();

    // Nombre de la ultima escena descargada: distingue reinicio de partida nueva.
    private static string lastUnloadedScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        // El proyecto tiene desactivada la recarga de dominio al entrar en Play,
        // asi que los campos estaticos se limpian aqui para que cada partida
        // empiece realmente en cero.
        TotalBones = 0;
        awardedKeys.Clear();
        lastUnloadedScene = null;
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void HookSceneTracking()
    {
        SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
    }

    static void HandleSceneUnloaded(Scene scene)
    {
        lastUnloadedScene = scene.name;
    }

    void Awake()
    {
        // Evita instancias duplicadas del administrador
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        string sceneName = gameObject.scene.name;

        // Si se descargo esta misma escena es un reinicio; si viene de otra, es partida nueva
        bool isRestart = lastUnloadedScene == sceneName;
        lastUnloadedScene = null;

        bool keepScore = isRestart ? !resetOnLevelRestart : !resetOnNewGame;

        if (!keepScore)
        {
            TotalBones = 0;
            awardedKeys.Clear();
        }

        // Refresca la interfaz ya existente con el valor definitivo
        Notify();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ------------------------------------------------------------------
    // API publica
    // ------------------------------------------------------------------

    // Suma huesitos. Ignora cantidades no positivas: nunca hay puntaje negativo.
    public static void AddBones(int amount)
    {
        if (amount <= 0)
            return;

        TotalBones = Mathf.Max(0, TotalBones + amount);
        Notify();
    }

    // Suma huesitos solo la primera vez para una clave dada.
    // Devuelve true si realmente se otorgo la recompensa.
    public static bool AddBonesOnce(string key, int amount)
    {
        if (amount <= 0)
            return false;

        if (!string.IsNullOrEmpty(key))
        {
            if (awardedKeys.Contains(key))
                return false;

            awardedKeys.Add(key);
        }

        AddBones(amount);
        return true;
    }

    public static bool HasAwarded(string key)
    {
        return !string.IsNullOrEmpty(key) && awardedKeys.Contains(key);
    }

    // Limpia el puntaje Y el registro de claves.
    // Es obligatorio limpiar awardedKeys: si solo se pone el total a 0, las
    // cajas, huesitos y arañas ya reclamados no volverian a dar puntos.
    public static void ResetScore()
    {
        TotalBones = 0;
        awardedKeys.Clear();
        lastUnloadedScene = null;
        Notify();
    }

    static void Notify()
    {
        Action<int> handler = BonesChanged;

        if (handler != null)
            handler(TotalBones);
    }
}
