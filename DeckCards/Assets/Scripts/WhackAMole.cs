using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class WhackAMole : MonoBehaviour
{
    public Button moleButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gamePanel;
    public GameObject endPanel;
    public TextMeshProUGUI finalScoreText;

    [SerializeField]public float gameDuration = 20f;
    private int score = 0;
    private float timeLeft;
    private bool gameRunning = false;

    void Start()
    {
        timeLeft = gameDuration;
        moleButton.onClick.AddListener(HitMole);
        moleButton.gameObject.SetActive(false);
        gamePanel.SetActive(true);
        endPanel.SetActive(false);
        StartCoroutine(GameLoop());
    }

    void Update()
    {
        if (gameRunning)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Tiempo: " + Mathf.Ceil(timeLeft);
            if (timeLeft <= 0)
                EndGame();
        }
    }

    void HitMole()
    {
        score++;
        scoreText.text = "Puntos: " + score;
        moleButton.gameObject.SetActive(false);
    }

    IEnumerator GameLoop()
    {
        gameRunning = true;
        scoreText.text = "Puntos: 0";
        timerText.text = "Tiempo: " + gameDuration;

        while (gameRunning)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            ShowMole();
        }
    }

    void ShowMole()
    {
        if (!gameRunning) return;
        RectTransform rt = moleButton.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(Random.Range(-200f, 200f), Random.Range(-100f, 100f));
        moleButton.gameObject.SetActive(true);
        StartCoroutine(HideMoleAfterDelay());
    }

    IEnumerator HideMoleAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        moleButton.gameObject.SetActive(false);
    }

    void EndGame()
    {
        gameRunning = false;
        moleButton.gameObject.SetActive(false);
        timerText.text = "¡Fin del juego!";
        gamePanel.SetActive(false);
        endPanel.SetActive(true);
        finalScoreText.text = "Tu puntuación: " + score;

        // Guardar score
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
            scoreManager.SaveScore(score);
    }

    // Llamado desde el botón "SCORES"
    public void GoToScoresScene()
    {
        SceneManager.LoadScene("ScoresScene");
    }

    // Llamado desde el botón "RETRY"
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
