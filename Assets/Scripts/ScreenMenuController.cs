using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenMenuController : MonoBehaviour
{
    public enum ScreenAction
    {
        StartGame,
        RetryGame,
        NextLevel,
        MainMenu,
        QuitApplication,
        Information
    }

    [SerializeField] private ScreenAction action;
    private string gameSceneName = "Level1_Bootstrap";
    private string mainMenuSceneName = "MainMenu";
    private string nextLevelSceneName = "NextLevel";
    private string informationSceneName = "Info";

    public const string CompletedLevelNumberKey = "Rexy2D.CompletedLevelNumber";

    private bool isLoading;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteAction();
        }

        if (action == ScreenAction.Information && Input.GetKeyDown(KeyCode.Escape))
            LoadScene(mainMenuSceneName);

        if (action == ScreenAction.NextLevel && Input.GetKeyDown(KeyCode.Escape))
            LoadScene(mainMenuSceneName);
    }

    public void ExecuteAction()
    {
        if (isLoading)
            return;

        switch (action)
        {
            case ScreenAction.StartGame:
                PlayerPrefs.SetInt(CompletedLevelNumberKey, 0);
                PlayerPrefs.Save();
                LoadScene(gameSceneName);
                break;

            case ScreenAction.RetryGame:
                LoadScene(gameSceneName);
                break;

            case ScreenAction.MainMenu:
                LoadScene(mainMenuSceneName);
                break;

            case ScreenAction.NextLevel:
                LoadScene(GetNextLevel());
                break;

            case ScreenAction.QuitApplication:
                CloseApplication();
                break;

            case ScreenAction.Information:
                LoadScene(informationSceneName);
                break;
        }
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

    private string GetNextLevel()
    {
        int currentLevelNumber = PlayerPrefs.GetInt(CompletedLevelNumberKey, 0);

        if (currentLevelNumber == 0)
            currentLevelNumber = GetLevelNumber(SceneManager.GetActiveScene().name);

        int nextLevelNumber = currentLevelNumber + 1;
        nextLevelSceneName = $"Level{nextLevelNumber}_Bootstrap";

        if (IsSceneInBuildSettings(nextLevelSceneName))
            return nextLevelSceneName;

        Debug.LogWarning("No more levels available. Returning to main menu.");
        return mainMenuSceneName;
    }

    private static int GetLevelNumber(string sceneName)
    {
        int levelIndex = sceneName.IndexOf("Level", StringComparison.OrdinalIgnoreCase);
        if (levelIndex < 0)
            return 0;

        int numberStart = levelIndex + "Level".Length;
        int numberEnd = numberStart;

        while (numberEnd < sceneName.Length && char.IsDigit(sceneName[numberEnd]))
            numberEnd++;

        if (numberEnd == numberStart)
            return 0;

        int.TryParse(sceneName.Substring(numberStart, numberEnd - numberStart), out int levelNumber);
        return levelNumber;
    }

    private static bool IsSceneInBuildSettings(string sceneName)
    {
        string sceneFileName = $"/{sceneName}.unity";

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i).Replace('\\', '/');
            if (scenePath.EndsWith(sceneFileName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public void QuitApplication()
    {
        CloseApplication();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        LoadScene(mainMenuSceneName);
    }

    public void LoadInformation()
    {
        Time.timeScale = 1f;
        LoadScene(informationSceneName);
    }

    public void LoadGame()
    {
        Time.timeScale = 1f;
        LoadScene(gameSceneName);
    }

    private void LoadScene(string sceneName)
    {
        isLoading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
