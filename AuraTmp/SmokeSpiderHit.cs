using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class SmokeSpiderHit
{
    public static object Execute()
    {
        if (!Application.isPlaying)
            return "ERR: hay que entrar en Play Mode antes de lanzar la prueba";

        GameObject go = new GameObject("SmokeSpiderHost");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<SmokeSpiderHost>();
        return "SmokeSpiderHost iniciado";
    }
}

public class SmokeSpiderHost : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Run());
    }

    static Text HudLives()
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return null;

        Transform t = canvas.transform.Find("LivesText");
        return t != null ? t.GetComponent<Text>() : null;
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1.0f);

        PlayerController player = PlayerController.Current;
        GameObject spiderGo = GameObject.Find("Spider");
        SpiderEnemy spider = spiderGo != null ? spiderGo.GetComponent<SpiderEnemy>() : null;

        if (player == null || spider == null)
        {
            Debug.Log("[SPIDER] ERROR player=" + (player != null) + " spider=" + (spider != null));
            yield break;
        }

        Debug.Log("[SPIDER] antes | vidas=" + LivesManager.CurrentLives +
                  " | HUD='" + (HudLives() != null ? HudLives().text : "?") + "'" +
                  " | jugadorMuerto=" + player.IsDead);

        // Coloca al jugador sobre la araña: el contacto real dispara OnTriggerEnter2D
        player.transform.position = spider.transform.position + new Vector3(0f, 0.2f, 0f);

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.1f);

            Debug.Log("[SPIDER] t=" + ((i + 1) * 0.1f).ToString("F2") +
                      " | vidas=" + LivesManager.CurrentLives +
                      " | HUD='" + (HudLives() != null ? HudLives().text : "?") + "'" +
                      " | jugadorMuerto=" + (player != null && player.IsDead));
        }

        yield return new WaitForSeconds(1.4f);
        Debug.Log("[SPIDER] tras reinicio | vidas=" + LivesManager.CurrentLives +
                  " | HUD='" + (HudLives() != null ? HudLives().text : "?") + "'" +
                  " | huesitos=" + BoneScoreManager.TotalBones);

        Debug.Log("[SPIDER] DONE");
    }
}
