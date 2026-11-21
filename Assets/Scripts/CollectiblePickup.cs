using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectiblePickup : MonoBehaviour
{
    [Header("Puntuación")]
    [SerializeField] private int score = 50;

    [Header("Física")]
    [SerializeField] private bool useTrigger = true;

    private bool collected;

    private void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider == null)
            pickupCollider = gameObject.AddComponent<BoxCollider2D>();

        pickupCollider.isTrigger = useTrigger;

        Rigidbody2D pickupBody = GetComponent<Rigidbody2D>();
        if (pickupBody != null)
        {
            pickupBody.bodyType = RigidbodyType2D.Kinematic;
            pickupBody.gravityScale = 0f;
        }
    }

    public void Collect()
    {
        if (collected) return;

        collected = true;

        HUDManager hud = FindFirstObjectByType<HUDManager>();
        if (hud != null)
            hud.AddScore(score);

        Destroy(gameObject);
    }
}
