using UnityEngine;

// Pequeño estallido de fragmentos construido con el sprite que ya existe del
// propio objeto (caja, huesito o araña). No crea recursos nuevos.
public class BoneFxBurst : MonoBehaviour
{
    private SpriteRenderer[] fragments;
    private Vector3[] velocities;
    private float[] spins;
    private Color baseColor;
    private float life;
    private float elapsed;
    private float gravity = 16f;

    public static BoneFxBurst Spawn(
        Sprite sprite,
        Vector3 position,
        Color color,
        int count,
        float speed,
        float size,
        float lifetime,
        int sortingOrder)
    {
        if (sprite == null || count <= 0)
            return null;

        GameObject root = new GameObject("BoneFx");

        root.transform.position = position;

        BoneFxBurst burst = root.AddComponent<BoneFxBurst>();

        burst.Init(sprite, color, count, speed, size, lifetime, sortingOrder);

        return burst;
    }

    private void Init(
        Sprite sprite,
        Color color,
        int count,
        float speed,
        float size,
        float lifetime,
        int sortingOrder)
    {
        baseColor = color;
        life = Mathf.Max(lifetime, 0.05f);

        fragments = new SpriteRenderer[count];
        velocities = new Vector3[count];
        spins = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject frag = new GameObject("Fragment" + i);

            frag.transform.SetParent(transform, false);

            frag.transform.localPosition =
                (Vector3)(Random.insideUnitCircle * 0.1f);

            float scale = size * Random.Range(0.16f, 0.32f);

            frag.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = frag.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            float angle = (360f / count) * i + Random.Range(-20f, 20f);

            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;

            velocities[i] = direction * speed * Random.Range(0.55f, 1.2f);
            velocities[i] += Vector3.up * speed * Random.Range(0.2f, 0.55f);

            spins[i] = Random.Range(-320f, 320f);

            fragments[i] = sr;
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / life);
        float alpha = baseColor.a * (1f - t);

        for (int i = 0; i < fragments.Length; i++)
        {
            SpriteRenderer sr = fragments[i];

            if (sr == null)
                continue;

            velocities[i] += Vector3.down * gravity * Time.deltaTime;

            sr.transform.position += velocities[i] * Time.deltaTime;
            sr.transform.Rotate(0f, 0f, spins[i] * Time.deltaTime);

            Color c = baseColor;
            c.a = alpha;
            sr.color = c;
        }

        if (elapsed >= life)
            Destroy(gameObject);
    }
}
