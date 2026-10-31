using System.Collections;
using UnityEngine;

// Huesito coleccionable. Solo el jugador puede recogerlo y entrega +1 una vez.
[RequireComponent(typeof(Collider2D))]
public class BoneCollectible : MonoBehaviour
{
    [Header("Recompensa")]
    public int boneReward = 1;

    [Tooltip("Identificador unico y estable. Evita otorgar la recompensa dos veces.")]
    public string rewardId = "";

    [Header("Animacion")]
    public bool floatAnimation = true;
    public float floatAmplitude = 0.12f;
    public float floatSpeed = 2f;
    public float spinSpeed = 0f;

    private Collider2D boneCollider;
    private SpriteRenderer boneRenderer;
    private Vector3 basePosition;
    private float phase;
    private bool collected;

    void Awake()
    {
        boneCollider = GetComponent<Collider2D>();
        boneRenderer = GetComponent<SpriteRenderer>();

        if (boneCollider != null)
            boneCollider.isTrigger = true;

        basePosition = transform.position;

        // Fase aleatoria para que los huesitos no floten todos al unisono
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (collected || !floatAnimation)
            return;

        float offset = Mathf.Sin(Time.time * floatSpeed + phase) * floatAmplitude;

        transform.position = basePosition + new Vector3(0f, offset, 0f);

        if (Mathf.Abs(spinSpeed) > 0.01f)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        // Solo el jugador puede recoger huesitos
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null || player.IsDead)
            return;

        Collect();
    }

    void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (boneCollider != null)
            boneCollider.enabled = false;

        string key = string.IsNullOrEmpty(rewardId)
            ? "bone:" + name + ":" + Mathf.RoundToInt(basePosition.x * 10f)
            : "bone:" + rewardId;

        bool awarded = BoneScoreManager.AddBonesOnce(key, boneReward);

        if (awarded)
        {
            BoneHudUI.ShowPopup(
                transform.position + Vector3.up * 0.45f,
                "+" + boneReward);
        }

        BoneFeedback.PlayCollect(
            boneRenderer != null ? boneRenderer.sprite : null,
            transform.position,
            boneRenderer != null ? boneRenderer.sortingOrder : 7);

        StartCoroutine(CollectSequence());
    }

    IEnumerator CollectSequence()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Color startColor = boneRenderer != null ? boneRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(startScale, startScale * 1.4f, t);

            if (boneRenderer != null)
            {
                Color c = startColor;
                c.a = startColor.a * (1f - t);
                boneRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
