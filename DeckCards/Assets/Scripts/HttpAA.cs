using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

public class HttpAA : MonoBehaviour
{
    
    [SerializeField] private int userId = 1; // ID del usuario en tu JSON
    [SerializeField] private RawImage[] deckImages; // Asignar en el inspector las 5 RawImage

    private string APIUrl = "https://my-json-server.typicode.com/IssaMs46/Sistemas_Interactivos/users/";
    private string RickAndMortyUrl = "https://rickandmortyapi.com/api/character/";

    void Start()
    {
        StartCoroutine(GetUser(userId));
    }

    IEnumerator GetUser(int userId)
    {
        UnityWebRequest request = UnityWebRequest.Get(APIUrl + userId);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        if (request.responseCode == 200)
        {
            string json = request.downloadHandler.text;
            Debug.Log("User JSON: " + json);

            // Convertir el JSON a clase User
            User user = JsonUtility.FromJson<User>(json);

            // Obtener las imágenes de su deck
            for (int i = 0; i < user.deck.Length && i < deckImages.Length; i++)
            {
                StartCoroutine(GetCharacterImage(user.deck[i], deckImages[i]));
            }
        }
        else
        {
            Debug.LogError("Error: " + request.responseCode);
        }
    }

    IEnumerator GetCharacterImage(int characterId, RawImage targetImage)
    {
        UnityWebRequest request = UnityWebRequest.Get(RickAndMortyUrl + characterId);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        if (request.responseCode == 200)
        {
            string json = request.downloadHandler.text;
            Character character = JsonUtility.FromJson<Character>(json);
            StartCoroutine(GetImage(character.image, targetImage));
        }
        else
        {
            Debug.LogError("Error: " + request.responseCode);
        }
    }

    IEnumerator GetImage(string imageUrl, RawImage targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        if (request.responseCode == 200)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            targetImage.texture = texture;
        }
        else
        {
            Debug.LogError("Error: " + request.responseCode);
        }
    }
}

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public bool state;
    public int[] deck;
}

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public string species;
    public string image;
}
