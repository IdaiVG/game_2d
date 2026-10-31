using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Text;

public class VictoryLayout
{
    public static object Execute()
    {
        var sb = new StringBuilder();

        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null) return "NO_CANVAS";

        Transform panelTr = canvas.transform.Find("VictoryOverlay/Panel");
        if (panelTr == null) return "NO_PANEL";

        GameObject panel = panelTr.gameObject;
        RectTransform panelRT = panel.GetComponent<RectTransform>();

        sb.AppendLine("Panel size=" + panelRT.sizeDelta);
        foreach (Transform child in panelRT)
        {
            RectTransform rt = child as RectTransform;
            if (rt == null) { sb.AppendLine("  " + child.name + " (no rect)"); continue; }
            float top = rt.anchoredPosition.y + rt.sizeDelta.y * 0.5f;
            float bottom = rt.anchoredPosition.y - rt.sizeDelta.y * 0.5f;
            Text t = child.GetComponent<Text>();
            sb.AppendLine("  " + child.name
                + " anchor=" + rt.anchorMin + "->" + rt.anchorMax
                + " pivot=" + rt.pivot
                + " pos=" + rt.anchoredPosition
                + " size=" + rt.sizeDelta
                + " spanY=[" + bottom.ToString("0.0") + ".." + top.ToString("0.0") + "]"
                + (t != null ? " text='" + t.text + "' fs=" + t.fontSize : ""));
        }

        // Activar el panel para poder revisarlo visualmente
        Transform overlay = canvas.transform.Find("VictoryOverlay");
        if (overlay != null) overlay.gameObject.SetActive(true);

        return sb.ToString();
    }
}
