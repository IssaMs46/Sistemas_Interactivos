using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AuthHandler : MonoBehaviour
{
    public string Token { get; set; }
    public string Username { get; set; }

    private string apiUrl = "https://sid-restapi.onrender.com";

    void Start()
    {
        Debug.Log("No stored credentials. Please log in manually.");
    }

    public void Login()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;
        StartCoroutine(LoginCoroutine(username, password));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        string jsonData = JsonUtility.ToJson(new AuthData { username = username, password = password });
        string url = apiUrl + "/api/auth/login";

        UnityWebRequest www = new UnityWebRequest(url, "POST");
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

    private IEnumerator GetProfile()
    {
        Debug.Log("Fetching profile for user: " + Username);
        string url = apiUrl + "/api/usuarios/" + Username;

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Profile retrieved");
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);

            Debug.Log("Username: " + response.usuario.username);
            Debug.Log("Score: " + response.usuario.data.score);

            SetUIForUserLogged();
        }
        else
        {
            Debug.LogError("GetProfile failed: " + www.error + " | Response: " + www.downloadHandler.text);
        }
    }

    public void SetUIForUserLogged()
    {
        GameObject.Find("PanelLogin").SetActive(false);
    }
}

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
