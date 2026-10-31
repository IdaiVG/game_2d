using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ShowVictory
{
    public static object Execute()
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null) return "NO_CANVAS";

        Transform overlay = canvas.transform.Find("VictoryOverlay");
        if (overlay != null) overlay.gameObject.SetActive(true);

        Transform res = canvas.transform.Find("VictoryOverlay/Panel/BonesResult");
        Text t = res != null ? res.GetComponent<Text>() : null;
        if (t != null) t.text = "HUESITOS CONSEGUIDOS: 45\nPUNTUACIÓN TOTAL: 45";

        Canvas.ForceUpdateCanvases();
        return "overlayActive=" + (overlay != null && overlay.gameObject.activeSelf);
    }
}
