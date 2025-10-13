using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class AuthHandler : MonoBehaviour
{
    public static string Token { get; private set; }
    public static string Username { get; private set; }

    private string apiUrl = "http://localhost:1234";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Mantener entre escenas
    }

    void Start()
    {
        Debug.Log("No stored credentials. Please log in manually.");
    }

    // --- LOGIN ---
    public void Login()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;
        StartCoroutine(LoginCoroutine(username, password));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        AuthData loginData = new AuthData { username = username, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        UnityWebRequest www = new UnityWebRequest(apiUrl + "/api/auth/login", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login successful");
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
            Token = response.token;
            Username = response.usuario.username;
            Debug.Log("Token received for " + Username);
            SetUIForUserLogged();
        }
        else
        {
            Debug.LogError("Login failed: " + www.error + " | Response: " + www.downloadHandler.text);
        }
    }

    // --- REGISTER / CREAR USUARIO ---
    public void Register()
    {
        string username = GameObject.Find("InputFieldUsername2").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword2").GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Debe ingresar un usuario y contraseña");
            return;
        }

        StartCoroutine(RegisterCoroutine(username, password));
    }

    private IEnumerator RegisterCoroutine(string username, string password)
    {
        string url = apiUrl + "/api/usuarios";

        AuthData data = new AuthData { username = username, password = password };
        string jsonData = JsonUtility.ToJson(data);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Usuario creado correctamente: " + request.downloadHandler.text);
            // Loguear automáticamente al usuario tras crearlo
            StartCoroutine(LoginCoroutine(username, password));
        }
        else
        {
            Debug.LogError("Error al crear usuario: " + request.error);
            Debug.LogError("Respuesta: " + request.downloadHandler.text);
        }
    }

    // --- SCORE UPDATE ---
    public void UpdateScore(int newScore)
    {
        StartCoroutine(UpdateScoreCoroutine(newScore));
    }

    private IEnumerator UpdateScoreCoroutine(int newScore)
    {
        ScoreUpdate update = new ScoreUpdate
        {
            username = Username,
            data = new UserData { score = newScore }
        };
        string jsonData = JsonUtility.ToJson(update);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest patchRequest = new UnityWebRequest(apiUrl + "/api/usuarios", "PATCH");
        patchRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        patchRequest.downloadHandler = new DownloadHandlerBuffer();
        patchRequest.SetRequestHeader("Content-Type", "application/json");
        patchRequest.SetRequestHeader("x-token", Token);

        yield return patchRequest.SendWebRequest();

        if (patchRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score actualizado correctamente.");
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(patchRequest.downloadHandler.text);
            Debug.Log("Nuevo Score en servidor: " + response.usuario.data.score);
        }
        else
        {
            Debug.LogError("Error al actualizar score: " + patchRequest.error);
            Debug.LogError("Respuesta: " + patchRequest.downloadHandler.text);
        }
    }

    public void SetUIForUserLogged()
    {
        SceneManager.LoadScene("Jueguito");
    }
}

// --- Clases Auxiliares ---

[System.Serializable]
class AuthData
{
    public string username;
    public string password;
}

[System.Serializable]
class AuthResponse
{
    public User usuario;
    public string token;
}

[System.Serializable]
class User
{
    public string _id;
    public string username;
    public UserData data;
}

[System.Serializable]
class UserData
{
    public int score;
}

[System.Serializable]
class ScoreUpdate
{
    public string username;
    public UserData data;
}
