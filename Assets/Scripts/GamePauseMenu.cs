using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameObject pausePanel;
    private bool isPaused;

    public bool IsPaused
    {
        get { return isPaused; }
    }

    private void Start()
    {
        CreateInterface();
    }

    private void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!Application.isMobilePlatform)
        {
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    private void CreateInterface()
    {
        Canvas canvas = FindOrCreateCanvas();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        if (Application.isMobilePlatform)
        {
            Button pauseButton = CreateButton(
                canvas.transform,
                "II",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-62f, -62f),
                new Vector2(92f, 92f),
                new Color(0.10f, 0.14f, 0.22f, 0.96f),
                42);
            pauseButton.onClick.AddListener(TogglePause);

            Button appButton = CreateButton(
                canvas.transform,
                "×",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-62f, -166f),
                new Vector2(92f, 92f),
                new Color(0.58f, 0.16f, 0.18f, 0.96f),
                48);
            appButton.onClick.AddListener(CloseApplication);
        }

        if (Application.isMobilePlatform)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }

        CanvasScaler existingScaler = canvas.GetComponent<CanvasScaler>();
        if (existingScaler == null)
        {
            existingScaler = canvas.gameObject.AddComponent<CanvasScaler>();
            existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            existingScaler.referenceResolution = new Vector2(1920f, 1080f);
            existingScaler.matchWidthOrHeight = 0.5f;
        }

        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        pausePanel = CreatePausePanel(canvas.transform);
        pausePanel.SetActive(false);
    }

    private Canvas FindOrCreateCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
            return canvas;

        GameObject canvasObject = new GameObject("PauseMenuCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private GameObject CreatePausePanel(Transform parent)
    {
        GameObject overlay = new GameObject("PauseOverlay");
        overlay.transform.SetParent(parent, false);

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0.02f, 0.035f, 0.07f, 0.92f);

        GameObject card = new GameObject("PauseCard");
        card.transform.SetParent(overlay.transform, false);

        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = Application.isMobilePlatform
            ? new Vector2(760f, 980f)
            : new Vector2(620f, 700f);

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.07f, 0.10f, 0.16f, 1f);
        Outline cardOutline = card.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0.30f, 0.66f, 0.92f, 0.75f);
        cardOutline.effectDistance = new Vector2(3f, -3f);

        CreateTitle(card.transform, "PAUSA");

        float buttonWidth = Application.isMobilePlatform ? 620f : 500f;
        float buttonHeight = Application.isMobilePlatform ? 120f : 92f;

        CreateCenteredButton(card.transform, "CONTINUAR", 55f, buttonWidth, buttonHeight,
            new Color(0.16f, 0.55f, 0.36f, 1f), Resume);
        CreateCenteredButton(card.transform, "REINICIAR NIVEL", -75f, buttonWidth, buttonHeight,
            new Color(0.20f, 0.36f, 0.58f, 1f), RestartLevel);
        CreateCenteredButton(card.transform, "VOLVER AL MENÚ", -205f, buttonWidth, buttonHeight,
            new Color(0.24f, 0.26f, 0.40f, 1f), LoadMainMenu);
        CreateCenteredButton(card.transform, "CERRAR APP", -335f, buttonWidth, buttonHeight,
            new Color(0.62f, 0.19f, 0.22f, 1f), CloseApplication);

        return overlay;
    }

    private void CreateTitle(Transform parent, string title)
    {
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(parent, false);

        RectTransform titleRect = titleObject.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -45f);
        titleRect.sizeDelta = new Vector2(500f, 100f);

        Text titleText = titleObject.AddComponent<Text>();
        titleText.text = title;
        titleText.font = Font.CreateDynamicFontFromOSFont("Arial", 46);
        titleText.fontSize = Application.isMobilePlatform ? 62 : 46;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.88f, 0.94f, 1f, 1f);

        Shadow shadow = titleObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(3f, -3f);
    }

    private Button CreateCenteredButton(
        Transform parent,
        string text,
        float y,
        float width,
        float height,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(parent, text,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, y),
            new Vector2(width, height),
            color,
            Application.isMobilePlatform ? 34 : 28);
        button.onClick.AddListener(action);
        return button;
    }

    private Button CreateButton(
        Transform parent,
        string text,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        Color color,
        int fontSize)
    {
        GameObject buttonObject = new GameObject(text);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.02f, 0.04f, 0.07f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return button;
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void CloseApplication()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
