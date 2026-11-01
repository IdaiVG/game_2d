using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

// Activa/desactiva el Game Over para revisarlo visualmente (no guarda escena).
public static class ToggleGameOver
{
    public static object Execute(JObject args)
    {
        bool active = args != null && args["active"] != null && (bool)args["active"];

        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return "ERR: sin VictoryCanvas";

        Transform panel = canvas.transform.Find("GameOverPanel");
        if (panel == null)
            return "ERR: sin GameOverPanel";

        panel.gameObject.SetActive(active);

        Text lives = canvas.transform.Find("LivesText").GetComponent<Text>();
        if (lives != null)
            lives.text = active ? "VIDAS: 0/3" : "VIDAS: 3/3";

        if (active)
        {
            Text message = canvas.transform.Find("MessageText").GetComponent<Text>();
            if (message != null)
                message.text = "";
        }

        return "GameOverPanel active = " + active;
    }
}
