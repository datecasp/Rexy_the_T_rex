using UnityEngine;

public class FallDeathZone : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float deathLineY = 0f;
    [SerializeField] private float lineWidth = 100f;

    private void Reset()
    {
        transform.position = new Vector3(transform.position.x, deathLineY, 0f);
    }

    private void Awake()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
            FindPlayer();

        if (player != null)
        {
            float playerBottom = player.position.y;
            Collider2D playerCollider = player.GetComponentInChildren<Collider2D>();
            if (playerCollider != null)
                playerBottom = playerCollider.bounds.min.y;

            if (playerBottom <= deathLineY)
            {
                TrexController controller = player.GetComponentInParent<TrexController>();
                if (controller != null)
                    controller.Die();
            }
        }
    }

    private void FindPlayer()
    {
        if (player != null) return;

        TrexController controller = FindFirstObjectByType<TrexController>();
        if (controller != null)
            player = controller.transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TrexController controller = other.GetComponentInParent<TrexController>();
        if (controller != null)
            controller.Die();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
        Gizmos.DrawLine(
            new Vector3(transform.position.x - lineWidth * 0.5f, deathLineY, 0f),
            new Vector3(transform.position.x + lineWidth * 0.5f, deathLineY, 0f));
    }
#endif
}
