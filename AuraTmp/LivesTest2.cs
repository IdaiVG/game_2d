using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LivesTest2
{
    public static object Execute()
    {
        if (!Application.isPlaying)
            return "ERR: hay que entrar en Play Mode antes de lanzar la prueba";

        GameObject go = new GameObject("LivesTest2Host");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<LivesTest2Host>();
        return "LivesTest2Host iniciado";
    }
}

public class LivesTest2Host : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Run());
    }

    static Text FindText(string path)
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return null;

        Transform t = canvas.transform.Find(path);
        return t != null ? t.GetComponent<Text>() : null;
    }

    static void SetLives(int value)
    {
        FieldInfo field = typeof(LivesManager).GetField(
            "<CurrentLives>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic);

        if (field != null)
            field.SetValue(null, value);
    }

    static string Snapshot(string phase)
    {
        Text lives = FindText("LivesText");
        Text bones = FindText("BoneHud/Counter");
        Text msg = FindText("MessageText");

        return "[LT2] " + phase +
               " | scene=" + SceneManager.GetActiveScene().name +
               " | lives=" + LivesManager.CurrentLives +
               " | HUDvidas='" + (lives != null ? lives.text : "-") + "'" +
               " | HUDhuesitos='" + (bones != null ? bones.text : "-") + "'" +
               " | huesitos=" + BoneScoreManager.TotalBones +
               " | msg='" + (msg != null ? msg.text.Replace("\n", " / ") : "-") + "'";
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1.0f);

        BoneScoreManager.AddBones(10);
        SetLives(2);
        Debug.Log(Snapshot("A antes de MENU PRINCIPAL (huesitos 10, vidas 2)"));

        // ---------- MENU PRINCIPAL ----------
        if (LivesManager.Instance != null)
            LivesManager.Instance.GoToMenu();

        yield return new WaitForSeconds(1.8f);
        Debug.Log(Snapshot("B en MainMenu"));

        // ---------- NUEVA PARTIDA ----------
        SceneManager.LoadScene("Level_01");
        yield return new WaitForSeconds(1.8f);
        Debug.Log(Snapshot("C nueva partida Level_01"));

        // ---------- MUERTE EN EL AIRE ----------
        PlayerController player = PlayerController.Current;

        if (player == null)
        {
            Debug.Log("[LT2] D | ERROR: PlayerController.Current es null");
        }
        else
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            player.transform.position = player.transform.position + Vector3.up * 6f;
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(0.1f);

            float startY = player.transform.position.y;
            player.Die();

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(0.15f);

                if (player == null || rb == null)
                    break;

                Debug.Log("[LT2] D muerte en el aire t=" + (i * 0.15f).ToString("F2") +
                          " y=" + player.transform.position.y.ToString("F2") +
                          " vy=" + rb.linearVelocity.y.ToString("F2") +
                          " sim=" + rb.simulated +
                          " vidas=" + LivesManager.CurrentLives);
            }

            Debug.Log("[LT2] D | altura inicial=" + startY.ToString("F2") + " vidas=" + LivesManager.CurrentLives);
        }

        yield return new WaitForSeconds(1.8f);
        Debug.Log(Snapshot("E tras reinicio por muerte en el aire"));

        Debug.Log("[LT2] DONE");
    }
}
