using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Comprueba los listeners persistentes de los botones del Game Over y de victoria.
public static class CheckListeners
{
    public static object Execute()
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return "ERR: sin VictoryCanvas";

        StringBuilder sb = new StringBuilder();

        Dump(sb, canvas.transform, "GameOverPanel/Panel/GameOverRestartButton");
        Dump(sb, canvas.transform, "GameOverPanel/Panel/GameOverMenuButton");
        Dump(sb, canvas.transform, "VictoryOverlay/Panel/RestartButton");
        Dump(sb, canvas.transform, "VictoryOverlay/Panel/MainMenuButton");

        return sb.ToString();
    }

    static void Dump(StringBuilder sb, Transform canvas, string path)
    {
        Transform t = canvas.Find(path);
        if (t == null)
        {
            sb.Append(path).Append(" => NO ENCONTRADO\n");
            return;
        }

        Button button = t.GetComponent<Button>();
        int count = button.onClick.GetPersistentEventCount();

        sb.Append(path).Append(" => ").Append(count).Append(" listener(s): ");

        for (int i = 0; i < count; i++)
        {
            Object target = button.onClick.GetPersistentTarget(i);
            string method = button.onClick.GetPersistentMethodName(i);
            string targetName = target != null ? target.name + " (" + target.GetType().Name + ")" : "null";

            sb.Append("[").Append(i).Append("] ").Append(targetName).Append(".").Append(method).Append(" ");
        }

        sb.Append("| interactable=").Append(button.interactable);
        sb.Append("| raycastTarget=").Append(button.targetGraphic != null && button.targetGraphic.raycastTarget);
        sb.Append("\n");
    }
}
