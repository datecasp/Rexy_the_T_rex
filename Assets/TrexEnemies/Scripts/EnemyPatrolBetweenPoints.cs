using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class EnemyPatrolBetweenPoints : MonoBehaviour
{
    [Header("Rango de patrulla relativo a la posición inicial")]
    [SerializeField] private Vector2 patrolRange = new Vector2(-2.5f, 2.5f);

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reachDistance = 0.08f;

    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [SerializeField] private bool flipSpriteWithDirection = true;

    private Rigidbody2D rb;
    private DinosaurEnemyAI dinosaurAI;
    private float spawnPositionX;
    private float leftPositionX;
    private float rightPositionX;
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();

        dinosaurAI = GetComponent<DinosaurEnemyAI>();
        if (dinosaurAI != null)
            dinosaurAI.SetMovementControlledExternally(true);

        spawnPositionX = transform.position.x;
        UpdatePatrolBounds();

        if (transform.position.x > (leftPositionX + rightPositionX) * 0.5f)
            direction = -1;
    }

    private void OnValidate()
    {
        if (patrolRange.x > 0f)
            patrolRange.x = 0f;

        if (patrolRange.y < 0f)
            patrolRange.y = 0f;

        patrolRange.x = Mathf.Min(patrolRange.x, patrolRange.y - 0.25f);
        patrolRange.y = Mathf.Max(patrolRange.y, patrolRange.x + 0.25f);
    }

    private void UpdatePatrolBounds()
    {
        leftPositionX = spawnPositionX + Mathf.Min(patrolRange.x, -0.25f);
        rightPositionX = spawnPositionX + Mathf.Max(patrolRange.y, 0.25f);
    }

    private void FixedUpdate()
    {
        UpdatePatrolBounds();

        float positionX = rb.position.x;
        if (positionX <= leftPositionX + reachDistance)
            direction = 1;
        else if (positionX >= rightPositionX - reachDistance)
            direction = -1;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(direction * moveSpeed));

        if (spriteRenderer != null && flipSpriteWithDirection)
            spriteRenderer.flipX = direction < 0f;
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (dinosaurAI != null)
            dinosaurAI.SetMovementControlledExternally(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float leftX = transform.position.x + patrolRange.x;
        float rightX = transform.position.x + patrolRange.y;

        Vector3 center = new Vector3((leftX + rightX) * 0.5f, transform.position.y, 0f);

        Gizmos.color = new Color(1f, 0.85f, 0f, 0.15f);
        Gizmos.DrawCube(center, new Vector3(rightX - leftX, 0.25f, 0f));

        Gizmos.color = new Color(1f, 0.85f, 0f, 0.95f);
        Gizmos.DrawWireCube(center, new Vector3(rightX - leftX, 0.25f, 0f));
        Gizmos.DrawLine(new Vector3(leftX, transform.position.y - 0.35f, 0f),
            new Vector3(leftX, transform.position.y + 0.35f, 0f));
        Gizmos.DrawLine(new Vector3(rightX, transform.position.y - 0.35f, 0f),
            new Vector3(rightX, transform.position.y + 0.35f, 0f));
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, 0f), 0.14f);
        Gizmos.DrawSphere(new Vector3(leftX, transform.position.y, 0f), 0.12f);
        Gizmos.DrawSphere(new Vector3(rightX, transform.position.y, 0f), 0.12f);
    }
#endif
}
