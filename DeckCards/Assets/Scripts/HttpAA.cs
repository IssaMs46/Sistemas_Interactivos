using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class HttpAA : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField userIdInput; // Input para escribir ID del usuario
    [SerializeField] private Button loadButton;          // Botón para cargar datos
    [SerializeField] private TMP_Text usernameText;      // Texto para mostrar nombre del usuario
    [SerializeField] private RawImage[] deckImages;      // Imágenes de los personajes
    [SerializeField] private TMP_Text[] deckNames;       // Textos para nombres de personajes

    private string APIUrl = "https://my-json-server.typicode.com/IssaMs46/Sistemas_Interactivos/users/";
    private string RickAndMortyUrl = "https://rickandmortyapi.com/api/character/";

    private void Start()
    {
        loadButton.onClick.AddListener(() =>
        {
            if (int.TryParse(userIdInput.text, out int id))
            {
                StartCoroutine(GetUser(id));
            }
            else
            {
                Debug.LogWarning("El ID ingresado no es válido");
            }
        });
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

            User user = JsonUtility.FromJson<User>(json);

            // Mostrar el nombre del usuario
            if (usernameText != null)
                usernameText.text = user.username;

            // Limpiar imágenes y textos anteriores
            for (int i = 0; i < deckImages.Length; i++)
            {
                deckImages[i].texture = null;
                if (i < deckNames.Length)
                    deckNames[i].text = "";
            }

            // Obtener personajes del deck
            for (int i = 0; i < user.deck.Length && i < deckImages.Length; i++)
            {
                StartCoroutine(GetCharacter(user.deck[i], deckImages[i], deckNames[i]));
            }
        }
        else
        {
            Debug.LogError("Error: " + request.responseCode);
        }
    }

    IEnumerator GetCharacter(int characterId, RawImage targetImage, TMP_Text targetName)
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

            // Poner nombre del personaje
            if (targetName != null)
                targetName.text = character.name;

            // Poner imagen
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

[Serializable]
public class User
{
    public int id;
    public string username;
    public bool state;
    public int[] deck;
}

[Serializable]
public class Character
{
    public int id;
    public string name;
    public string species;
    public string image;
}
