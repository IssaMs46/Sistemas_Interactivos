using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FriendsListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform friendsContainer;
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private TextMeshProUGUI emptyText;

    private DatabaseReference friendsRef;
    private string myUserId;
    private Dictionary<string, GameObject> activeFriends = new Dictionary<string, GameObject>();

    void Start()
    {
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("No hay usuario logeado para mostrar amigos.");
            return;
        }

        myUserId = user.UserId;
        friendsRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(myUserId).Child("friends");

        // Cargar estado inicial y escuchar cambios
        LoadFriendsList();
        friendsRef.ChildAdded += HandleFriendAdded;
        friendsRef.ChildRemoved += HandleFriendRemoved;
    }

    private void LoadFriendsList()
    {
        friendsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al cargar lista de amigos: " + task.Exception);
                return;
            }

            if (!task.IsCompleted || task.Result == null)
                return;

            DataSnapshot snapshot = task.Result;

            foreach (Transform child in friendsContainer)
                Destroy(child.gameObject);
            activeFriends.Clear();

            foreach (DataSnapshot friend in snapshot.Children)
            {
                string friendId = friend.Key;
                string friendName = friend.Value != null ? friend.Value.ToString() : "Unknown";
                CreateFriendItem(friendId, friendName);
            }

            RefreshEmptyText();
        });
    }

    private void HandleFriendAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.Snapshot == null || !args.Snapshot.Exists)
            return;

        string id = args.Snapshot.Key;
        string name = args.Snapshot.Value != null ? args.Snapshot.Value.ToString() : "Unknown";

        if (!activeFriends.ContainsKey(id))
        {
            CreateFriendItem(id, name);
            RefreshEmptyText();
            Debug.Log("Nuevo amigo agregado: " + name);
        }
    }

    private void HandleFriendRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.Snapshot == null)
            return;

        string id = args.Snapshot.Key;

        if (activeFriends.ContainsKey(id))
        {
            Destroy(activeFriends[id]);
            activeFriends.Remove(id);
            Debug.Log("Amigo eliminado de la lista: " + id);
            RefreshEmptyText();
        }
    }

    private void CreateFriendItem(string friendId, string friendName)
    {
        GameObject item = Instantiate(friendItemPrefab, friendsContainer);
        TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = friendName;

        activeFriends[friendId] = item;
    }

    private void RefreshEmptyText()
    {
        if (emptyText == null) return;
        emptyText.gameObject.SetActive(activeFriends.Count == 0);
    }

    private void OnDestroy()
    {
        if (friendsRef != null)
        {
            friendsRef.ChildAdded -= HandleFriendAdded;
            friendsRef.ChildRemoved -= HandleFriendRemoved;
        }
    }
}
