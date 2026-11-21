using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text livesText;

    [Header("Iconos de vidas")]
    [SerializeField] private Image[] lifeIcons;
    [SerializeField] private Sprite dinosaurSprite;

    [Header("Marcador con imágenes")]
    [SerializeField] private ScoreDigitDisplay scoreDigitDisplay;

    [SerializeField] private string gameOverSceneName = "GameOver";

    private int score;
    private static int lives = 3;

    private void Awake()
    {
        FindLifeIcons();
        FindScoreText();
        FindScoreDigitDisplay();
    }

    private void FindScoreDigitDisplay()
    {
        if (scoreDigitDisplay != null) return;

        if (scoreText != null)
        {
            scoreDigitDisplay = scoreText.GetComponent<ScoreDigitDisplay>();
            if (scoreDigitDisplay == null)
                scoreDigitDisplay = scoreText.gameObject.AddComponent<ScoreDigitDisplay>();
        }
    }

    private void FindScoreText()
    {
        if (scoreText != null) return;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].gameObject.name.Contains("Puntos")
                || texts[i].gameObject.name.Contains("PUNTOS"))
            {
                scoreText = texts[i];
                return;
            }
        }
    }

    private void Start()
    {
        lives = 3;
        UpdateHUD();
    }

    public void ResetLives()
    {
        lives = 3;
        UpdateHUD();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateHUD();
    }

    public bool LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        UpdateHUD();

        if (lives == 0)
        {
            SceneManager.LoadScene(gameOverSceneName);
            return false;
        }

        return true;
    }

    public bool HasLivesRemaining()
    {
        return lives > 0;
    }

    private void FindLifeIcons()
    {
        if (lifeIcons != null && lifeIcons.Length > 0) return;

        System.Collections.Generic.List<Image> foundIcons =
            new System.Collections.Generic.List<Image>();

        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject.name.StartsWith("dinosaur_")
                || images[i].gameObject.name.Contains("dinosaur"))
                foundIcons.Add(images[i]);
        }

        foundIcons.Sort((a, b) =>
            a.gameObject.name.CompareTo(b.gameObject.name));

        if (foundIcons.Count > 0)
        {
            if (foundIcons.Count > 3)
                foundIcons = foundIcons.GetRange(0, 3);

            lifeIcons = foundIcons.ToArray();
            return;
        }

        Sprite iconSprite = dinosaurSprite;
        if (iconSprite == null)
        {
            SpriteRenderer[] worldIcons = FindObjectsByType<SpriteRenderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < worldIcons.Length; i++)
            {
                if (worldIcons[i].sprite == null) continue;

                if (worldIcons[i].gameObject.name.Contains("dinosaur"))
                {
                    iconSprite = worldIcons[i].sprite;
                    worldIcons[i].gameObject.SetActive(false);
                }
            }
        }

        if (iconSprite == null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        SpriteRenderer[] positionedIcons = FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        System.Collections.Generic.List<SpriteRenderer> dinosaurIcons =
            new System.Collections.Generic.List<SpriteRenderer>();

        for (int i = 0; i < positionedIcons.Length; i++)
        {
            if (positionedIcons[i].sprite != null
                && positionedIcons[i].gameObject.name.Contains("dinosaur"))
            {
                dinosaurIcons.Add(positionedIcons[i]);
            }
        }

        dinosaurIcons.Sort((a, b) =>
            a.gameObject.name.CompareTo(b.gameObject.name));

        for (int i = 0; i < 3; i++)
        {
            GameObject iconObject = new GameObject("dinosaur_" + i);
            iconObject.layer = LayerMask.NameToLayer("UI");

            RectTransform rect = iconObject.AddComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Vector3 desiredPosition = new Vector3(-300f + i * 75f, 220f, 0f);
            if (i < dinosaurIcons.Count)
                desiredPosition = dinosaurIcons[i].transform.localPosition;

            rect.anchoredPosition = new Vector2(desiredPosition.x, desiredPosition.y);
            rect.sizeDelta = new Vector2(48f, 48f);

            Image icon = iconObject.AddComponent<Image>();
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            foundIcons.Add(icon);

            if (i < dinosaurIcons.Count)
                dinosaurIcons[i].gameObject.SetActive(false);
        }

        lifeIcons = foundIcons.ToArray();

        SpriteRenderer[] remainingIcons = FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < remainingIcons.Length; i++)
        {
            if (remainingIcons[i].gameObject.name.Contains("dinosaur"))
                remainingIcons[i].gameObject.SetActive(false);
        }
    }

    public void UpdateHUD()
    {
        FindLifeIcons();
        FindScoreText();

        if (scoreText != null)
        {
            scoreText.text = string.Empty;
            scoreText.enabled = false;
        }

        FindScoreDigitDisplay();
        if (scoreDigitDisplay != null)
            scoreDigitDisplay.SetScore(score);

        if (livesText != null)
        {
            livesText.text = "Vidas: " + lives;
            livesText.enabled = false;
        }

        if (lifeIcons == null) return;

        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].enabled = i < lives;
        }
    }
}
