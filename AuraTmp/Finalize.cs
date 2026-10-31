using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;

public class Finalize
{
    public static object Execute()
    {
        var sb = new StringBuilder();

        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null) return "NO_CANVAS";

        Transform overlay = canvas.transform.Find("VictoryOverlay");
        if (overlay != null) overlay.gameObject.SetActive(false);

        Transform res = canvas.transform.Find("VictoryOverlay/Panel/BonesResult");
        Text t = res != null ? res.GetComponent<Text>() : null;
        if (t != null)
        {
            t.text = "HUESITOS CONSEGUIDOS: 0\nPUNTUACIÓN TOTAL: 0";
            EditorUtility.SetDirty(t);
        }

        // Contador del HUD a cero para el estado guardado
        Transform hud = canvas.transform.Find("BoneHud");
        Transform counter = hud != null ? hud.Find("Counter") : null;
        Text c = counter != null ? counter.GetComponent<Text>() : null;
        if (c != null)
        {
            c.text = "HUESITOS: 000";
            EditorUtility.SetDirty(c);
        }

        sb.AppendLine("overlayActive=" + (overlay != null && overlay.gameObject.activeSelf));
        sb.AppendLine("resultText=" + (t != null ? t.text.Replace("\n", " | ") : "NULL"));
        sb.AppendLine("hudText=" + (c != null ? c.text : "NULL"));

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        bool saved = EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        sb.AppendLine("sceneSaved=" + saved);

        return sb.ToString();
    }
}
