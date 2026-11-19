using UnityEngine;

/// <summary>
/// IA sencilla para enemigos dinosaurio 2D: patrulla si no detecta objetivo y persigue al jugador
/// cuando entra en Detection Range. Animator parameter esperado: Speed (float).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class DinosaurEnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float detectionRange = 6f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 2.5f;
    [SerializeField] private float stopDistance = 0.9f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Combate")]
    [SerializeField] private float stompHorizontalMargin = 0.15f;
    [SerializeField, Range(250, 500)] private int stompScore = 250;
    [SerializeField] private float minimumFallSpeed = 0.1f;
    [SerializeField] private float stompTopMargin = 0.15f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 startPosition;
    private int patrolDirection = 1;
    private bool isDead;
    private bool movementControlledExternally;

    public void SetMovementControlledExternally(bool controlled)
    {
        movementControlledExternally = controlled;

        if (controlled && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        startPosition = transform.position;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(targetTag);
            if (player != null) target = player.transform;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || movementControlledExternally) return;

        Vector2 position = rb.position;
        Vector2 destination;
        bool chasing = target != null &&
            Vector2.Distance(position, target.position) <= detectionRange;

        if (chasing)
        {
            destination = target.position;
            if (Vector2.Distance(position, destination) <= stopDistance)
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetFloat("Speed", 0f);
                return;
            }
        }
        else
        {
            float left = startPosition.x - patrolDistance;
            float right = startPosition.x + patrolDistance;
            if (position.x <= left) patrolDirection = 1;
            if (position.x >= right) patrolDirection = -1;
            destination = new Vector2(position.x + patrolDirection, position.y);
        }

        float direction = Mathf.Sign(destination.x - position.x);
        if (Mathf.Abs(destination.x - position.x) < 0.05f) direction = 0f;
        Vector2 velocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = velocity;

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));

        if (spriteRenderer != null && direction != 0f)
            spriteRenderer.flipX = direction < 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        TrexController player = collision.collider.GetComponentInParent<TrexController>();
        Rigidbody2D playerBody = collision.collider.GetComponentInParent<Rigidbody2D>();

        if (player == null || playerBody == null) return;

        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return;

        // Los colliders secundarios representan partes peligrosas, como la cola.
        // Un impacto con ellos siempre mata al jugador, aunque el jugador esté encima.
        if (collision.collider != enemyCollider)
        {
            DinosaurEnemyAI enemyOwner = collision.collider.GetComponentInParent<DinosaurEnemyAI>();
            if (enemyOwner == this)
            {
                player.Die();
                return;
            }
        }

        bool isFalling = playerBody.linearVelocity.y < -minimumFallSpeed;

        bool isAboveEnemy = player.transform.position.y >=
            enemyCollider.bounds.max.y - stompTopMargin;
        bool isOverEnemyLength =
            player.transform.position.x >= enemyCollider.bounds.min.x - stompHorizontalMargin
            && player.transform.position.x <= enemyCollider.bounds.max.x + stompHorizontalMargin;

        if (isFalling && isAboveEnemy && isOverEnemyLength)
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;

            foreach (Collider2D childCollider in GetComponentsInChildren<Collider2D>())
                childCollider.enabled = false;

            HUDManager hud = FindFirstObjectByType<HUDManager>();
            if (hud != null) hud.AddScore(stompScore);

            Destroy(gameObject);
        }
        else
        {
            player.Die();
        }
    }

    private void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // Para añadir combate más adelante: crear un clip Attack y un Trigger "Attack"
    // en el Animator; no se invoca aquí porque la imagen fuente no incluye frames de ataque.
}
