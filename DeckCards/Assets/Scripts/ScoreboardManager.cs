using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ScoreboardManager : MonoBehaviour
{
    public TMP_Text[] usernameTexts; // Asigna en inspector
    public TMP_Text[] scoreTexts;    // Asigna en inspector

    private string apiUrl = "https://sid-restapi.onrender.com/api/usuarios";

    void Start()
    {
        StartCoroutine(LoadScoreboard());
    }

    private IEnumerator LoadScoreboard()
    {
        UnityWebRequest www = UnityWebRequest.Get(apiUrl);
        www.SetRequestHeader("x-token", AuthHandler.Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Scoreboard retrieved: " + www.downloadHandler.text);

            // Parsear JSON
            UsersResponse response = JsonUtility.FromJson<UsersResponse>(www.downloadHandler.text);

            // Ordenar descendente por score
            List<User> sortedUsers = new List<User>(response.usuarios);
            sortedUsers.Sort((a, b) => b.data.score.CompareTo(a.data.score));

            // Mostrar en UI
            for (int i = 0; i < usernameTexts.Length; i++)
            {
                if (i < sortedUsers.Count)
                {
                    usernameTexts[i].text = sortedUsers[i].username;
                    scoreTexts[i].text = sortedUsers[i].data.score.ToString();
                }
                else
                {
                    usernameTexts[i].text = "-";
                    scoreTexts[i].text = "-";
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load scoreboard: " + www.error + " | Response: " + www.downloadHandler.text);
        }
    }
}

// --- Clases Auxiliares ---

[System.Serializable]
class UsersResponse
{
    public User[] usuarios;
}