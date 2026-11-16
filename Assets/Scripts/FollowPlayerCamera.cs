using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow settings")]
    public float smoothSpeed = 5f;
    public float jumpFollowThreshold = 1.5f; // altura mínima antes de empezar a subir la cámara

    private float groundAnchorY;
    private bool hasGroundAnchor;

    private void Start()
    {
        if (target != null)
        {
            groundAnchorY = target.position.y;
            hasGroundAnchor = true;
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        // Mantener un ancla del suelo mientras el jugador está cerca del suelo
        if (target.position.y <= groundAnchorY + 0.5f)
        {
            groundAnchorY = target.position.y;
            hasGroundAnchor = true;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float desiredX = target.position.x + offset.x;
        float desiredY;

        // Si el jugador está por encima del umbral de salto, la cámara empieza a seguirle
        if (target.position.y > groundAnchorY + jumpFollowThreshold)
        {
            desiredY = target.position.y + offset.y;
        }
        else
        {
            // Mientras salta poco, se queda casi fija en el suelo
            desiredY = groundAnchorY + offset.y;
        }

        Vector3 desiredPosition = new Vector3(desiredX, desiredY, offset.z);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}