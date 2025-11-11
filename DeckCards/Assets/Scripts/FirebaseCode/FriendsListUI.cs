using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform friendsContainer;
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private TextMeshProUGUI emptyText;

    private DatabaseReference usersRef;
    private DatabaseReference usersOnlineRef;
    private string myUserId;

    private Dictionary<string, TextMeshProUGUI> friendNameTexts = new();

    void Start()
    {
        usersRef = FirebaseDatabase.DefaultInstance.GetReference("users");
        usersOnlineRef = FirebaseDatabase.DefaultInstance.GetReference("users-online");
        myUserId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (myUserId == null)
        {
            Debug.LogWarning("FriendsListUI: No user logged in.");
            return;
        }

        LoadFriends();
        ListenForOnlineStatusChanges();
    }

    // Cargar la lista de amigos desde Firebase
    private void LoadFriends()
    {
        usersRef.Child(myUserId).Child("friends").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.IsCompleted)
            {
                Debug.LogError("Error loading friends list: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (!snapshot.Exists)
            {
                emptyText.gameObject.SetActive(true);
                return;
            }

            emptyText.gameObject.SetActive(false);

            foreach (DataSnapshot child in snapshot.Children)
            {
                string friendId = child.Key;
                string friendName = child.Value.ToString();

                // Crear item en la UI
                GameObject item = Instantiate(friendItemPrefab, friendsContainer);
                item.transform.localScale = Vector3.one;

                TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                nameText.text = friendName;
                nameText.color = Color.gray; // color base

                friendNameTexts[friendId] = nameText;
            }

            // Actualiza los colores basados en quién está online
            RefreshOnlineColors();
        });
    }

    // Escuchar conexiones y desconexiones en tiempo real
    private void ListenForOnlineStatusChanges()
    {
        usersOnlineRef.ChildAdded += (sender, args) =>
        {
            if (args.Snapshot == null || !args.Snapshot.Exists) return;

            string userId = args.Snapshot.Key;
            if (friendNameTexts.ContainsKey(userId))
            {
                friendNameTexts[userId].color = Color.green;
                Debug.Log(friendNameTexts[userId].text + " se ha conectado (color cambiado a verde).");
            }
        };

        usersOnlineRef.ChildRemoved += (sender, args) =>
        {
            if (args.Snapshot == null || !args.Snapshot.Exists) return;

            string userId = args.Snapshot.Key;
            if (friendNameTexts.ContainsKey(userId))
            {
                friendNameTexts[userId].color = Color.gray;
                Debug.Log(friendNameTexts[userId].text + " se ha desconectado (color cambiado a gris).");
            }
        };
    }

    // Revisar el estado inicial de todos los amigos
    private void RefreshOnlineColors()
    {
        usersOnlineRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.IsCompleted)
                return;

            DataSnapshot snapshot = task.Result;
            HashSet<string> onlineUsers = new HashSet<string>();

            foreach (DataSnapshot child in snapshot.Children)
                onlineUsers.Add(child.Key);

            foreach (var kvp in friendNameTexts)
            {
                kvp.Value.color = onlineUsers.Contains(kvp.Key) ? Color.green : Color.gray;
            }
        });
    }
}
