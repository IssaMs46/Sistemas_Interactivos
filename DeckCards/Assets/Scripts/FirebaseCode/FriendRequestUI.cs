using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FriendRequestUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField friendUsernameInput;
    [SerializeField] private Button sendRequestButton;
    [SerializeField] private Transform requestsContainer;
    [SerializeField] private GameObject requestItemPrefab;

    private FriendRequestManager friendManager;
    private Dictionary<string, GameObject> activeRequests = new Dictionary<string, GameObject>();

    void Start()
    {
        friendManager = FindObjectOfType<FriendRequestManager>();

        if (sendRequestButton != null)
            sendRequestButton.onClick.AddListener(OnSendFriendRequestClicked);

        FriendRequestManager.OnFriendRequestReceived += CreateRequestItem;
        FriendRequestManager.OnFriendRequestRemoved += RemoveRequestItem;
    }

    void OnDestroy()
    {
        FriendRequestManager.OnFriendRequestReceived -= CreateRequestItem;
        FriendRequestManager.OnFriendRequestRemoved -= RemoveRequestItem;
    }

    private void OnSendFriendRequestClicked()
    {
        string friendUsername = friendUsernameInput.text.Trim();
        if (string.IsNullOrEmpty(friendUsername))
        {
            Debug.LogWarning("Enter a username to send a friend request.");
            return;
        }

        FindFriendByUsername(friendUsername);
    }

    private async void FindFriendByUsername(string friendUsername)
    {
        var dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");
        var getUsersTask = dbRef.GetValueAsync();
        await getUsersTask;

        if (getUsersTask.Exception != null)
        {
            Debug.LogError("Error finding friend: " + getUsersTask.Exception);
            return;
        }

        DataSnapshot snapshot = getUsersTask.Result;
        foreach (DataSnapshot user in snapshot.Children)
        {
            string username = user.Child("username").Value?.ToString();
            if (username == friendUsername)
            {
                string friendId = user.Key;
                friendManager.SendFriendRequest(friendId, friendUsername);
                Debug.Log("Friend request sent to " + friendUsername);
                return;
            }
        }

        Debug.LogWarning("No user found with that username.");
    }

   private void CreateRequestItem(string friendId, string friendName)
{
    if (requestItemPrefab == null || requestsContainer == null)
    {
        Debug.LogError("FriendRequestUI: missing prefab or container.");
        return;
    }

    Debug.Log("Creating request item for " + friendName + " (" + friendId + ")");

    GameObject newItem = Instantiate(requestItemPrefab, requestsContainer);

    // Buscamos todos los componentes necesarios dentro del prefab
    TextMeshProUGUI nameText = null;
    Button acceptButton = null;
    Button rejectButton = null;

    // Esto busca en todos los hijos, sin depender de rutas exactas
    TextMeshProUGUI[] texts = newItem.GetComponentsInChildren<TextMeshProUGUI>(true);
    foreach (var t in texts)
    {
        if (t.gameObject.name == "FriendName")
        {
            nameText = t;
            break;
        }
    }

    Button[] buttons = newItem.GetComponentsInChildren<Button>(true);
    foreach (var b in buttons)
    {
        if (b.gameObject.name == "ButtonAccept")
            acceptButton = b;
        else if (b.gameObject.name == "ButtonReject")
            rejectButton = b;
    }

    if (nameText == null)
    {
        Debug.LogError("FriendRequestUI: FriendName text not found in prefab.");
        return;
    }
    if (acceptButton == null)
    {
        Debug.LogError("FriendRequestUI: ButtonAccept not found in prefab.");
        return;
    }
    if (rejectButton == null)
    {
        Debug.LogError("FriendRequestUI: ButtonReject not found in prefab.");
        return;
    }

    nameText.text = friendName;

    // Quitamos posibles listeners anteriores del prefab
    acceptButton.onClick.RemoveAllListeners();
    rejectButton.onClick.RemoveAllListeners();

    // Asignamos los eventos con logs para comprobar que se llaman
    acceptButton.onClick.AddListener(() =>
    {
        Debug.Log("Accept clicked for " + friendName + " (" + friendId + ")");
        if (friendManager != null)
            friendManager.RespondFriendRequest(friendId, friendName, 1);
        RemoveRequestItem(friendId);
    });

    rejectButton.onClick.AddListener(() =>
    {
        Debug.Log("Reject clicked for " + friendName + " (" + friendId + ")");
        if (friendManager != null)
            friendManager.RespondFriendRequest(friendId, friendName, 2);
        RemoveRequestItem(friendId);
    });

    // Guardamos referencia para poder borrar luego
    if (!activeRequests.ContainsKey(friendId))
        activeRequests.Add(friendId, newItem);
}


    private void RemoveRequestItem(string friendId)
    {
        if (activeRequests.ContainsKey(friendId))
        {
            Destroy(activeRequests[friendId]);
            activeRequests.Remove(friendId);
            Debug.Log("Removed friend request from UI: " + friendId);
        }
    }
}
