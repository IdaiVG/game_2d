using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Reutiliza exactamente los mismos sub-sprites de boton que ya usa la pantalla
// de victoria, para no dibujar la hoja completa dentro de cada boton.
public static class FixButtonSprites
{
    public static object Execute()
    {
        Scene scene = SceneManager.GetActiveScene();

        GameObject canvasGo = GameObject.Find("VictoryCanvas");
        if (canvasGo == null)
            return "ERR: sin VictoryCanvas";

        Transform canvas = canvasGo.transform;

        Transform victoryRestart = canvas.Find("VictoryOverlay/Panel/RestartButton");
        Transform victoryMenu = canvas.Find("VictoryOverlay/Panel/MainMenuButton");
        Transform goRestart = canvas.Find("GameOverPanel/Panel/GameOverRestartButton");
        Transform goMenu = canvas.Find("GameOverPanel/Panel/GameOverMenuButton");

        if (victoryRestart == null || victoryMenu == null)
            return "ERR: sin botones de victoria";
        if (goRestart == null || goMenu == null)
            return "ERR: sin botones de Game Over";

        Image srcRestart = victoryRestart.GetComponent<Image>();
        Image srcMenu = victoryMenu.GetComponent<Image>();

        Image dstRestart = goRestart.GetComponent<Image>();
        Image dstMenu = goMenu.GetComponent<Image>();

        CopyImage(srcRestart, dstRestart);
        CopyImage(srcMenu, dstMenu);

        // Etiquetas legibles sobre el boton
        SetLabel(goRestart, 40);
        SetLabel(goMenu, 34);

        StringBuilder sb = new StringBuilder();
        sb.Append("restart sprite = ").Append(srcRestart.sprite != null ? srcRestart.sprite.name : "null");
        sb.Append(" | menu sprite = ").Append(srcMenu.sprite != null ? srcMenu.sprite.name : "null");

        EditorUtility.SetDirty(dstRestart);
        EditorUtility.SetDirty(dstMenu);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return "OK | " + sb.ToString() + " | escena guardada";
    }

    static void CopyImage(Image src, Image dst)
    {
        dst.sprite = src.sprite;
        dst.color = Color.white;
        dst.type = src.type;
        dst.preserveAspect = src.preserveAspect;
        dst.pixelsPerUnitMultiplier = src.pixelsPerUnitMultiplier;
        dst.raycastTarget = true;
    }

    static void SetLabel(Transform button, int fontSize)
    {
        Text label = button.Find("Label").GetComponent<Text>();
        if (label == null)
            return;

        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        EditorUtility.SetDirty(label);
    }
}
