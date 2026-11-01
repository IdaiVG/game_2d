using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Construye el HUD de vidas y la ventana de Game Over DENTRO del Canvas
// existente (VictoryCanvas). No crea Canvas, EventSystem ni botones duplicados.
public static class FixLivesUI
{
    static Font font;
    static Sprite buttonSprite;

    public static object Execute()
    {
        Scene scene = SceneManager.GetActiveScene();

        GameObject canvasGo = GameObject.Find("VictoryCanvas");
        if (canvasGo == null)
            return "ERR: VictoryCanvas no encontrado";

        Transform canvas = canvasGo.transform;

        Transform counterTr = canvas.Find("BoneHud/Counter");
        if (counterTr == null)
            return "ERR: VictoryCanvas/BoneHud/Counter no encontrado";

        font = counterTr.GetComponent<Text>().font;
        buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prefabs/UI/Button.png");

        if (font == null)
            return "ERR: fuente del HUD no disponible";

        // ---------------- HUD: VIDAS + HUESITOS ----------------

        // Huésitos baja un poco para dejar la primera línea a las vidas
        RectTransform boneHud = canvas.Find("BoneHud") as RectTransform;
        if (boneHud != null)
        {
            boneHud.anchoredPosition = new Vector2(26f, -96f);
            EditorUtility.SetDirty(boneHud);
        }

        RectTransform livesRt = canvas.Find("LivesText") as RectTransform;
        if (livesRt == null)
            return "ERR: VictoryCanvas/LivesText no encontrado";

        Text livesText = livesRt.GetComponent<Text>();
        livesRt.anchorMin = new Vector2(0f, 1f);
        livesRt.anchorMax = new Vector2(0f, 1f);
        livesRt.pivot = new Vector2(0f, 1f);
        livesRt.anchoredPosition = new Vector2(26f, -20f);
        livesRt.sizeDelta = new Vector2(420f, 64f);

        livesText.font = font;
        livesText.fontSize = 44;
        livesText.alignment = TextAnchor.MiddleLeft;
        livesText.color = new Color(1f, 0.85f, 0.8f, 1f);
        livesText.text = "VIDAS: 3/3";
        livesText.raycastTarget = false;
        livesText.supportRichText = true;

        AddOutline(livesText.gameObject, 2f, 0.95f);
        EditorUtility.SetDirty(livesRt);
        EditorUtility.SetDirty(livesText);

        // ---------------- MENSAJES DE MUERTE ----------------

        RectTransform msgRt = canvas.Find("MessageText") as RectTransform;
        if (msgRt == null)
            return "ERR: VictoryCanvas/MessageText no encontrado";

        Text messageText = msgRt.GetComponent<Text>();
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.pivot = new Vector2(0.5f, 0.5f);
        msgRt.anchoredPosition = new Vector2(0f, 150f);
        msgRt.sizeDelta = new Vector2(1500f, 280f);

        messageText.font = font;
        messageText.fontSize = 40;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = new Color(1f, 0.87f, 0.62f, 1f);
        messageText.text = "";
        messageText.raycastTarget = false;
        messageText.supportRichText = true;
        messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        messageText.verticalOverflow = VerticalWrapMode.Overflow;

        AddOutline(messageText.gameObject, 2f, 0.95f);
        EditorUtility.SetDirty(msgRt);
        EditorUtility.SetDirty(messageText);

        // ---------------- GAME OVER ----------------

        Transform old = canvas.Find("GameOverPanel");
        if (old != null)
            Object.DestroyImmediate(old.gameObject);

        GameObject panelRoot = new GameObject("GameOverPanel", typeof(RectTransform));
        panelRoot.layer = LayerMask.NameToLayer("UI");

        RectTransform pr = (RectTransform)panelRoot.transform;
        pr.SetParent(canvas, false);
        pr.anchorMin = Vector2.zero;
        pr.anchorMax = Vector2.one;
        pr.pivot = new Vector2(0.5f, 0.5f);
        pr.anchoredPosition = Vector2.zero;
        pr.sizeDelta = Vector2.zero;

        // Fondo oscuro que bloquea los clics hacia el juego
        Image dim = MakeImage(panelRoot.transform, "Dim",
            Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.8f));
        Stretch(dim.rectTransform);

        Image panel = MakeImage(panelRoot.transform, "Panel",
            Vector2.zero, new Vector2(1120f, 720f),
            new Color(0.055f, 0.065f, 0.09f, 0.985f));

        Transform body = panel.transform;

        Text title = MakeText(body, "GameOverTitle",
            new Vector2(0f, 240f), new Vector2(1000f, 120f), 78,
            new Color(0.95f, 0.35f, 0.33f, 1f), TextAnchor.MiddleCenter,
            "<b>GAME OVER</b>");

        MakeText(body, "GameOverMainMessage",
            new Vector2(0f, 130f), new Vector2(1000f, 70f), 44,
            new Color(0.95f, 0.9f, 0.86f, 1f), TextAnchor.MiddleCenter,
            "LOS MUERTOS TE HAN RECLAMADO...");

        MakeText(body, "GameOverSecondaryMessage",
            new Vector2(0f, 15f), new Vector2(980f, 140f), 27,
            new Color(0.72f, 0.78f, 0.86f, 1f), TextAnchor.MiddleCenter,
            "El cementerio ha guardado tu alma. ¿Te atreves a volver?\n" +
            "<size=23><i>Tu aventura termina aquí. ¿Te atreves a desafiar nuevamente al cementerio?</i></size>");

        Button restartButton = MakeButton(body, "GameOverRestartButton",
            new Vector2(0f, -115f), new Vector2(460f, 100f), "REINTENTAR", 42);

        Button menuButton = MakeButton(body, "GameOverMenuButton",
            new Vector2(0f, -240f), new Vector2(460f, 100f), "MENÚ PRINCIPAL", 36);

        panelRoot.SetActive(false);

        // ---------------- REFERENCIAS DEL INSPECTOR ----------------

        GameObject livesGo = GameObject.Find("LivesManager");
        if (livesGo == null)
            return "ERR: LivesManager no encontrado en la escena";

        LivesManager lives = livesGo.GetComponent<LivesManager>();
        if (lives == null)
            return "ERR: el objeto LivesManager no tiene el componente LivesManager";

        lives.maxLives = 3;
        lives.messageDuration = 2.5f;
        lives.mainMenuScene = "MainMenu";
        lives.livesText = livesText;
        lives.messageText = messageText;
        lives.gameOverPanel = panelRoot;

        EditorUtility.SetDirty(lives);

        // Listeners persistentes de los OnClick()
        ClearListeners(restartButton);
        ClearListeners(menuButton);

        UnityEventTools.AddPersistentListener(restartButton.onClick, lives.RestartGame);
        UnityEventTools.AddPersistentListener(menuButton.onClick, lives.GoToMenu);

        EditorUtility.SetDirty(restartButton);
        EditorUtility.SetDirty(menuButton);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return "OK | HUD: lógicamente 'VIDAS: 3/3' en VictoryCanvas/LivesText | " +
               "mensajes en VictoryCanvas/MessageText | " +
               "GameOverPanel creado (GameOverTitle, GameOverMainMessage, " +
               "GameOverSecondaryMessage, GameOverRestartButton, GameOverMenuButton) | " +
               "listeners: RestartGame / GoToMenu | escena guardada";
    }

    // ------------------------------------------------------------------

    static Text MakeText(Transform parent, string name, Vector2 pos, Vector2 size,
        int fontSize, Color color, TextAnchor anchor, string content)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.alignment = anchor;
        t.color = color;
        t.text = content;
        t.raycastTarget = false;
        t.supportRichText = true;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return t;
    }

    static Image MakeImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        return img;
    }

    static Button MakeButton(Transform parent, string name, Vector2 pos,
        Vector2 size, string label, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");

        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = true;

        if (buttonSprite != null)
            img.sprite = buttonSprite;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = img;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.82f, 0.88f, 1f, 1f);
        colors.pressedColor = new Color(0.68f, 0.74f, 0.9f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.78f, 0.78f, 0.78f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        Text labelText = MakeText(go.transform, "Label",
            Vector2.zero, Vector2.zero, fontSize,
            Color.white, TextAnchor.MiddleCenter, label);

        RectTransform lrt = labelText.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.anchoredPosition = Vector2.zero;
        lrt.sizeDelta = Vector2.zero;

        return button;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    static void AddOutline(GameObject go, float distance, float alpha)
    {
        Outline outline = go.GetComponent<Outline>();
        if (outline == null)
            outline = go.AddComponent<Outline>();

        outline.effectColor = new Color(0f, 0f, 0f, alpha);
        outline.effectDistance = new Vector2(distance, -distance);
        outline.useGraphicAlpha = true;
    }

    static void ClearListeners(Button button)
    {
        while (button.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(button.onClick, 0);
    }
}
