using UnityEngine;
using UnityEngine.SceneManagement;

// Prueba del camino menu principal -> partida nueva.
// Se guarda FUERA de Assets para no forzar recompilacion en Play Mode.
public class MenuTest
{
    public static object Go()
    {
        BoneScoreManager.AddBones(12);

        LevelGoal goal = Object.FindAnyObjectByType<LevelGoal>();
        if (goal == null) return "sin LevelGoal";

        goal.GoToMainMenu();

        return "salida al menu solicitada con Total=" + BoneScoreManager.TotalBones;
    }

    public static object Report()
    {
        string hud = "n/a";
        if (BoneHudUI.Instance != null && BoneHudUI.Instance.counterLabel != null)
            hud = BoneHudUI.Instance.counterLabel.text;

        return "escena=" + SceneManager.GetActiveScene().name
            + " Total=" + BoneScoreManager.TotalBones
            + " HUD=" + hud;
    }

    public static object Play()
    {
        SceneManager.LoadScene("Level_01");
        return "Level_01 solicitado (misma llamada que el boton JUGAR del menu)";
    }
}
