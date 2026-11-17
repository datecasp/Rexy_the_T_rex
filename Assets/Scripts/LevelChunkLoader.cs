using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChunkLoader : MonoBehaviour
{
    [Serializable]
    public class Chunk
    {
        public string sceneName;
        public float worldCenterX;
        public float worldStartX;
        public float worldEndX;
        public bool useWorldRange;
        public float loadRadius = 28f;
        public float unloadRadius = 38f;
        public float rangeLoadMargin = 8f;
        public float rangeUnloadMargin = 14f;

        [NonSerialized]
        public Scene scene;

        [NonSerialized]
        public bool isLoading;
    }

    [Header("Jugador")]
    [SerializeField] private Transform player;

    [Header("Chunks")]
    [SerializeField] private Chunk[] chunks;

    [Header("Frecuencia")]
    [SerializeField] private float updateInterval = 0.25f;

    private IEnumerator updateRoutine;

    private void OnEnable()
    {
        updateRoutine = UpdateChunksRoutine();
        StartCoroutine(updateRoutine);
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject trex = GameObject.Find("Trex");

            if (trex != null)
                player = trex.transform;
        }
    }

    private IEnumerator UpdateChunksRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            if (player != null)
                UpdateChunks(player.position.x);

            yield return wait;
        }
    }

    private void UpdateChunks(float playerX)
    {
        foreach (Chunk chunk in chunks)
        {
            if (chunk == null || string.IsNullOrEmpty(chunk.sceneName))
                continue;

            Scene loadedScene = SceneManager.GetSceneByName(chunk.sceneName);
            chunk.scene = loadedScene;

            bool shouldLoad;
            bool shouldUnload;

            if (chunk.useWorldRange)
            {
                shouldLoad = playerX >= chunk.worldStartX - chunk.rangeLoadMargin
                    && playerX <= chunk.worldEndX + chunk.rangeLoadMargin;

                shouldUnload = playerX < chunk.worldStartX - chunk.rangeUnloadMargin
                    || playerX > chunk.worldEndX + chunk.rangeUnloadMargin;
            }
            else
            {
                float distance = Mathf.Abs(playerX - chunk.worldCenterX);
                shouldLoad = distance <= chunk.loadRadius;
                shouldUnload = distance > chunk.unloadRadius;
            }

            if (!loadedScene.IsValid()
                && shouldLoad
                && !chunk.isLoading)
            {
                StartCoroutine(LoadChunk(chunk));
            }

            if (loadedScene.IsValid()
                && loadedScene.isLoaded
                && shouldUnload
                && !chunk.isLoading)
            {
                StartCoroutine(UnloadChunk(chunk));
            }
        }
    }

    private IEnumerator LoadChunk(Chunk chunk)
    {
        chunk.isLoading = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            chunk.sceneName,
            LoadSceneMode.Additive);

        if (operation == null)
        {
            Debug.LogError("No se pudo cargar el chunk: " + chunk.sceneName);
            chunk.isLoading = false;
            yield break;
        }

        yield return operation;

        chunk.scene = SceneManager.GetSceneByName(chunk.sceneName);
        chunk.isLoading = false;
    }

    private IEnumerator UnloadChunk(Chunk chunk)
    {
        chunk.isLoading = true;

        AsyncOperation operation = SceneManager.UnloadSceneAsync(chunk.scene);

        if (operation == null)
        {
            Debug.LogError("No se pudo descargar el chunk: " + chunk.sceneName);
            chunk.isLoading = false;
            yield break;
        }

        yield return operation;

        chunk.scene = default;
        chunk.isLoading = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (chunks == null)
            return;

        foreach (Chunk chunk in chunks)
        {
            if (chunk == null)
                continue;

            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);

            if (chunk.useWorldRange)
            {
                float loadStart = chunk.worldStartX - chunk.rangeLoadMargin;
                float loadEnd = chunk.worldEndX + chunk.rangeLoadMargin;
                Vector3 loadCenter = new Vector3((loadStart + loadEnd) * 0.5f, transform.position.y, 0f);
                Gizmos.DrawWireCube(loadCenter, new Vector3(loadEnd - loadStart, 20f, 1f));

                Gizmos.color = new Color(1f, 0.65f, 0f, 0.35f);
                float unloadStart = chunk.worldStartX - chunk.rangeUnloadMargin;
                float unloadEnd = chunk.worldEndX + chunk.rangeUnloadMargin;
                Vector3 unloadCenter = new Vector3((unloadStart + unloadEnd) * 0.5f, transform.position.y, 0f);
                Gizmos.DrawWireCube(unloadCenter, new Vector3(unloadEnd - unloadStart, 20f, 1f));
            }
            else
            {
                Vector3 center = new Vector3(chunk.worldCenterX, transform.position.y, 0f);
                Gizmos.DrawWireCube(center, new Vector3(chunk.loadRadius * 2f, 20f, 1f));

                Gizmos.color = new Color(1f, 0.65f, 0f, 0.35f);
                Gizmos.DrawWireCube(center, new Vector3(chunk.unloadRadius * 2f, 20f, 1f));
            }
        }
    }
#endif
}
