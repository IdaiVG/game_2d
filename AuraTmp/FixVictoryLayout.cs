using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;

public class FixVictoryLayout
{
    public static object Execute()
    {
        var sb = new StringBuilder();

        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null) return "NO_CANVAS";

        Transform panel = canvas.transform.Find("VictoryOverlay/Panel");
        if (panel == null) return "NO_PANEL";

        RectTransform result = panel.Find("BonesResult") as RectTransform;
        RectTransform restart = panel.Find("RestartButton") as RectTransform;
        RectTransform menu = panel.Find("MainMenuButton") as RectTransform;

        if (result == null || restart == null || menu == null) return "MISSING_CHILDREN";

        // Texto de huesitos en el hueco libre entre el subtitulo y los botones
        result.anchoredPosition = new Vector2(0f, 40f);
        result.sizeDelta = new Vector2(1000f, 84f);

        Text t = result.GetComponent<Text>();
        if (t != null) t.fontSize = 30;

        // Bajar los botones para dejar sitio sin salir del panel
        restart.anchoredPosition = new Vector2(0f, -80f);
        menu.anchoredPosition = new Vector2(0f, -230f);

        EditorUtility.SetDirty(result);
        EditorUtility.SetDirty(restart);
        EditorUtility.SetDirty(menu);

        // Reporte de huecos
        RectTransform sub = panel.Find("Subtitle") as RectTransform;
        float subBottom = sub.anchoredPosition.y - sub.sizeDelta.y * 0.5f;
        float resTop = result.anchoredPosition.y + result.sizeDelta.y * 0.5f;
        float resBottom = result.anchoredPosition.y - result.sizeDelta.y * 0.5f;
        float restartTop = restart.anchoredPosition.y + restart.sizeDelta.y * 0.5f;
        float restartBottom = restart.anchoredPosition.y - restart.sizeDelta.y * 0.5f;
        float menuTop = menu.anchoredPosition.y + menu.sizeDelta.y * 0.5f;
        float menuBottom = menu.anchoredPosition.y - menu.sizeDelta.y * 0.5f;

        sb.AppendLine("subBottom=" + subBottom + " resTop=" + resTop
            + " resBottom=" + resBottom + " restartTop=" + restartTop
            + " restartBottom=" + restartBottom + " menuTop=" + menuTop
            + " menuBottom=" + menuBottom + " panelHalf=" + (panel as RectTransform).sizeDelta.y * 0.5f);
        sb.AppendLine("gapSubtitleToResult=" + (subBottom - resTop));
        sb.AppendLine("gapResultToRestart=" + (resBottom - restartTop));
        sb.AppendLine("gapRestartToMenu=" + (restartBottom - menuTop));
        sb.AppendLine("menuBottomMargin=" + (menuBottom + (panel as RectTransform).sizeDelta.y * 0.5f));

        // La superposicion debe quedar desactivada al guardar la escena
        Transform overlay = canvas.transform.Find("VictoryOverlay");
        if (overlay != null) overlay.gameObject.SetActive(false);

        // Deja el texto con el formato real para revisarlo
        if (t != null) t.text = "HUESITOS CONSEGUIDOS: 45\nPUNTUACIÓN TOTAL: 45";

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        return sb.ToString();
    }
}
