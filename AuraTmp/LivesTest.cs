using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Arranca el arnes de pruebas del sistema de vidas (solo Play Mode).
public static class LivesTest
{
    public static object Execute()
    {
        if (!Application.isPlaying)
            return "ERR: hay que entrar en Play Mode antes de lanzar la prueba";

        GameObject go = new GameObject("LivesTestHost");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<LivesTestHost>();
        return "LivesTestHost iniciado";
    }
}

public class LivesTestHost : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Run());
    }

    // ------------------------------------------------------------------

    static Text FindText(string path)
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return null;

        Transform t = canvas.transform.Find(path);
        return t != null ? t.GetComponent<Text>() : null;
    }

    static GameObject FindPanel()
    {
        GameObject canvas = GameObject.Find("VictoryCanvas");
        if (canvas == null)
            return null;

        Transform t = canvas.transform.Find("GameOverPanel");
        return t != null ? t.gameObject : null;
    }

    static string Snapshot(string phase)
    {
        Text lives = FindText("LivesText");
        Text bones = FindText("BoneHud/Counter");
        Text msg = FindText("MessageText");
        GameObject panel = FindPanel();
        PlayerController player = PlayerController.Current;

        string message = msg != null ? msg.text.Replace("\n", " / ") : "<sin MessageText>";

        return "[LT] " + phase +
               " | scene=" + SceneManager.GetActiveScene().name +
               " | lives=" + LivesManager.CurrentLives +
               " | HUDvidas='" + (lives != null ? lives.text : "?") + "'" +
               " | HUDhuesitos='" + (bones != null ? bones.text : "?") + "'" +
               " | gameOver=" + (panel != null && panel.activeSelf) +
               " | msg='" + message + "'" +
               " | player=" + (player != null ? player.transform.position.ToString("F2") : "null");
    }

    static void Log(string phase)
    {
        Debug.Log(Snapshot(phase));
    }

    IEnumerator KillAndSample(string tag)
    {
        PlayerController player = PlayerController.Current;

        if (player == null)
        {
            Debug.Log("[LT] " + tag + " | ERROR: PlayerController.Current es null");
            yield break;
        }

        Debug.Log("[LT] " + tag + " | antes de Die() lives=" + LivesManager.CurrentLives +
                  " pos=" + player.transform.position.ToString("F2"));

        // Tres llamadas seguidas: solo la primera debe descontar vida
        player.Die();
        player.Die();
        player.Die();

        Debug.Log("[LT] " + tag + " | tras 3 llamadas a Die() lives=" + LivesManager.CurrentLives);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.15f);

            if (player == null || rb == null)
                yield break;

            Debug.Log("[LT] " + tag + " | muerte t=" + (i * 0.15f).ToString("F2") +
                      " y=" + player.transform.position.y.ToString("F2") +
                      " vy=" + rb.linearVelocity.y.ToString("F2") +
                      " sim=" + rb.simulated +
                      " bodyType=" + rb.bodyType +
                      " vidas=" + LivesManager.CurrentLives);
        }
    }

    static void SetLives(int value)
    {
        FieldInfo field = typeof(LivesManager).GetField(
            "<CurrentLives>k__BackingField",
            BindingFlags.Static | BindingFlags.NonPublic);

        if (field != null)
            field.SetValue(null, value);
    }

    static BreakableCrate FindCrate(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.GetComponent<BreakableCrate>() : null;
    }

    static void BreakCrate(BreakableCrate crate)
    {
        MethodInfo method = typeof(BreakableCrate).GetMethod(
            "Break", BindingFlags.Instance | BindingFlags.NonPublic);

        if (crate != null && method != null)
            method.Invoke(crate, null);
    }

    static int CountExtraLives()
    {
        return Object.FindObjectsByType<ExtraLifeCollectible>(FindObjectsSortMode.None).Length;
    }

    // ------------------------------------------------------------------

    IEnumerator Run()
    {
        yield return new WaitForSeconds(1.0f);
        Log("P0 arranque");

        // ---------- PRIMERA MUERTE ----------
        yield return KillAndSample("P1 muerte 1");
        yield return new WaitForSeconds(1.6f);
        Log("P1 tras reinicio automatico");

        yield return new WaitForSeconds(2.9f);
        Log("P1 mensaje tras 2.9s");

        // ---------- SEGUNDA MUERTE ----------
        yield return KillAndSample("P2 muerte 2");
        yield return new WaitForSeconds(1.6f);
        Log("P2 tras reinicio automatico");

        yield return new WaitForSeconds(2.9f);
        Log("P2 mensaje tras 2.9s");

        // ---------- TERCERA MUERTE ----------
        yield return KillAndSample("P3 muerte 3");
        yield return new WaitForSeconds(1.6f);
        Log("P3 GAME OVER");

        yield return new WaitForSeconds(2.5f);
        Log("P3 +2.5s sin recargar");

        // ---------- REINTENTAR ----------
        if (LivesManager.Instance != null)
        {
            Debug.Log("[LT] P4 | pulsando REINTENTAR (LivesManager.RestartGame)");
            LivesManager.Instance.RestartGame();
        }

        yield return new WaitForSeconds(1.6f);
        Log("P4 tras REINTENTAR");

        // ---------- VIDA EXTRA CON 3 VIDAS ----------
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Level/ExtraLifeCollectible.prefab");

        if (prefab == null)
        {
            Debug.Log("[LT] P5 | ERROR: prefab de vida extra no encontrado");
        }
        else
        {
            PlayerController player = PlayerController.Current;
            Vector3 at = player != null
                ? player.transform.position + Vector3.up * 0.1f
                : Vector3.zero;

            GameObject first = (GameObject)Instantiate(prefab, at, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            Log("P5 vida extra con 3 vidas");
            Debug.Log("[LT] P5 | coleccionables vivos=" + CountExtraLives() +
                      " (debe seguir existiendo, no se consume)");

            // ---------- VIDA EXTRA CON 2 VIDAS ----------
            SetLives(2);
            Debug.Log("[LT] P6 | vidas forzadas a " + LivesManager.CurrentLives);

            GameObject second = (GameObject)Instantiate(prefab, at, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            Log("P6 vida extra con 2 vidas");
            Debug.Log("[LT] P6 | coleccionables vivos=" + CountExtraLives() +
                      " | segundo coleccionable destruido=" + (second == null));

            if (first != null)
                Destroy(first);
        }

        // ---------- CAJA AL 100% ----------
        BreakableCrate crateA = FindCrate("Crate_A");
        if (crateA != null)
        {
            crateA.extraLifeDropChance = 1f;
            int bonesBefore = BoneScoreManager.TotalBones;

            BreakCrate(crateA);
            yield return new WaitForSeconds(0.5f);

            Log("P7 caja probabilidad 100%");
            Debug.Log("[LT] P7 | huesitos antes=" + bonesBefore +
                      " despues=" + BoneScoreManager.TotalBones +
                      " | coleccionables vivos=" + CountExtraLives());
        }
        else
        {
            Debug.Log("[LT] P7 | ERROR: Crate_A no encontrada");
        }

        // ---------- CAJA AL 0% ----------
        BreakableCrate crateB = FindCrate("Crate_B");
        if (crateB != null)
        {
            crateB.extraLifeDropChance = 0f;
            int bonesBefore = BoneScoreManager.TotalBones;

            BreakCrate(crateB);
            yield return new WaitForSeconds(0.5f);

            Log("P8 caja probabilidad 0%");
            Debug.Log("[LT] P8 | huesitos antes=" + bonesBefore +
                      " despues=" + BoneScoreManager.TotalBones +
                      " | coleccionables vivos=" + CountExtraLives());
        }
        else
        {
            Debug.Log("[LT] P8 | ERROR: Crate_B no encontrada");
        }

        // ---------- Estado final ----------
        BreakableCrate[] all = Object.FindObjectsByType<BreakableCrate>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
            all[i].extraLifeDropChance = 0.2f;

        ExtraLifeCollectible[] drops = Object.FindObjectsByType<ExtraLifeCollectible>(FindObjectsSortMode.None);
        for (int i = 0; i < drops.Length; i++)
            Destroy(drops[i].gameObject);

        Log("P9 estado final");

        PlayerController p = PlayerController.Current;
        if (p != null)
        {
            p.TryJump();
            bool j1 = p.TryJump();
            bool j2 = p.TryJump();
            Debug.Log("[LT] P10 | saltos: primero=true segundo=" + j1 + " tercero=" + j2);
        }

        Debug.Log("[LT] DONE");
    }
}
