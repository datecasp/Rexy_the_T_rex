using UnityEngine;
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

    private Rigidbody2D rb;
    private TrexAnimationDriver anim;
    private SpriteRenderer sprite;

    private float movement;
    private bool isGrounded;
    private bool isRunning;

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
    }

    private void Update()
    {
        // Movimiento con teclado
        movement = Input.GetAxisRaw("Horizontal");

        // Carrera con teclado
        isRunning = Input.GetKey(runKey);

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

        // Salto con teclado
        if (Input.GetKeyDown(jumpKey) && isGrounded)
            Jump();

        // Ataque con teclado
        if (Input.GetKeyDown(attackKey))
            Attack();
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
}