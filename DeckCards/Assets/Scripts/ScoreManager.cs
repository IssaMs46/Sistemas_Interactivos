using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Networking;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int Score { get; private set; }
    public float TimeLeft = 10f;  // ejemplo: 10 segundos por partida
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject EndPanel;

    private string apiUrl = "https://sid-restapi.onrender.com/api/usuarios/score";

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Score = 0;
        EndPanel.SetActive(false);
    }

    void Update()
    {
        if (TimeLeft > 0)
        {
            TimeLeft -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.CeilToInt(TimeLeft);
        }
        else
        {
            EndGame();
        }
    }

    public void AddScore(int points)
    {
        Score += points;
        scoreText.text = "Score: " + Score;
    }

    void EndGame()
    {
        TimeLeft = 0;
        EndPanel.SetActive(true);
        StartCoroutine(SendScoreToAPI());
    }

    public void RetryGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GoToScoreboard()
    {
        SceneManager.LoadScene("ScoreboardScene");
    }

    private IEnumerator SendScoreToAPI()
    {
        UserScore data = new UserScore { username = AuthHandler.Username, score = Score };
        string jsonData = JsonUtility.ToJson(data);

        UnityWebRequest www = UnityWebRequest.Put(apiUrl, jsonData);
        www.method = "PUT";
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-token", AuthHandler.Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Score enviado: " + Score);
        }
        else
        {
            Debug.LogError("❌ Error al enviar score: " + www.error);
        }
    }
}

[System.Serializable]
class UserScore
{
    public string username;
    public int score;
}
