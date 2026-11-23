using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ScreenSceneSetup
{
    [MenuItem("Tools/Rexy 2D/Crear escenas de pantallas")]
    public static void CreateScreenScenes()
    {
        CreateScreenScene("Assets/Scenes/MainMenu.unity", "01_MainMenu_RexyTheTrex", "01_MainMenu_RexyTheTrex_0", ScreenMenuController.ScreenAction.StartGame);
        CreateScreenScene("Assets/Scenes/LevelClear.unity", "03_LevelClear", "03_LevelClear_0", ScreenMenuController.ScreenAction.MainMenu);
        CreateScreenScene("Assets/Scenes/GameOver.unity", "02_GameOver", "02_GameOver_0", ScreenMenuController.ScreenAction.RetryGame);
        CreateScreenScene("Assets/Scenes/Info.unity", "Info", "Info", ScreenMenuController.ScreenAction.MainMenu);

        ConfigureMainMenuInformationButton();
        SetupLevelEndTrigger();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Rexy 2D", "Escenas y final del nivel configurados.", "Aceptar");
    }

    private static void CreateScreenScene(string scenePath, string textureName, string spriteName, ScreenMenuController.ScreenAction action)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        Canvas canvas = new GameObject("ScreenCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject screen = new GameObject("Screen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScreenMenuController));
        screen.transform.SetParent(canvas.transform, false);

        RectTransform rect = screen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Screens/" + textureName + ".png");
        if (sprite == null)
            sprite = FindSpriteByName(spriteName);

        Image image = screen.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = false;
        image.raycastTarget = true;
        image.color = Color.white;

        ScreenMenuController controller = screen.GetComponent<ScreenMenuController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("action").enumValueIndex = (int)action;
        serializedController.FindProperty("gameSceneName").stringValue = "Level1_Bootstrap";
        serializedController.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
        serializedController.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    private static void ConfigureMainMenuInformationButton()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        GameObject buttonObject = GameObject.Find("btn_info");

        if (buttonObject == null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            buttonObject = new GameObject("btn_info", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ScreenMenuController));
            buttonObject.transform.SetParent(canvas.transform, false);
        }
        else
        {
            RectTransform existingRect = buttonObject.GetComponent<RectTransform>();
            if (existingRect == null)
            {
                GameObject replacement = new GameObject("btn_info", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ScreenMenuController));
                replacement.transform.SetParent(Object.FindFirstObjectByType<Canvas>().transform, false);
                Object.DestroyImmediate(buttonObject);
                buttonObject = replacement;
            }
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -290f);
        rect.sizeDelta = new Vector2(420f, 150f);

        Image image = buttonObject.GetComponent<Image>();
        if (image == null)
            image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
            button = buttonObject.AddComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => buttonObject.GetComponent<ScreenMenuController>().ExecuteAction());

        ScreenMenuController controller = buttonObject.GetComponent<ScreenMenuController>();
        if (controller == null)
            controller = buttonObject.AddComponent<ScreenMenuController>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("action").enumValueIndex = (int)ScreenMenuController.ScreenAction.Information;
        serializedController.FindProperty("gameSceneName").stringValue = "Level1_Bootstrap";
        serializedController.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
        serializedController.FindProperty("informationSceneName").stringValue = "Info";
        serializedController.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Sprite FindSpriteByName(string spriteName)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Screens" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        return null;
    }

    private static void SetupLevelEndTrigger()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Level1_Chunk_IslaFinal.unity");
        GameObject cave = GameObject.Find("cuevafinal");

        if (cave == null)
        {
            Debug.LogWarning("No se encontró el prefab cuevafinal en la escena.");
        }
        else
        {
            LevelEndTrigger trigger = cave.GetComponent<LevelEndTrigger>();
            if (trigger == null)
                trigger = cave.AddComponent<LevelEndTrigger>();

            BoxCollider2D collider = cave.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = cave.AddComponent<BoxCollider2D>();

            collider.isTrigger = true;
            collider.size = new Vector2(4f, 5f);
            collider.offset = Vector2.zero;

            EditorUtility.SetDirty(trigger);
        }

        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/Rexy 2D/Actualizar escenas de compilacion")]
    public static void UpdateBuildSettings()
    {
        string[] scenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level1_Bootstrap.unity",
            "Assets/Scenes/Level1_Chunk_Isla1.unity",
            "Assets/Scenes/Level1_Chunk_Isla2.unity",
            "Assets/Scenes/Level1_Chunk_Intermedio.unity",
            "Assets/Scenes/Level1_Chunk_Isla3.unity",
            "Assets/Scenes/Level1_Chunk_Plataformas.unity",
            "Assets/Scenes/Level1_Chunk_IslaFinal.unity",
            "Assets/Scenes/LevelClear.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/Info.unity"
        };

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
            scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);

        EditorBuildSettings.scenes = scenes;
    }
}
