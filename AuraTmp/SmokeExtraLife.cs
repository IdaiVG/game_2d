using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public static class SmokeExtraLife
{
    public static object Execute()
    {
        if (!Application.isPlaying)
            return "ERR: hay que entrar en Play Mode antes de lanzar la prueba";

        GameObject go = new GameObject("SmokeExtraLifeHost");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<SmokeExtraLifeHost>();
        return "SmokeExtraLifeHost iniciado";
    }
}

public class SmokeExtraLifeHost : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Run());
    }

    static void SetLives(int value)
    {
        FieldInfo field = typeof(LivesManager).GetField(
            "<CurrentLives>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic);

        if (field != null)
            field.SetValue(null, value);
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1.0f);

        // ---------- CAJA CON EL PREFAB ACTUALIZADO ----------
        GameObject crateGo = GameObject.Find("Crate_A");
        BreakableCrate crate = crateGo != null ? crateGo.GetComponent<BreakableCrate>() : null;

        if (crate == null)
        {
            Debug.Log("[SMOKE] ERROR: Crate_A sin BreakableCrate");
            yield break;
        }

        SetLives(2);
        crate.extraLifeDropChance = 1f;

        MethodInfo breakMethod = typeof(BreakableCrate).GetMethod(
            "Break", BindingFlags.Instance | BindingFlags.NonPublic);

        breakMethod.Invoke(crate, null);
        yield return new WaitForSeconds(0.5f);

        ExtraLifeCollectible[] drops = Object.FindObjectsByType<ExtraLifeCollectible>(FindObjectsSortMode.None);
        Debug.Log("[SMOKE] caja rota | coleccionables=" + drops.Length +
                  " | huesitos=" + BoneScoreManager.TotalBones +
                  " | vidas=" + LivesManager.CurrentLives);

        // ---------- RECOGER LA VIDA ----------
        PlayerController player = PlayerController.Current;

        if (drops.Length > 0 && player != null)
        {
            Vector3 at = drops[0].transform.position;
            player.transform.position = at;

            yield return new WaitForSeconds(0.5f);

            Debug.Log("[SMOKE] recogida | vidas=" + LivesManager.CurrentLives +
                      " | HUDvidas='" + (GameObject.Find("VictoryCanvas").transform.Find("LivesText").GetComponent<Text>().text) + "'" +
                      " | coleccionables=" + Object.FindObjectsByType<ExtraLifeCollectible>(FindObjectsSortMode.None).Length);
        }

        // ---------- ATAQUE A LA ARAÑA ----------
        GameObject spiderGo = GameObject.Find("Spider");
        SpiderEnemy spider = spiderGo != null ? spiderGo.GetComponent<SpiderEnemy>() : null;

        if (spider != null && player != null)
        {
            int bonesBefore = BoneScoreManager.TotalBones;

            player.transform.position = spider.transform.position + new Vector3(-2f, 1.2f, 0f);
            yield return new WaitForSeconds(0.1f);

            bool attacked = player.TryAttack();
            yield return new WaitForSeconds(0.4f);

            Debug.Log("[SMOKE] ataque | ejecutado=" + attacked +
                      " | huesitos antes=" + bonesBefore +
                      " despues=" + BoneScoreManager.TotalBones +
                      " | vidas=" + LivesManager.CurrentLives);
        }

        Debug.Log("[SMOKE] DONE");
    }
}
