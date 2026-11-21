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
        RetryOrMenu
    }

    [SerializeField] private ScreenAction action;
    [SerializeField] private string gameSceneName = "Level1_Bootstrap";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isLoading;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || Input.GetMouseButtonDown(0))
        {
            ExecuteAction();
        }

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
        }
    }

    private void LoadScene(string sceneName)
    {
        isLoading = true;
        SceneManager.LoadScene(sceneName);
    }
}
