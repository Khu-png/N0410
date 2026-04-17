using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Player player;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private ScoreText scoreText;

    private const string HighScoreKey = "HighScore";

    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (poolManager == null)
        {
            poolManager = FindFirstObjectByType<PoolManager>();
        }

        if (scoreText == null)
        {
            scoreText = FindFirstObjectByType<ScoreText>();
        }
    }

    private void Start()
    {
        Score = 0;
        IsGameOver = false;
        Time.timeScale = 0f;
        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (player != null)
        {
            player.SetControlState(false);
            player.ResetToCenter();
        }

        if (poolManager != null)
        {
            poolManager.SetSpawnState(false);
        }

        scoreText?.UpdateScore(Score);
        scoreText?.UpdateHighScore(HighScore);
    }

    public void StartGame()
    {
        Score = 0;
        IsGameOver = false;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (player != null)
        {
            player.SetControlState(true);
            player.ResetToCenter();
        }

        if (poolManager != null)
        {
            poolManager.SetSpawnState(true);
        }

        scoreText?.UpdateScore(Score);
        scoreText?.UpdateHighScore(HighScore);
        Time.timeScale = 1f;
    }

    public void AddScore(int amount)
    {
        if (IsGameOver)
        {
            return;
        }

        Score += amount;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        scoreText?.UpdateScore(Score);
        scoreText?.UpdateHighScore(HighScore);
    }

    public void GameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;

        if (player != null)
        {
            player.SetControlState(false);
        }

        if (poolManager != null)
        {
            poolManager.SetSpawnState(false);
            poolManager.ReturnAllToPool();
        }

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        scoreText?.UpdateScore(Score);
        scoreText?.UpdateHighScore(HighScore);
        Time.timeScale = 1f;
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
