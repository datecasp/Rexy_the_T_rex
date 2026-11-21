using UnityEngine;
using UnityEngine.SceneManagement;
using TrexSprite;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(TrexAnimationDriver))]
public class TrexController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Input")]
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.F;

    [Header("Respawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1f;

    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    private TrexAnimationDriver anim;
    private SpriteRenderer sprite;

    private float movement;
    private bool isGrounded;
    private bool isRunning;
    private bool isDead;

    private void Reset()
    {
        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");
    }

    private void Awake()
    {
        if (groundCheck == null)
            groundCheck = transform.Find("GroundCheck");

        if (GetComponent<Rigidbody2D>() == null)
            gameObject.AddComponent<Rigidbody2D>();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<TrexAnimationDriver>();
        sprite = GetComponent<SpriteRenderer>();

        initialPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        initialRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        initialScale = transform.localScale;
    }

    private void Update()
    {
        if (!Application.isMobilePlatform)
        {
            movement = Input.GetAxisRaw("Horizontal");
            isRunning = Input.GetKey(runKey);

            if (Input.GetKeyDown(jumpKey) && isGrounded)
                Jump();

            if (Input.GetKeyDown(attackKey))
                Attack();
        }

        // Comprobar suelo
        if (groundCheck != null)
            isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);

        // Mirar izquierda/derecha
        if (movement > 0)
            sprite.flipX = true;
        else if (movement < 0)
            sprite.flipX = false;

        anim.SetMoveSpeed(Mathf.Abs(movement));
        anim.SetRunning(isRunning);

    }

    private void FixedUpdate()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(movement * speed, rb.linearVelocity.y);
    }

    public void MoveHorizontal(float value)
    {
        movement = value;
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public void Jump()
    {
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        anim.Jump();
    }

    public void Attack()
    {
        anim.Attack();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollectiblePickup pickup = other.GetComponent<CollectiblePickup>();
        if (pickup == null)
            pickup = other.GetComponentInParent<CollectiblePickup>();

        if (pickup != null)
            pickup.Collect();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollectiblePickup pickup = collision.collider.GetComponent<CollectiblePickup>();
        if (pickup == null)
            pickup = collision.collider.GetComponentInParent<CollectiblePickup>();

        if (pickup != null)
            pickup.Collect();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        enabled = false;
        movement = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        HUDManager hud = FindFirstObjectByType<HUDManager>();
        bool canRespawn = hud != null
            ? hud.LoseLife()
            : false;

        if (canRespawn)
            Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = true;
        isGrounded = false;
        isDead = false;
        enabled = true;
    }
}
