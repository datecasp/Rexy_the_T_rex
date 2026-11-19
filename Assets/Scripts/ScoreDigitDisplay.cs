using UnityEngine;
using UnityEngine.UI;

public class ScoreDigitDisplay : MonoBehaviour
{
    [SerializeField] private Sprite[] digitSprites;
    [SerializeField] private Vector2 digitSize = new Vector2(32f, 32f);
    [SerializeField] private float spacing = 4f;

    private readonly System.Collections.Generic.List<Image> digits =
        new System.Collections.Generic.List<Image>();

    private RectTransform container;

    private void Awake()
    {
        EnsureContainer();
        FindDigitSprites();
    }

    public void SetScore(int score)
    {
        EnsureContainer();
        FindDigitSprites();

        string value = Mathf.Max(0, score).ToString();

        while (digits.Count < value.Length)
        {
            GameObject digitObject = new GameObject("Digit_" + digits.Count);
            digitObject.layer = LayerMask.NameToLayer("UI");

            RectTransform rect = digitObject.AddComponent<RectTransform>();
            rect.SetParent(container, false);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            digits.Add(digitObject.AddComponent<Image>());
        }

        for (int i = 0; i < digits.Count; i++)
        {
            if (i >= value.Length)
            {
                digits[i].gameObject.SetActive(false);
                continue;
            }

            digits[i].gameObject.SetActive(true);

            int digit = value[i] - '0';
            Sprite digitSprite = GetDigitSprite(digit);
            digits[i].sprite = digitSprite;
            digits[i].color = digitSprite != null
                ? Color.white
                : new Color(1f, 1f, 1f, 0f);
            digits[i].preserveAspect = true;

            RectTransform rect = digits[i].rectTransform;
            rect.anchoredPosition = new Vector2(
                i * (digitSize.x + spacing),
                0f);
            rect.sizeDelta = digitSize;
        }

        float width = value.Length * digitSize.x +
            Mathf.Max(0, value.Length - 1) * spacing;
        container.sizeDelta = new Vector2(width, digitSize.y);
    }

    private void EnsureContainer()
    {
        if (container != null) return;

        Transform existing = transform.Find("ScoreDigits");
        if (existing != null)
        {
            container = existing as RectTransform;
            return;
        }

        GameObject containerObject = new GameObject("ScoreDigits");
        containerObject.layer = LayerMask.NameToLayer("UI");

        container = containerObject.AddComponent<RectTransform>();
        container.SetParent(transform, false);
        container.anchorMin = new Vector2(1f, 0.5f);
        container.anchorMax = new Vector2(1f, 0.5f);
        container.pivot = new Vector2(0f, 0.5f);
        container.anchoredPosition = new Vector2(8f, 0f);
    }

    private void FindDigitSprites()
    {
        if (digitSprites != null && digitSprites.Length == 10)
        {
            bool complete = true;
            for (int i = 0; i < 10; i++)
            {
                if (digitSprites[i] == null)
                {
                    complete = false;
                    break;
                }
            }

            if (complete) return;
        }

        digitSprites = new Sprite[10];

        Sprite[] loadedSprites = Resources.LoadAll<Sprite>("DinoScoreDigits_Unity");

        if (loadedSprites.Length == 0)
            loadedSprites = Resources.FindObjectsOfTypeAll<Sprite>();

        if (AllDigitsLoaded()) return;

        // Los nombres de los sub-sprites importados son Digit_0_0, ... Digit_9_0.

        loadedSprites = loadedSprites ?? new Sprite[0];

        for (int i = 0; i < loadedSprites.Length; i++)
        {
            Sprite sprite = loadedSprites[i];
            string name = sprite.name.ToLowerInvariant();

            if (!name.StartsWith("digit_")) continue;

            string[] parts = name.Split('_');

            if (parts.Length >= 3
                && parts[1].Length == 1
                && parts[1][0] >= '0'
                && parts[1][0] <= '9')
            {
                digitSprites[parts[1][0] - '0'] = sprite;
                continue;
            }

            // Compatibilidad con sprites importados con nombres antiguos.

            if (parts.Length >= 2
                && parts[1].Length == 1
                && parts[1][0] >= '0'
                && parts[1][0] <= '9')
            {
                digitSprites[parts[1][0] - '0'] = sprite;
            }
        }
    }

    private bool AllDigitsLoaded()
    {
        for (int i = 0; i < digitSprites.Length; i++)
        {
            if (digitSprites[i] == null)
                return false;
        }

        return true;
    }

    private Sprite GetDigitSprite(int digit)
    {
        if (digitSprites == null || digitSprites.Length <= digit)
            return null;

        return digitSprites[digit];
    }
}
