
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SpiderEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    public float speed = 1.5f;
    public float patrolDistance = 1.5f;
    public bool startsFacingRight = true;

    [Header("Ataque")]
    public float attackDuration = 0.6f;

    [Header("Pisoton")]
    public float stompBounceForce = 10f;
    public float stompTolerance = 0.4f;
    public float stompMinFallSpeed = 0.1f;

    [Header("Muerte")]
    public float deathDuration = 0.8f;

    [Header("Recompensa")]
    public int boneReward = 10;

    [Tooltip("Identificador unico y estable. Evita otorgar la recompensa dos veces.")]
    public string rewardId = "";

    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D enemyCollider;

    private float patrolTime;
    private bool attacking;
    private bool dead;

    // Valores del parametro entero "State" del controlador Spider.controller
    private const int StateWalk = 1;
    private const int StateAttack = 3;
    private const int StateDeath = 5;

    private const string DeathStateName = "Spider_Death";

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        initialPosition = transform.position;
        spriteRenderer.flipX = !startsFacingRight;
    }

    void Update()
    {
        if (dead || attacking)
            return;

        // Movimiento de patrulla
        patrolTime += Time.deltaTime;

        float offset = Mathf.Sin(
            patrolTime * speed / Mathf.Max(patrolDistance, 0.01f)
        ) * patrolDistance;

        float nextOffset = Mathf.Sin(
            (patrolTime + 0.02f) * speed /
            Mathf.Max(patrolDistance, 0.01f)
        ) * patrolDistance;

        transform.position = initialPosition +
            new Vector3(offset, 0f, 0f);

        // Orientacion de la araña
        bool movingRight = nextOffset > offset;
        spriteRenderer.flipX = startsFacingRight
            ? !movingRight
            : movingRight;

        // Animacion de caminar
        if (animator != null)
            animator.SetInteger("State", StateWalk);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || attacking)
            return;

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        // Un jugador ya muerto (o que ya gano) no recibe mas daño, no puede
        // seguir eliminando arañas ni cobrar recompensas
        if (player.IsDead || player.IsLevelCompleted)
            return;

        // Pisoton: los pies del jugador llegan sobre la parte superior de la araña
        if (IsStompFromAbove(other))
        {
            player.StompBounce(stompBounceForce);
            Die(true);
            return;
        }

        // Contacto lateral o desde abajo: daña al jugador
        player.Die();
        StartCoroutine(AttackSequence());
    }

    // Detecta el pisoton combinando tres señales para evitar falsos positivos:
    // velocidad vertical de caida, altura de los pies y centro del jugador.
    bool IsStompFromAbove(Collider2D playerCollider)
    {
        if (playerCollider == null || enemyCollider == null)
            return false;

        Rigidbody2D playerRb = playerCollider.attachedRigidbody;

        bool descending = playerRb != null &&
            playerRb.linearVelocity.y < -stompMinFallSpeed;

        if (!descending)
            return false;

        Bounds playerBounds = playerCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;

        // Pies del jugador contra la parte superior de la araña
        bool feetOnTop = playerBounds.min.y >=
            enemyBounds.max.y - stompTolerance;

        // El centro del jugador debe quedar por encima del centro de la araña
        bool centerAbove = playerBounds.center.y > enemyBounds.center.y;

        return feetOnTop && centerAbove;
    }

    IEnumerator AttackSequence()
    {
        attacking = true;

        if (animator != null)
            animator.SetInteger("State", StateAttack);

        yield return new WaitForSeconds(attackDuration);

        if (!dead)
        {
            attacking = false;

            if (animator != null)
                animator.SetInteger("State", StateWalk);
        }
    }

    // Muerte por el ataque del jugador (no muestra mensaje en pantalla)
    public void Die()
    {
        Die(false);
    }

    // Muerte al ser aplastada desde arriba (muestra el mensaje de enemigo eliminado)
    public void Die(bool showKillMessage)
    {
        if (dead)
            return;

        dead = true;
        attacking = false;
        StopAllCoroutines();

        // Una araña muerta ya no daña al jugador ni puede volver a recibir golpes
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // Entrada forzada al estado de muerte: ninguna transicion posterior lo interrumpe
        if (animator != null)
        {
            animator.SetInteger("State", StateDeath);
            animator.Play(DeathStateName, 0, 0f);
        }

        if (showKillMessage)
            EnemyKillMessageUI.Show();

        // Recompensa en huesitos: una sola vez por araña
        AwardBones();

        StartCoroutine(DeathSequence());
    }

    // Entrega los huesitos de la araña una unica vez
    void AwardBones()
    {
        if (boneReward <= 0)
            return;

        // No se otorgan recompensas si el jugador ya ha muerto
        PlayerController player = PlayerController.Current;

        if (player != null && player.IsDead)
            return;

        string key = string.IsNullOrEmpty(rewardId)
            ? "spider:" + name + ":" + Mathf.RoundToInt(initialPosition.x * 10f)
            : "spider:" + rewardId;

        if (!BoneScoreManager.AddBonesOnce(key, boneReward))
            return;

        BoneHudUI.ShowPopup(
            transform.position + Vector3.up * 0.7f,
            "+" + boneReward);

        BoneFeedback.PlaySpiderReward(
            spriteRenderer != null ? spriteRenderer.sprite : null,
            transform.position + Vector3.up * 0.3f,
            spriteRenderer != null ? spriteRenderer.sortingOrder : 8);
    }

    IEnumerator DeathSequence()
    {
        // La araña desaparece cuando termina la animacion de muerte
        float waitTime = Mathf.Max(deathDuration, GetDeathClipLength());

        yield return new WaitForSeconds(waitTime);

        Destroy(gameObject);
    }

    float GetDeathClipLength()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == DeathStateName)
                return clips[i].length;
        }

        return 0f;
    }
}
