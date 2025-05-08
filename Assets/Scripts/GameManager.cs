using UnityEngine;
using TMPro; // Add this for using TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton pattern for easy access
    public int score = 0;
    public TextMeshProUGUI scoreText; // Assign in the inspector
    public GameObject gameOverPanel; // Assign a panel for game over
    public GameObject gameOverText;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        UpdateScoreText(); // Initial score update
    }

    void Start()
    {
        gameOverPanel.SetActive(false); // Hide game over panel initially
        UpdateScoreText();
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        gameOverText.SetActive(true);
    }
}
