using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text livesText;

    private int score;
    private int lives = 3;

    void Start()
    {
        UpdateHUD();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateHUD();
    }

    public void LoseLife()
    {
        lives--;
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        scoreText.text = "Puntos: " + score;
        livesText.text = "Vidas: " + lives;
    }
}