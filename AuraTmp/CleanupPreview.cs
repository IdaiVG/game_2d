using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Retira la vista previa y guarda la escena con el estado definitivo.
public static class CleanupPreview
{
    public static object Execute()
    {
        GameObject preview = GameObject.Find("ExtraLifePreview");

        if (preview != null)
            Object.DestroyImmediate(preview);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return "OK | preview retirado | escena guardada";
    }
}
