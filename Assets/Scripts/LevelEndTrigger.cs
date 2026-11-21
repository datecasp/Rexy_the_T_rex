using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private string levelClearSceneName = "LevelClear";

    private bool activated;

    private void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        TrexController player = other.GetComponentInParent<TrexController>();
        if (player == null)
            return;

        activated = true;
        player.enabled = false;
        SceneManager.LoadScene(levelClearSceneName);
    }
}
