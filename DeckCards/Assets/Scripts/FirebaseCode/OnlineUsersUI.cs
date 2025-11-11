using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlineUsersUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI usersListText;

    private DatabaseReference usersOnlineRef;
    private readonly Dictionary<string, string> onlineUsers = new Dictionary<string, string>();

    void Start()
    {
        usersOnlineRef = FirebaseDatabase.DefaultInstance.GetReference("users-online");

        // Listen for user connections and disconnections
        usersOnlineRef.ChildAdded += HandleUserConnected;
        usersOnlineRef.ChildRemoved += HandleUserDisconnected;

        // Load initial user list
        usersOnlineRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (DataSnapshot snapshot in task.Result.Children)
                {
                    string id = snapshot.Key;
                    string username = snapshot.Value != null ? snapshot.Value.ToString() : "Unknown";
                    if (!onlineUsers.ContainsKey(id))
                        onlineUsers.Add(id, username);
                }
                UpdateUI();
            }
        });
    }

    private void HandleUserConnected(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string id = args.Snapshot.Key;
        string username = args.Snapshot.Value != null ? args.Snapshot.Value.ToString() : "Unknown";

        if (!string.IsNullOrEmpty(username))
        {
            if (!onlineUsers.ContainsKey(id))
            {
                onlineUsers[id] = username;
                UpdateUI();
                Debug.Log(username + " connected.");
            }
        }
    }

    private void HandleUserDisconnected(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        string id = args.Snapshot.Key;

        if (onlineUsers.ContainsKey(id))
        {
            string username = onlineUsers[id];
            onlineUsers.Remove(id);
            UpdateUI();
            Debug.Log(username + " disconnected.");
        }
    }

    private void UpdateUI()
    {
        if (usersListText == null) return;

        if (onlineUsers.Count == 0)
        {
            usersListText.text = "No users online";
            return;
        }

        string display = "Users Online:\n";
        foreach (var user in onlineUsers.Values)
        {
            display += "- " + user + "\n";
        }

        usersListText.text = display;
    }

    private void OnDestroy()
    {
        if (usersOnlineRef != null)
        {
            usersOnlineRef.ChildAdded -= HandleUserConnected;
            usersOnlineRef.ChildRemoved -= HandleUserDisconnected;
        }
    }
}
