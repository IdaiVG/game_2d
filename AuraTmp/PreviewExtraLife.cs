using UnityEditor;
using UnityEngine;

// Coloca una vista previa del coleccionable de vida (sin guardar la escena).
public static class PreviewExtraLife
{
    public static object Execute()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Level/ExtraLifeCollectible.prefab");

        if (prefab == null)
            return "ERR: prefab no encontrado";

        GameObject old = GameObject.Find("ExtraLifePreview");
        if (old != null)
            Object.DestroyImmediate(old);

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "ExtraLifePreview";
        go.transform.position = new Vector3(-2f, 2.4f, 0f);

        return "preview creado en " + go.transform.position;
    }
}
