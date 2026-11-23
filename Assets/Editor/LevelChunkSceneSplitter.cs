using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelChunkSceneSplitter
{
    private const string SourceScene = "Assets/Scenes/Level1_Scene.unity";
    private const string BackupScene = "Assets/Scenes/Level1_Scene_Backup.unity";
    private const string BootstrapScene = "Assets/Scenes/Level1_Bootstrap.unity";

    private static readonly ChunkDefinition[] ChunkDefinitions =
    {
        new ChunkDefinition("Isla1", "Level1_Chunk_Isla1", "Assets/Scenes/Level1_Chunk_Isla1.unity"),
        new ChunkDefinition("Isla2", "Level1_Chunk_Isla2", "Assets/Scenes/Level1_Chunk_Isla2.unity"),
        new ChunkDefinition("Isla3", "Level1_Chunk_Isla3", "Assets/Scenes/Level1_Chunk_Isla3.unity")
    };

    [MenuItem("Rexy/Crear escenas por chunks")]
    public static void SplitLevel()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScene))
        {
            Debug.LogError("No existe la escena: " + SourceScene);
            return;
        }

        EnsureUnsavedScenesSaved();

        if (!AssetDatabase.CopyAsset(SourceScene, BackupScene))
        {
            Debug.LogError("No se pudo crear la copia de seguridad: " + BackupScene);
            return;
        }

        if (!AssetDatabase.CopyAsset(SourceScene, BootstrapScene))
        {
            Debug.LogError("No se pudo crear la escena bootstrap: " + BootstrapScene);
            return;
        }

        Scene source = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);

        List<EditorBuildSettingsScene> buildScenes =
            new List<EditorBuildSettingsScene>();

        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/MainMenu.unity"));
        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/Info.unity"));
        buildScenes.Add(CreateBuildSettingsScene(BootstrapScene));

        foreach (ChunkDefinition definition in ChunkDefinitions)
        {
            GameObject root = GameObject.Find(definition.rootName);

            if (root == null)
            {
                Debug.LogError("No se encontró el objeto raíz: " + definition.rootName);
                continue;
            }

            float centerX = root.transform.position.x;
            GameObject chunkCopy = Object.Instantiate(root);
            chunkCopy.name = definition.rootName;

            Scene chunkScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);

            NativeArray<EntityId> chunkEntityIds =
                new NativeArray<EntityId>(1, Allocator.Temp);
            chunkEntityIds[0] = chunkCopy.GetEntityId();

            SceneManager.MoveGameObjectsToScene(
                chunkEntityIds,
                chunkScene);

            chunkEntityIds.Dispose();

            EditorSceneManager.MarkSceneDirty(chunkScene);
            EditorSceneManager.SaveScene(chunkScene, definition.scenePath);

            buildScenes.Add(CreateBuildSettingsScene(definition.scenePath));

            SceneManager.UnloadSceneAsync(chunkScene);

            Object.DestroyImmediate(root);

            ConfigureChunkLoader(source, definition.sceneName, centerX);
        }

        ConfigureBootstrapPlayer(source);
        EditorSceneManager.MarkSceneDirty(source);
        EditorSceneManager.SaveScene(source);

        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.Refresh();

        Debug.Log("Escenas por chunks creadas. Original conservado en " + BackupScene);
        EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
    }

    private static EditorBuildSettingsScene CreateBuildSettingsScene(string path)
    {
        return new EditorBuildSettingsScene(path, true);
    }

    private static void ConfigureChunkLoader(Scene bootstrapScene, string sceneName, float centerX)
    {
        GameObject loaderObject = GameObject.Find("LevelChunkLoader");

        if (loaderObject == null)
        {
            loaderObject = new GameObject("LevelChunkLoader");
        }

        LevelChunkLoader loader = loaderObject.GetComponent<LevelChunkLoader>();

        if (loader == null)
            loader = loaderObject.AddComponent<LevelChunkLoader>();

        SerializedObject serializedLoader = new SerializedObject(loader);
        SerializedProperty chunks = serializedLoader.FindProperty("chunks");

        int index = chunks.arraySize;
        chunks.InsertArrayElementAtIndex(index);

        SerializedProperty chunk = chunks.GetArrayElementAtIndex(index);
        chunk.FindPropertyRelative("sceneName").stringValue = sceneName;
        chunk.FindPropertyRelative("worldCenterX").floatValue = centerX;
        chunk.FindPropertyRelative("loadRadius").floatValue = 28f;
        chunk.FindPropertyRelative("unloadRadius").floatValue = 38f;

        serializedLoader.ApplyModifiedProperties();
    }

    private static void ConfigureBootstrapPlayer(Scene bootstrapScene)
    {
        GameObject loaderObject = GameObject.Find("LevelChunkLoader");

        if (loaderObject == null)
            return;

        GameObject trex = GameObject.Find("Trex");

        if (trex == null)
        {
            Debug.LogWarning("No se encontró el objeto Trex para configurar el loader.");
            return;
        }

        SerializedObject serializedLoader =
            new SerializedObject(loaderObject.GetComponent<LevelChunkLoader>());

        SerializedProperty player = serializedLoader.FindProperty("player");
        player.objectReferenceValue = trex.transform;

        serializedLoader.ApplyModifiedProperties();
    }

    [MenuItem("Rexy/Mover tramo intermedio fuera de Isla3")]
    public static void MoveIntermediateObjects()
    {
        EnsureUnsavedScenesSaved();

        const string bootstrapPath = BootstrapScene;
        const string isla3Path = "Assets/Scenes/Level1_Chunk_Isla3.unity";
        const string intermediatePath = "Assets/Scenes/Level1_Chunk_Intermedio.unity";
        const string intermediateSceneName = "Level1_Chunk_Intermedio";

        EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
        Scene isla3Scene = EditorSceneManager.OpenScene(isla3Path, OpenSceneMode.Additive);

        string[] names =
        {
            "plataforma_2",
            "plataforma_2 (1)",
            "plataforma_3",
            "AtlasA_029_0"
        };

        List<GameObject> objectsToMove = new List<GameObject>();

        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject.scene != isla3Scene)
                continue;

            for (int i = 0; i < names.Length; i++)
            {
                if (gameObject.name == names[i])
                {
                    objectsToMove.Add(gameObject);
                    break;
                }
            }
        }

        if (objectsToMove.Count != names.Length)
        {
            Debug.LogWarning(
                "Se esperaban " + names.Length
                + " objetos, pero se encontraron "
                + objectsToMove.Count
                + ". Se moverán los encontrados.");
        }

        if (objectsToMove.Count == 0)
        {
            Debug.LogError("No se encontraron los objetos del tramo intermedio.");
            return;
        }

        Scene intermediateScene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Additive);

        GameObject intermediateRoot = new GameObject("ChunkIntermedio");
        SceneManager.MoveGameObjectToScene(intermediateRoot, intermediateScene);

        foreach (GameObject gameObject in objectsToMove)
            gameObject.transform.SetParent(null, true);

        NativeArray<EntityId> entityIds =
            new NativeArray<EntityId>(objectsToMove.Count, Allocator.Temp);

        float sumX = 0f;

        for (int i = 0; i < objectsToMove.Count; i++)
        {
            entityIds[i] = objectsToMove[i].GetEntityId();
            sumX += objectsToMove[i].transform.position.x;
        }

        SceneManager.MoveGameObjectsToScene(entityIds, intermediateScene);
        entityIds.Dispose();

        foreach (GameObject gameObject in objectsToMove)
            gameObject.transform.SetParent(intermediateRoot.transform, true);

        float intermediateCenterX = sumX / objectsToMove.Count;

        EditorSceneManager.MarkSceneDirty(intermediateScene);
        EditorSceneManager.SaveScene(intermediateScene, intermediatePath);
        EditorSceneManager.MarkSceneDirty(isla3Scene);
        EditorSceneManager.SaveScene(isla3Scene, isla3Path);

        EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);

        GameObject loaderObject = GameObject.Find("LevelChunkLoader");

        if (loaderObject != null)
        {
            LevelChunkLoader loader = loaderObject.GetComponent<LevelChunkLoader>();

            SerializedObject serializedLoader = new SerializedObject(loader);
            SerializedProperty chunks = serializedLoader.FindProperty("chunks");
            chunks.ClearArray();

            AddChunk(chunks, "Level1_Chunk_Isla1", 20.73129f);
            AddChunk(chunks, "Level1_Chunk_Isla2", 49.91051f);
            AddChunk(chunks, intermediateSceneName, intermediateCenterX);
            AddChunk(chunks, "Level1_Chunk_Isla3", 64.41553f);

            serializedLoader.ApplyModifiedProperties();
        }

        List<EditorBuildSettingsScene> buildScenes =
            new List<EditorBuildSettingsScene>();

        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/MainMenu.unity"));
        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/Info.unity"));
        buildScenes.Add(CreateBuildSettingsScene(bootstrapPath));
        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/Level1_Chunk_Isla1.unity"));
        buildScenes.Add(CreateBuildSettingsScene("Assets/Scenes/Level1_Chunk_Isla2.unity"));
        buildScenes.Add(CreateBuildSettingsScene(intermediatePath));
        buildScenes.Add(CreateBuildSettingsScene(isla3Path));

        EditorBuildSettings.scenes = buildScenes.ToArray();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByPath(bootstrapPath));
        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath(bootstrapPath));

        AssetDatabase.Refresh();

        Debug.Log(
            "Tramo intermedio creado en " + intermediatePath
            + " con centro X=" + intermediateCenterX);

        EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
    }

    private static void AddChunk(
        SerializedProperty chunks,
        string sceneName,
        float centerX)
    {
        int index = chunks.arraySize;
        chunks.InsertArrayElementAtIndex(index);

        SerializedProperty chunk = chunks.GetArrayElementAtIndex(index);
        chunk.FindPropertyRelative("sceneName").stringValue = sceneName;
        chunk.FindPropertyRelative("worldCenterX").floatValue = centerX;
        chunk.FindPropertyRelative("loadRadius").floatValue = 28f;
        chunk.FindPropertyRelative("unloadRadius").floatValue = 38f;
    }

    [MenuItem("Rexy/Limpiar bootstrap y corregir Isla3")]
    public static void CleanBootstrapAndFixIsla3()
    {
        EnsureUnsavedScenesSaved();

        const string bootstrapPath = BootstrapScene;
        Scene bootstrapScene = EditorSceneManager.OpenScene(
            bootstrapPath,
            OpenSceneMode.Single);

        string[] residualNames =
        {
            "AtlasA_029_0",
            "plataforma_2",
            "plataforma_3"
        };

        int removed = 0;

        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject.scene != bootstrapScene)
                continue;

            foreach (string residualName in residualNames)
            {
                if (gameObject.name != residualName)
                    continue;

                if (gameObject.transform.parent == null)
                {
                    Object.DestroyImmediate(gameObject);
                    removed++;
                }

                break;
            }
        }

        GameObject loaderObject = GameObject.Find("LevelChunkLoader");

        if (loaderObject == null)
        {
            Debug.LogError("No se encontró LevelChunkLoader en el bootstrap.");
            return;
        }

        LevelChunkLoader loader = loaderObject.GetComponent<LevelChunkLoader>();
        SerializedObject serializedLoader = new SerializedObject(loader);
        SerializedProperty chunks = serializedLoader.FindProperty("chunks");

        for (int i = 0; i < chunks.arraySize; i++)
        {
            SerializedProperty chunk = chunks.GetArrayElementAtIndex(i);
            string sceneName = chunk.FindPropertyRelative("sceneName").stringValue;

            if (sceneName == "Level1_Chunk_Isla3")
            {
                chunk.FindPropertyRelative("worldCenterX").floatValue = 88.41553f;
                chunk.FindPropertyRelative("worldStartX").floatValue = 64.41553f;
                chunk.FindPropertyRelative("worldEndX").floatValue = 112.41553f;
                chunk.FindPropertyRelative("useWorldRange").boolValue = true;
                chunk.FindPropertyRelative("rangeLoadMargin").floatValue = 8f;
                chunk.FindPropertyRelative("rangeUnloadMargin").floatValue = 14f;
            }
        }

        serializedLoader.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(bootstrapScene);
        EditorSceneManager.SaveScene(bootstrapScene);
        AssetDatabase.Refresh();

        Debug.Log(
            "Bootstrap limpiado: " + removed
            + " objetos residuales eliminados. Isla3 configurada por rango.");

        EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
    }

    [MenuItem("Rexy/Reubicar chunk intermedio entre Isla2 e Isla3")]
    public static void RepositionIntermediateChunk()
    {
        EnsureUnsavedScenesSaved();

        const string bootstrapPath = BootstrapScene;
        const string intermediatePath = "Assets/Scenes/Level1_Chunk_Intermedio.unity";

        Scene intermediateScene = EditorSceneManager.OpenScene(
            intermediatePath,
            OpenSceneMode.Single);

        Vector3 positionPlataforma2 = new Vector3(56f, 7.26234f, 0f);
        Vector3 positionPlataforma2B = new Vector3(61f, 7.26234f, 0f);
        Vector3 positionPlataforma3 = new Vector3(67f, 2.18234f, 0f);
        Vector3 positionAtlas = new Vector3(72f, 2.54275f, 0f);

        SetWorldPosition(intermediateScene, "plataforma_2", positionPlataforma2);
        SetWorldPosition(intermediateScene, "plataforma_2 (1)", positionPlataforma2B);
        SetWorldPosition(intermediateScene, "plataforma_3", positionPlataforma3);
        SetWorldPosition(intermediateScene, "AtlasA_029_0", positionAtlas);

        EditorSceneManager.MarkSceneDirty(intermediateScene);
        EditorSceneManager.SaveScene(intermediateScene);

        Scene bootstrapScene = EditorSceneManager.OpenScene(
            bootstrapPath,
            OpenSceneMode.Single);

        GameObject loaderObject = GameObject.Find("LevelChunkLoader");

        if (loaderObject != null)
        {
            LevelChunkLoader loader = loaderObject.GetComponent<LevelChunkLoader>();
            SerializedObject serializedLoader = new SerializedObject(loader);
            SerializedProperty chunks = serializedLoader.FindProperty("chunks");

            for (int i = 0; i < chunks.arraySize; i++)
            {
                SerializedProperty chunk = chunks.GetArrayElementAtIndex(i);

                if (chunk.FindPropertyRelative("sceneName").stringValue
                    != "Level1_Chunk_Intermedio")
                {
                    continue;
                }

                chunk.FindPropertyRelative("worldCenterX").floatValue = 65f;
                chunk.FindPropertyRelative("worldStartX").floatValue = 55f;
                chunk.FindPropertyRelative("worldEndX").floatValue = 75f;
                chunk.FindPropertyRelative("useWorldRange").boolValue = true;
                chunk.FindPropertyRelative("rangeLoadMargin").floatValue = 6f;
                chunk.FindPropertyRelative("rangeUnloadMargin").floatValue = 12f;
                break;
            }

            serializedLoader.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(bootstrapScene);
        EditorSceneManager.SaveScene(bootstrapScene);
        AssetDatabase.Refresh();

        Debug.Log("Chunk intermedio reubicado entre Isla2 e Isla3.");
        EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single);
    }

    private static void SetWorldPosition(
        Scene scene,
        string objectName,
        Vector3 worldPosition)
    {
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject.scene == scene && gameObject.name == objectName)
                gameObject.transform.position = worldPosition;
        }
    }

    private static void EnsureUnsavedScenesSaved()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            throw new System.OperationCanceledException("Operación cancelada por el usuario.");
    }

    private readonly struct ChunkDefinition
    {
        public readonly string rootName;
        public readonly string sceneName;
        public readonly string scenePath;

        public ChunkDefinition(string rootName, string sceneName, string scenePath)
        {
            this.rootName = rootName;
            this.sceneName = sceneName;
            this.scenePath = scenePath;
        }
    }
}
