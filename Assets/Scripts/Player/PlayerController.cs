using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    [Header("Salto")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    public float doubleJumpForce = 10f;
    [Header("Deteccion del suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    [Header("Ataque")]
    public Transform attackPoint;
    public float attackRadius = 2.2f;
    public float attackOffsetX = 2f;
    public float attackOffsetY = -1.2f;
    public float attackCooldown = 0.45f;
    public float attackVisualDuration = 0.12f;
    [Header("Muerte")]
    public float deathY = -10f;
    [Tooltip("Espera de reserva si el clip Player_Death no se puede leer.")]
    public float deathDelay = 1.2f;
    [Tooltip("Pausa final con la ultima pose de la animacion antes de reiniciar.")]
    public float deathPoseHold = 0.3f;
    private const string DeathStateName = "Player_Death";
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float horizontal;
    private bool isGrounded;
    private bool isRunning;
    private bool jumpRequested;
    private bool isDead;
    private bool levelCompleted;
    // Acceso de solo lectura para que otros sistemas consulten el estado del jugador
    public static PlayerController Current { get; private set; }
    public bool IsDead => isDead;
    public bool IsLevelCompleted => levelCompleted;
    private int jumpsRemaining;
    private float pendingJumpForce;
    private bool facingRight = true;
    private bool wasGrounded;
    private float nextAttackTime;
    private SpriteRenderer slashRenderer;
    private float slashTimer;
    void Awake()
    {
        Current = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        jumpsRemaining = maxJumps;
        CreateAttackVisual();
    }
    void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }
    void Update()
    {
        UpdateAttackVisual();
        // Reiniciar manualmente
        if (!isDead && !levelCompleted && Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
            return;
        }
        if (isDead || levelCompleted)
            return;
        // Detectar caida al vacio
        if (transform.position.y < deathY)
        {
            Die();
            return;
        }
        // Movimiento horizontal
        horizontal = Input.GetAxisRaw("Horizontal");
        // Correr con Shift
        isRunning = Input.GetKey(KeyCode.LeftShift)
                 || Input.GetKey(KeyCode.RightShift);
        // Comprobar si esta en el suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null;
        }
        // Recuperar saltos SOLO con aterrizaje valido sobre la capa Ground
        if (isGrounded && rb.linearVelocity.y <= 0.05f && !jumpRequested)
        {
            jumpsRemaining = maxJumps;
            wasGrounded = true;
        }
        else if (!isGrounded)
        {
            // Solo al caer de una plataforma sin saltar se pierde el primer salto
            if (wasGrounded && !jumpRequested)
                jumpsRemaining = Mathf.Min(jumpsRemaining, maxJumps - 1);
            wasGrounded = false;
        }
        // Saltar / doble salto (GetKeyDown impide repeticiones al mantener pulsado)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryJump();
        }
        // Atacar
        if (Input.GetKeyDown(KeyCode.J))
        {
            TryAttack();
        }
        // Girar personaje
        if (horizontal > 0.01f)
        {
            spriteRenderer.flipX = false;
            facingRight = true;
        }
        else if (horizontal < -0.01f)
        {
            spriteRenderer.flipX = true;
            facingRight = false;
        }
        UpdateAnimations();
    }
    void FixedUpdate()
    {
        if (isDead || levelCompleted)
            return;
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(
            horizontal * speed,
            rb.linearVelocity.y
        );
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                pendingJumpForce
            );
            jumpRequested = false;
            isGrounded = false;
        }
    }
    void UpdateAnimations()
    {
        int state;
        if (!isGrounded)
        {
            state = rb.linearVelocity.y > 0.05f ? 2 : 3;
        }
        else if (Mathf.Abs(horizontal) > 0.01f)
        {
            state = isRunning ? 5 : 1;
        }
        else
        {
            state = 0;
        }
        animator.SetInteger("State", state);
    }
    public void Die()
    {
        if (isDead || levelCompleted)
            return;
        isDead = true;
        if (LivesManager.Instance != null)
        LivesManager.Instance.LoseLife();
        horizontal = 0f;
        isRunning = false;
        jumpRequested = false;
        pendingJumpForce = 0f;
        jumpsRemaining = 0;
        // La fisica vertical se mantiene activa: el cuerpo sigue cayendo y
        // aterriza con normalidad durante la animacion. Desactivar simulated
        // (o pasar el cuerpo a Kinematic) congelaba al jugador en el aire y lo
        // dejaba superpuesto a la araña.
        rb.simulated = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        // El ataque en curso no debe quedar flotando sobre el cadaver
        if (slashRenderer != null)
            slashRenderer.enabled = false;
        // Entrada forzada al estado de muerte desde el primer fotograma
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetInteger("State", 4);
            animator.Play(DeathStateName, 0, 0f);
        }
        StartCoroutine(DeathSequence());
    }
    IEnumerator DeathSequence()
    {
        // Esperar al final de Player_Death antes de reiniciar o mostrar Game Over.
        float clipLength = GetDeathClipLength();
        float wait = clipLength > 0.01f
            ? clipLength + Mathf.Max(0f, deathPoseHold)
            : Mathf.Max(deathDelay, 0.1f);
        yield return new WaitForSeconds(wait);
        if (LivesManager.Instance != null && LivesManager.CurrentLives <= 0)
        {
            // La tercera muerte NO reinicia automaticamente el nivel.
            BoneScoreManager.ResetScore();
            LivesManager.Instance.ShowGameOver();
        }
        else
        {
            // Se conservan las vidas restantes, pero no los huesitos.
            BoneScoreManager.ResetScore();
            LevelRestarter.RestartCurrentLevel();
        }
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
    // Primer salto desde el suelo con jumpForce; los aereos con doubleJumpForce
    public bool TryJump()
    {
        if (isDead || levelCompleted)
            return false;
        if (jumpsRemaining <= 0)
            return false;
        bool firstJump = isGrounded;
        pendingJumpForce = firstJump ? jumpForce : doubleJumpForce;
        jumpRequested = true;
        jumpsRemaining--;
        return true;
    }
    // Rebote del pisoton: no consume el doble salto
    public void StompBounce(float force)
    {
        if (isDead || levelCompleted)
            return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        jumpsRemaining = maxJumps;
        jumpRequested = false;
    }
    public bool TryAttack()
    {
        if (isDead || levelCompleted)
            return false;
        if (Time.time < nextAttackTime)
            return false;
        nextAttackTime = Time.time + attackCooldown;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            GetAttackCenter(),
            attackRadius
        );
        for (int i = 0; i < hits.Length; i++)
        {
            SpiderEnemy spider = hits[i].GetComponentInParent<SpiderEnemy>();
            if (spider != null)
                spider.Die();
        }
        ShowAttackVisual();
        return true;
    }
    Vector2 GetAttackCenter()
    {
        float sign = facingRight ? 1f : -1f;
        if (attackPoint != null)
        {
            attackPoint.localPosition = new Vector3(
                attackOffsetX * sign,
                attackOffsetY,
                0f
            );
            return (Vector2)attackPoint.position;
        }
        return (Vector2)transform.position +
            new Vector2(attackOffsetX * sign, attackOffsetY);
    }
    void CreateAttackVisual()
    {
        if (attackPoint == null)
        {
            GameObject point = new GameObject("AttackPoint");
            point.transform.SetParent(transform, false);
            attackPoint = point.transform;
        }
        else
        {
            attackOffsetX = Mathf.Abs(attackPoint.localPosition.x);
            attackOffsetY = attackPoint.localPosition.y;
        }
        attackPoint.localPosition = new Vector3(attackOffsetX, attackOffsetY, 0f);
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite slashSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );
        GameObject slash = new GameObject("AttackSlash");
        slash.transform.SetParent(attackPoint, false);
        slash.transform.localPosition = Vector3.zero;
        slash.transform.localScale = new Vector3(
            attackRadius * 1.2f,
            attackRadius * 0.8f,
            1f
        );
        slashRenderer = slash.AddComponent<SpriteRenderer>();
        slashRenderer.sprite = slashSprite;
        slashRenderer.color = new Color(1f, 1f, 1f, 0.45f);
        slashRenderer.sortingOrder = 11;
        slashRenderer.enabled = false;
    }
    void ShowAttackVisual()
    {
        if (slashRenderer == null)
            return;
        slashRenderer.enabled = true;
        slashTimer = attackVisualDuration;
    }
    void UpdateAttackVisual()
    {
        if (slashRenderer == null || !slashRenderer.enabled)
            return;
        slashTimer -= Time.deltaTime;
        if (slashTimer <= 0f)
            slashRenderer.enabled = false;
    }
    public void CompleteLevel()
    {
        if (isDead || levelCompleted)
            return;
        levelCompleted = true;
        horizontal = 0;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        animator.SetInteger("State", 0);
        Debug.Log("Nivel completado");
    }
    public void RestartLevel()
    {
        // R empieza un intento nuevo con tres vidas y cero huesitos.
        LivesManager.ResetLives();
        BoneScoreManager.ResetScore();
        LevelRestarter.RestartCurrentLevel();
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            (Vector2)transform.position +
                new Vector2(attackOffsetX, attackOffsetY),
            attackRadius
        );
    }
}
