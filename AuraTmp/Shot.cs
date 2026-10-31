using UnityEngine;
using UnityEditor;
using System.IO;

public class Shot
{
    public static object Execute()
    {
        Camera cam = Camera.main;
        if (cam == null) return "NO_CAM";

        Transform bg = cam.transform.Find("Background");

        Vector3 oldPos = cam.transform.position;
        Quaternion oldRot = cam.transform.rotation;
        float oldSize = cam.orthographicSize;
        bool bgActive = bg != null && bg.gameObject.activeSelf;

        int w = 1280, h = 720;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        string dir = "C:/Users/Idai/AppData/Local/AuraForUnity/CapturedImages/boneshots";
        Directory.CreateDirectory(dir);

        float[] xs = { 16f, 53f };
        string[] names = { "area_crateA", "area_P7" };

        for (int i = 0; i < xs.Length; i++)
        {
            cam.transform.position = new Vector3(xs[i], 3.2f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.orthographicSize = 4.5f;

            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes(dir + "/" + names[i] + ".png", tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        RenderTexture.ReleaseTemporary(rt);

        cam.transform.position = oldPos;
        cam.transform.rotation = oldRot;
        cam.orthographicSize = oldSize;
        if (bg != null) bg.gameObject.SetActive(bgActive);

        return "ok " + dir;
    }
}
