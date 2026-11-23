using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ScreenMenuController : MonoBehaviour, IPointerDownHandler
{
    public enum ScreenAction
    {
        StartGame,
        RetryGame,
        MainMenu,
        RetryOrMenu,
        QuitApplication,
        Information
    }

    [SerializeField] private ScreenAction action;
    [SerializeField] private string gameSceneName = "Level1_Bootstrap";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string informationSceneName = "Info";

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

        if (action == ScreenAction.RetryOrMenu && Input.GetKeyDown(KeyCode.Escape))
            LoadScene(mainMenuSceneName);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ExecuteAction();
    }

    public void ExecuteAction()
    {
        if (isLoading)
            return;

        switch (action)
        {
            case ScreenAction.StartGame:
            case ScreenAction.RetryGame:
                LoadScene(gameSceneName);
                break;

            case ScreenAction.MainMenu:
                LoadScene(mainMenuSceneName);
                break;

            case ScreenAction.RetryOrMenu:
                LoadScene(gameSceneName);
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
        LoadScene("Info");
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
