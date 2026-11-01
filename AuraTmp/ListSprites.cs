using System.Text;
using UnityEditor;
using UnityEngine;

// Lista los sub-sprites de las imagenes candidatas para la vida extra.
public static class ListSprites
{
    public static object Execute()
    {
        string[] paths =
        {
            "Assets/Art/Enviroment/Cementery/Bone (1).png",
            "Assets/Art/Enviroment/Cementery/Bone (2).png",
            "Assets/Art/Enviroment/Cementery/Bone (3).png",
            "Assets/Art/Enviroment/Cementery/Bone (4).png",
            "Assets/Art/Obstacles/Skeleton.png"
        };

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < paths.Length; i++)
        {
            Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(paths[i]);
            int sprites = 0;
            StringBuilder names = new StringBuilder();

            for (int j = 0; j < assets.Length; j++)
            {
                Sprite s = assets[j] as Sprite;
                if (s == null)
                    continue;

                sprites++;
                if (sprites <= 6)
                    names.Append(s.name).Append(",");
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(paths[i]);

            sb.Append(paths[i])
              .Append(" => sprites=").Append(sprites)
              .Append(" tex=").Append(tex != null ? tex.width + "x" + tex.height : "?")
              .Append(" [").Append(names.ToString()).Append("]\n");
        }

        return sb.ToString();
    }
}
