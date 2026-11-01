using UnityEditor;
using UnityEngine;

// Cambia el sprite del coleccionable de vida al huesito unico del cementerio.
public static class FixExtraLifeSprite
{
    public static object Execute()
    {
        string prefabPath = "Assets/Prefabs/Level/ExtraLifeCollectible.prefab";

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Enviroment/Cementery/Bone (1).png");

        if (sprite == null)
            return "ERR: sprite no encontrado";

        GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
        SpriteRenderer sr = contents.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            PrefabUtility.UnloadPrefabContents(contents);
            return "ERR: sin SpriteRenderer en el prefab";
        }

        sr.sprite = sprite;
        sr.color = new Color(1f, 0.55f, 0.6f, 1f);
        sr.sortingOrder = 8;

        contents.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        PrefabUtility.UnloadPrefabContents(contents);
        AssetDatabase.SaveAssets();

        // Vista previa actualizada en la escena (no se guarda)
        GameObject old = GameObject.Find("ExtraLifePreview");
        if (old != null)
            Object.DestroyImmediate(old);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "ExtraLifePreview";
        go.transform.position = new Vector3(-2f, 2.4f, 0f);

        return "OK | sprite=" + sprite.name + " | preview actualizado";
    }
}
