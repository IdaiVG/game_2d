using UnityEngine;
using UnityEditor;
using System.Text;

public class ScaleBones
{
    public static object Execute()
    {
        var sb = new StringBuilder();
        string path = "Assets/Prefabs/Level/BoneCollectible.prefab";

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) return "NO_PREFAB";

        root.transform.localScale = new Vector3(1f, 1f, 1f);

        BoxCollider2D box = root.GetComponent<BoxCollider2D>();
        if (box != null) box.size = new Vector2(0.9f, 0.9f);

        SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
        sb.AppendLine("sprite=" + (sr != null && sr.sprite != null ? sr.sprite.name + " size=" + sr.sprite.bounds.size : "NULL"));
        sb.AppendLine("order=" + (sr != null ? sr.sortingOrder : -1));
        sb.AppendLine("scale=" + root.transform.localScale + " collider=" + (box != null ? box.size.ToString() : "NULL"));

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        AssetDatabase.SaveAssets();

        // Verificar que las instancias heredaron
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Bone_"))
                sb.AppendLine("  " + t.name + " worldScale=" + t.localScale);
        }

        return sb.ToString();
    }
}
