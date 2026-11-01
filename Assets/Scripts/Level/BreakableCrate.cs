using System.Collections;
using UnityEngine;

// Caja de madera rompible al caer encima de ella.
// Solo se rompe con un pisoton real: el jugador debe venir descendiendo, sus
// pies deben quedar sobre la tapa y el contacto debe ser vertical.
[RequireComponent(typeof(Collider2D))]
public class BreakableCrate : MonoBehaviour
{
    [Header("Recompensa")]
    public int boneReward = 5;

    [Tooltip("Identificador unico y estable. Evita otorgar la recompensa dos veces.")]
    public string rewardId = "";

    [Header("Vida extra")]
    [Tooltip("Probabilidad (0-1) de que la caja suelte una vida extra al romperse.")]
    [Range(0f, 1f)] public float extraLifeDropChance = 0.2f;

    [Tooltip("Prefab del coleccionable de vida. Sin prefab no hay tirada.")]
    public GameObject extraLifePrefab;

    [Tooltip("Altura sobre la superficie solida donde aparece la vida.")]
    public float extraLifeDropHeight = 0.9f;

    [Header("Rotura")]
    public bool breakOnStomp = true;
    public float stompTolerance = 0.35f;
    public float stompMinFallSpeed = 1f;
    public float destroyDelay = 0.32f;

    [Header("Efectos")]
    public GameObject breakEffect;
    public AudioClip breakSound;

    private Collider2D crateCollider;
    private SpriteRenderer crateRenderer;
    private bool broken;
    private bool dropRolled;

    private static readonly ContactPoint2D[] contacts = new ContactPoint2D[8];

    void Awake()
    {
        crateCollider = GetComponent<Collider2D>();
        crateRenderer = GetComponent<SpriteRenderer>();

        // La caja nunca debe recargar el doble salto del jugador
        if (crateCollider != null)
            crateCollider.isTrigger = false;

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken || !breakOnStomp)
            return;

        PlayerController player = collision.collider.GetComponentInParent<PlayerController>();

        if (player == null || player.IsDead)
            return;

        if (!IsStompFromAbove(collision))
            return;

        Break();
    }

    // Combina velocidad vertical, geometria y normal de contacto para descartar
    // los golpes laterales y los que llegan desde abajo.
    bool IsStompFromAbove(Collision2D collision)
    {
        if (collision.collider == null || crateCollider == null)
            return false;

        // 1) Debe existir una velocidad vertical real en el impacto
        float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);

        if (impactSpeed < stompMinFallSpeed)
            return false;

        // 2) El jugador no puede estar subiendo en el momento del choque
        Rigidbody2D playerBody = collision.collider.attachedRigidbody;

        if (playerBody != null && playerBody.linearVelocity.y > 1f)
            return false;

        Bounds playerBounds = collision.collider.bounds;
        Bounds crateBounds = crateCollider.bounds;

        // 3) Los pies del jugador sobre la tapa de la caja
        bool feetOnTop = playerBounds.min.y >= crateBounds.max.y - stompTolerance;

        // 4) El centro del jugador por encima del centro de la caja
        bool centerAbove = playerBounds.center.y > crateBounds.center.y;

        // 5) La normal del contacto debe ser predominantemente vertical
        int contactCount = collision.GetContacts(contacts);
        bool topContact = false;

        for (int i = 0; i < contactCount; i++)
        {
            Vector2 n = contacts[i].normal;

            if (Mathf.Abs(n.y) > Mathf.Abs(n.x))
            {
                topContact = true;
                break;
            }
        }

        return feetOnTop && centerAbove && topContact;
    }

    void Break()
    {
        if (broken)
            return;

        broken = true;

        // Deja de ser solida de inmediato: el jugador sigue su camino
        if (crateCollider != null)
            crateCollider.enabled = false;

        string key = string.IsNullOrEmpty(rewardId)
            ? "crate:" + name + ":" + Mathf.RoundToInt(transform.position.x * 10f)
            : "crate:" + rewardId;

        bool awarded = BoneScoreManager.AddBonesOnce(key, boneReward);

        if (awarded)
        {
            BoneHudUI.ShowPopup(
                transform.position + Vector3.up * 0.6f,
                "+" + boneReward);
        }

        BoneFeedback.PlayCrateBreak(
            crateRenderer != null ? crateRenderer.sprite : null,
            transform.position,
            crateRenderer != null ? crateRenderer.sortingOrder : 3);

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        if (breakSound != null)
            AudioSource.PlayClipAtPoint(breakSound, transform.position);

        // Una sola tirada por caja, despues de entregar los cinco huesitos
        TryDropExtraLife();

        StartCoroutine(BreakSequence());
    }

    // Calcula la probabilidad una unica vez. Si sale premiada, coloca el
    // coleccionable sobre la superficie solida mas cercana bajo la caja.
    void TryDropExtraLife()
    {
        if (dropRolled || extraLifePrefab == null)
            return;

        dropRolled = true;

        if (Random.value > extraLifeDropChance)
            return;

        Vector3 spawnPosition = FindDropPosition();

        GameObject drop = Instantiate(extraLifePrefab, spawnPosition, Quaternion.identity);
        drop.name = "ExtraLife";

        BoneHudUI.ShowPopup(
            spawnPosition,
            "¡VIDA EXTRA!",
            new Color(1f, 0.55f, 0.6f, 1f));
    }

    Vector3 FindDropPosition()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 6f);

        float bestY = float.NegativeInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i].collider;

            if (hit == null || hit.isTrigger)
                continue;

            if (hit.transform.IsChildOf(transform))
                continue;

            if (hit.GetComponentInParent<PlayerController>() != null)
                continue;

            if (hits[i].point.y > bestY)
                bestY = hits[i].point.y;
        }

        float baseY = bestY > float.NegativeInfinity ? bestY : transform.position.y;

        return new Vector3(
            transform.position.x,
            baseY + extraLifeDropHeight,
            transform.position.z);
    }

    // Desvanece y encoge la caja antes de destruirla
    IEnumerator BreakSequence()
    {
        float duration = Mathf.Max(destroyDelay, 0.01f);
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Color startColor = crateRenderer != null ? crateRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(startScale, startScale * 0.35f, t);

            if (crateRenderer != null)
            {
                Color c = startColor;
                c.a = startColor.a * (1f - t);
                crateRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
