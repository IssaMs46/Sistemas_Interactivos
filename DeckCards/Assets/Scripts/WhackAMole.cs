using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WhackAMole : MonoBehaviour
{
    public Button moleButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    public GameObject gamePanel;
    public GameObject endPanel;

    public TextMeshProUGUI finalScoreText; 
    public TextMeshProUGUI usernameText;

    public float gameDuration = 20f; // duración del juego en segundos
    private int score = 0;
    private float timeLeft;
    private bool gameRunning = false;

    void Start()
    {
        timeLeft = gameDuration;
        moleButton.onClick.AddListener(HitMole);
        moleButton.gameObject.SetActive(false);

        // Aseguramos el estado inicial de los paneles
        if (gamePanel != null) gamePanel.SetActive(true);
        if (endPanel != null) endPanel.SetActive(false);

        // Mostrar el nombre de usuario obtenido desde AuthHandler
        if (usernameText != null)
        {
            usernameText.text = "Jugador: " + AuthHandler.Username;
        }
        else
        {
            Debug.Log("No se encontró usernameText");
        }

        StartCoroutine(GameLoop());
    }

    void Update()
    {
        if (gameRunning)
        {
            timeLeft -= Time.deltaTime;
            timerText.text = "Tiempo: " + Mathf.Ceil(timeLeft).ToString();

            if (timeLeft <= 0)
            {
                EndGame();
            }
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
        float x = Random.Range(-200f, 200f);
        float y = Random.Range(-100f, 100f);
        rt.anchoredPosition = new Vector2(x, y);

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

        if (gamePanel != null) gamePanel.SetActive(false);
        if (endPanel != null) endPanel.SetActive(true);

        finalScoreText.text = "Tu puntuación: " + score;

        // Usar directamente el método de AuthHandler para actualizar el score
        if (!string.IsNullOrEmpty(AuthHandler.Username) && !string.IsNullOrEmpty(AuthHandler.Token))
        {
            AuthHandler instance = FindObjectOfType<AuthHandler>();
            if (instance != null)
            {
                instance.UpdateScore(score);
            }
            else
            {
                Debug.LogError("❌ No se encontró AuthHandler en la escena.");
            }
        }
        else
        {
            Debug.LogError("❌ Usuario no autenticado. No se puede enviar score.");
        }
    }
}
