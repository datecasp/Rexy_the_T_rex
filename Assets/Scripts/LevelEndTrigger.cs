using System;
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

        int levelNumber = GetLevelNumber(SceneManager.GetActiveScene().name);
        if (levelNumber > 0)
        {
            PlayerPrefs.SetInt(ScreenMenuController.CompletedLevelNumberKey, levelNumber);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(levelClearSceneName);
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
}
