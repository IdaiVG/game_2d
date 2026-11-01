using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Crea el prefab del coleccionable de vida reutilizando el arte del cementerio
// y lo enlaza en las tres cajas rompibles existentes.
public static class BuildExtraLifePrefab
{
    public static object Execute()
    {
        Scene scene = SceneManager.GetActiveScene();

        string prefabFolder = "Assets/Prefabs/Level";
        string prefabPath = prefabFolder + "/ExtraLifeCollectible.prefab";

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Enviroment/Cementery/Bone (4).png");

        if (sprite == null)
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Art/Enviroment/Cementery/Bone (1).png");

        if (sprite == null)
            return "ERR: no se pudo cargar el sprite del huesito";

        // --- Plantilla temporal en la escena ---
        GameObject temp = new GameObject("ExtraLifeTemplate");
        temp.transform.position = Vector3.zero;

        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.55f, 0.6f, 1f);
        sr.sortingOrder = 8;

        temp.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        CircleCollider2D col = temp.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;

        ExtraLifeCollectible life = temp.AddComponent<ExtraLifeCollectible>();
        life.livesToGrant = 1;
        life.floatAnimation = true;
        life.floatAmplitude = 0.15f;
        life.floatSpeed = 2.2f;
        life.spinSpeed = 0f;
        life.lifetime = 0f;

        // --- Prefab ---
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
        Object.DestroyImmediate(temp);

        if (prefab == null)
            return "ERR: no se pudo crear el prefab";

        // --- Enlazar en las tres cajas ---
        string[] crates = { "Crate_A", "Crate_B", "Crate_C" };
        int linked = 0;

        for (int i = 0; i < crates.Length; i++)
        {
            GameObject crateGo = GameObject.Find(crates[i]);
            if (crateGo == null)
                continue;

            BreakableCrate crate = crateGo.GetComponent<BreakableCrate>();
            if (crate == null)
                continue;

            crate.extraLifePrefab = prefab;
            crate.extraLifeDropChance = 0.2f;
            crate.extraLifeDropHeight = 0.9f;
            linked++;

            EditorUtility.SetDirty(crate);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        return "OK | prefab: " + prefabPath +
               " | sprite: " + AssetDatabase.GetAssetPath(sprite) +
               " | cajas enlazadas: " + linked +
               " | probabilidad: 0.2 | escena guardada";
    }
}
