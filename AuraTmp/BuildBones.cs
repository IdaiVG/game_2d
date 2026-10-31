using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;
using System.Collections.Generic;

public class BuildBones
{
    static StringBuilder log = new StringBuilder();

    public static object Execute()
    {
        Scene scene = SceneManager.GetActiveScene();

        GameObject canvasGO = GameObject.Find("VictoryCanvas");
        if (canvasGO == null) return "NO_CANVAS";

        // ---------------- Referencias de estilo ----------------
        GameObject titleGO = canvasGO.transform.Find("VictoryOverlay/Panel/Title").gameObject;
        Text titleText = titleGO.GetComponent<Text>();
        Font font = titleText != null ? titleText.font : null;
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        log.AppendLine("font=" + (font != null ? font.name : "NULL"));

        GameObject panelGO = canvasGO.transform.Find("VictoryOverlay/Panel").gameObject;
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();

        // ---------------- 1. HUD de huesitos ----------------
        Transform existingHud = canvasGO.transform.Find("BoneHud");
        if (existingHud != null) Object.DestroyImmediate(existingHud.gameObject);

        GameObject hudGO = new GameObject("BoneHud", typeof(RectTransform));
        RectTransform hudRT = hudGO.GetComponent<RectTransform>();
        hudRT.SetParent(canvasGO.transform, false);
        hudRT.anchorMin = new Vector2(0f, 1f);
        hudRT.anchorMax = new Vector2(0f, 1f);
        hudRT.pivot = new Vector2(0f, 1f);
        hudRT.anchoredPosition = new Vector2(26f, -22f);
        hudRT.sizeDelta = new Vector2(400f, 78f);
        hudRT.SetSiblingIndex(0);

        // Fondo
        GameObject bgGO = new GameObject("Bg", typeof(RectTransform));
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.SetParent(hudRT, false);
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        Sprite uiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (uiSprite != null) { bgImg.sprite = uiSprite; bgImg.type = Image.Type.Sliced; }
        bgImg.color = new Color(0.05f, 0.05f, 0.08f, 0.78f);
        bgImg.raycastTarget = false;

        // Icono de huesito
        Sprite boneIconSprite = LoadSprite("Assets/Art/Enviroment/Cementery/Bone (1).png", null);
        GameObject iconGO = new GameObject("BoneIcon", typeof(RectTransform));
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.SetParent(hudRT, false);
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.pivot = new Vector2(0f, 0.5f);
        iconRT.anchoredPosition = new Vector2(12f, 0f);
        iconRT.sizeDelta = new Vector2(54f, 54f);
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = boneIconSprite;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;
        log.AppendLine("boneIcon=" + (boneIconSprite != null ? boneIconSprite.name : "NULL"));

        // Contador
        GameObject counterGO = new GameObject("Counter", typeof(RectTransform));
        RectTransform counterRT = counterGO.GetComponent<RectTransform>();
        counterRT.SetParent(hudRT, false);
        counterRT.anchorMin = new Vector2(0f, 0.5f);
        counterRT.anchorMax = new Vector2(0f, 0.5f);
        counterRT.pivot = new Vector2(0f, 0.5f);
        counterRT.anchoredPosition = new Vector2(76f, 0f);
        counterRT.sizeDelta = new Vector2(310f, 60f);

        Text counter = counterGO.AddComponent<Text>();
        counter.font = font;
        counter.fontSize = 36;
        counter.fontStyle = FontStyle.Bold;
        counter.alignment = TextAnchor.MiddleLeft;
        counter.color = new Color(0.85f, 0.93f, 1f, 1f);
        counter.raycastTarget = false;
        counter.horizontalOverflow = HorizontalWrapMode.Overflow;
        counter.verticalOverflow = VerticalWrapMode.Overflow;
        counter.text = "HUESITOS: 000";

        Outline counterOutline = counterGO.AddComponent<Outline>();
        counterOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        counterOutline.effectDistance = new Vector2(2f, -2f);

        BoneHudUI hud = hudGO.AddComponent<BoneHudUI>();
        hud.counterLabel = counter;
        hud.pulseTarget = counterRT;
        hud.font = font;
        hud.popupParent = canvasGO.GetComponent<RectTransform>();
        hud.prefix = "HUESITOS: ";
        hud.numberFormat = "000";

        // ---------------- 2. Texto de huesitos en la victoria ----------------
        Transform oldResult = panelRT.Find("BonesResult");
        if (oldResult != null) Object.DestroyImmediate(oldResult.gameObject);

        // Coloca el texto justo encima de los botones
        float buttonTop = float.MaxValue;
        foreach (Transform child in panelRT)
        {
            if (!child.name.Contains("Button")) continue;
            RectTransform brt = child as RectTransform;
            if (brt == null) continue;
            float top = brt.anchoredPosition.y + brt.sizeDelta.y * 0.5f;
            if (top < buttonTop) buttonTop = top;
        }
        if (buttonTop == float.MaxValue) buttonTop = -20f;
        log.AppendLine("buttonTop=" + buttonTop);

        GameObject resultGO = new GameObject("BonesResult", typeof(RectTransform));
        RectTransform resultRT = resultGO.GetComponent<RectTransform>();
        resultRT.SetParent(panelRT, false);
        resultRT.anchorMin = new Vector2(0.5f, 0.5f);
        resultRT.anchorMax = new Vector2(0.5f, 0.5f);
        resultRT.pivot = new Vector2(0.5f, 0.5f);
        resultRT.sizeDelta = new Vector2(1000f, 110f);
        resultRT.anchoredPosition = new Vector2(0f, buttonTop + 78f);

        Text result = resultGO.AddComponent<Text>();
        result.font = font;
        result.fontSize = 38;
        result.fontStyle = FontStyle.Bold;
        result.alignment = TextAnchor.MiddleCenter;
        result.color = new Color(0.85f, 0.93f, 1f, 1f);
        result.raycastTarget = false;
        result.horizontalOverflow = HorizontalWrapMode.Overflow;
        result.verticalOverflow = VerticalWrapMode.Overflow;
        result.text = "HUESITOS CONSEGUIDOS: 0\nPUNTUACIÓN TOTAL: 0";

        Outline resultOutline = resultGO.AddComponent<Outline>();
        resultOutline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        resultOutline.effectDistance = new Vector2(2f, -2f);

        resultRT.SetSiblingIndex(2);

        // ---------------- 3. Administrador de puntuacion ----------------
        GameObject mgrGO = GameObject.Find("BoneScoreManager");
        if (mgrGO == null) mgrGO = new GameObject("BoneScoreManager");
        BoneScoreManager mgr = mgrGO.GetComponent<BoneScoreManager>();
        if (mgr == null) mgr = mgrGO.AddComponent<BoneScoreManager>();
        mgr.boneCollectibleReward = 1;
        mgr.crateReward = 5;
        mgr.spiderReward = 10;
        mgr.levelCompleteReward = 25;
        mgr.resetOnLevelRestart = false;
        mgr.resetOnNewGame = true;

        // ---------------- 4. Prefabs ----------------
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Level"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Level");

        log.AppendLine("folders_ok");

        Sprite crateSprite = LoadSprite("Assets/Art/Obstacles/Crate.png", null);
        log.AppendLine("crateSprite=" + (crateSprite != null ? crateSprite.name + " " + crateSprite.bounds.size : "NULL"));

        // --- Caja ---
        GameObject crateTemplate = BuildCrateObject("Crate_Template", crateSprite);
        GameObject cratePrefab = PrefabUtility.SaveAsPrefabAsset(crateTemplate, "Assets/Prefabs/Level/BreakableCrate.prefab");
        Object.DestroyImmediate(crateTemplate);
        log.AppendLine("cratePrefab=" + (cratePrefab != null ? "OK" : "FAIL"));

        // --- Huesito ---
        Sprite boneSprite = LoadSprite("Assets/Art/Enviroment/Cementery/Bone (1).png", null);
        GameObject boneTemplate = BuildBoneObject("Bone_Template", boneSprite);
        GameObject bonePrefab = PrefabUtility.SaveAsPrefabAsset(boneTemplate, "Assets/Prefabs/Level/BoneCollectible.prefab");
        Object.DestroyImmediate(boneTemplate);
        log.AppendLine("bonePrefab=" + (bonePrefab != null ? "OK" : "FAIL"));

        // ---------------- 5. Colocacion en la escena ----------------
        ClearOld("Crate_A"); ClearOld("Crate_B"); ClearOld("Crate_C");
        for (int i = 1; i <= 10; i++) ClearOld("Bone_" + i.ToString("00"));

        Vector3[] cratePositions = {
            new Vector3(16.8f, 2.795f, 0f),
            new Vector3(26.8f, 1.795f, 0f),
            new Vector3(42.8f, 3.795f, 0f)
        };
        string[] crateIds = { "crate_A", "crate_B", "crate_C" };

        for (int i = 0; i < cratePositions.Length; i++)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab, scene);
            inst.name = "Crate_" + (char)('A' + i);
            inst.transform.position = cratePositions[i];

            BreakableCrate bc = inst.GetComponent<BreakableCrate>();
            if (bc != null) bc.rewardId = crateIds[i];
        }

        Vector3[] bonePositions = {
            new Vector3(-6.5f, 0.85f, 0f),
            new Vector3(-1.0f, 0.85f, 0f),
            new Vector3(8.0f, 1.85f, 0f),
            new Vector3(15.5f, 2.85f, 0f),
            new Vector3(18.0f, 2.85f, 0f),
            new Vector3(24.5f, 1.85f, 0f),
            new Vector3(35.5f, 2.85f, 0f),
            new Vector3(46.0f, 4.75f, 0f),
            new Vector3(53.5f, 4.85f, 0f),
            new Vector3(55.5f, 6.60f, 0f)
        };

        Sprite[] boneSprites = {
            LoadSprite("Assets/Art/Enviroment/Cementery/Bone (1).png", null),
            LoadSprite("Assets/Art/Enviroment/Cementery/Bone (2).png", null),
            LoadSprite("Assets/Art/Enviroment/Cementery/Bone (3).png", null),
            LoadSprite("Assets/Art/Enviroment/Cementery/Bone (4).png", null)
        };

        for (int i = 0; i < bonePositions.Length; i++)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(bonePrefab, scene);
            inst.name = "Bone_" + (i + 1).ToString("00");
            inst.transform.position = bonePositions[i];

            BoneCollectible bc = inst.GetComponent<BoneCollectible>();
            if (bc != null) bc.rewardId = "bone_" + (i + 1).ToString("00");

            SpriteRenderer sr = inst.GetComponent<SpriteRenderer>();
            if (sr != null && boneSprites[i % 4] != null) sr.sprite = boneSprites[i % 4];
        }

        // ---------------- 6. Wiring de la meta ----------------
        GameObject goalGO = GameObject.Find("LevelGoal");
        LevelGoal goal = goalGO != null ? goalGO.GetComponent<LevelGoal>() : null;
        if (goal != null)
        {
            goal.bonesResultLabel = result;
            goal.boneReward = 25;
            EditorUtility.SetDirty(goal);
        }
        log.AppendLine("goal=" + (goal != null ? "OK" : "NULL"));

        // ---------------- 7. Ajustes de arañas ----------------
        int spiderCount = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!t.name.StartsWith("Spider")) continue;
            SpiderEnemy se = t.GetComponent<SpiderEnemy>();
            if (se == null) continue;
            se.boneReward = 10;
            se.rewardId = "spider_" + (spiderCount + 1);
            spiderCount++;
            EditorUtility.SetDirty(se);
        }
        log.AppendLine("spiders=" + spiderCount);

        EditorUtility.SetDirty(hud);
        EditorUtility.SetDirty(mgr);
        EditorSceneManager.MarkSceneDirty(scene);

        AssetDatabase.SaveAssets();

        // Validacion de posiciones sobre plataforma
        log.AppendLine("--- placement check ---");
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Bone_") || t.name.StartsWith("Crate_"))
                log.AppendLine("  " + t.name + " @ " + t.position);
        }

        return log.ToString();
    }

    static void ClearOld(string name)
    {
        GameObject old = GameObject.Find(name);
        if (old != null) Object.DestroyImmediate(old);
    }

    static GameObject BuildCrateObject(string name, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        go.layer = 0;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 3;
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = sprite != null ? new Vector2(sprite.bounds.size.x, sprite.bounds.size.y) : Vector2.one;
        box.isTrigger = false;

        BreakableCrate bc = go.AddComponent<BreakableCrate>();
        bc.boneReward = 5;
        bc.breakOnStomp = true;
        bc.stompTolerance = 0.35f;
        bc.stompMinFallSpeed = 1f;
        bc.destroyDelay = 0.32f;

        return go;
    }

    static GameObject BuildBoneObject(string name, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        go.layer = 0;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 7;
        go.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(1.2f, 1.2f);
        box.isTrigger = true;

        BoneCollectible bc = go.AddComponent<BoneCollectible>();
        bc.boneReward = 1;
        bc.floatAnimation = true;
        bc.floatAmplitude = 0.12f;
        bc.floatSpeed = 2f;

        return go;
    }

    static Sprite LoadSprite(string path, string spriteName)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object o in all)
        {
            Sprite s = o as Sprite;
            if (s == null) continue;
            if (spriteName == null || s.name == spriteName) return s;
        }
        return null;
    }
}
