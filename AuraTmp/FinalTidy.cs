using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Text;

public class FinalTidy
{
    public static object Execute()
    {
        var sb = new StringBuilder();

        // Alejar el huesito mas cercano al punto de aparicion
        GameObject bone02 = GameObject.Find("Bone_02");
        if (bone02 != null)
        {
            bone02.transform.position = new Vector3(-0.5f, 0.85f, 0f);
            sb.AppendLine("Bone_02 -> " + bone02.transform.position);
        }

        // Inventario final
        int bones = 0, crates = 0;
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.GetComponent<BoneCollectible>() != null) bones++;
            if (t.GetComponent<BreakableCrate>() != null) crates++;
        }
        sb.AppendLine("bones=" + bones + " crates=" + crates);

        BoneScoreManager mgr = Object.FindFirstObjectByType<BoneScoreManager>();
        sb.AppendLine("manager=" + (mgr != null ? "OK" : "NULL"));
        if (mgr != null)
            sb.AppendLine("  rewards: bone=" + mgr.boneCollectibleReward + " crate=" + mgr.crateReward
                + " spider=" + mgr.spiderReward + " level=" + mgr.levelCompleteReward
                + " resetRestart=" + mgr.resetOnLevelRestart + " resetNewGame=" + mgr.resetOnNewGame);

        LevelGoal goal = Object.FindFirstObjectByType<LevelGoal>();
        sb.AppendLine("goal=" + (goal != null ? "OK bonus=" + goal.boneReward + " label=" + (goal.bonesResultLabel != null ? goal.bonesResultLabel.name : "NULL") : "NULL"));

        BoneHudUI hud = Object.FindFirstObjectByType<BoneHudUI>();
        sb.AppendLine("hud=" + (hud != null ? "OK counter=" + (hud.counterLabel != null ? hud.counterLabel.text : "NULL") : "NULL"));

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        sb.AppendLine("saved=" + EditorSceneManager.SaveScene(SceneManager.GetActiveScene()));

        return sb.ToString();
    }
}
