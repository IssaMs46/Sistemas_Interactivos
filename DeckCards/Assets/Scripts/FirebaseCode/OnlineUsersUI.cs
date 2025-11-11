using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class OnlineUsersUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI usersOnlineText;

    private DatabaseReference usersOnlineRef;
    private Dictionary<string, string> onlineUsers = new Dictionary<string, string>();

    void Start()
    {
        usersOnlineRef = FirebaseDatabase.DefaultInstance.GetReference("users-online");

        // Escuchar eventos de usuarios conectados/desconectados
        usersOnlineRef.ChildAdded += HandleUserAdded;
        usersOnlineRef.ChildRemoved += HandleUserRemoved;

        // Escuchar cambios generales (por si Firebase actualiza algo fuera de los eventos individuales)
        usersOnlineRef.ValueChanged += HandleValueChanged;

        // Cargar el estado inicial de Firebase
        RefreshFromServer();
    }

    private void HandleUserAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.Snapshot == null || !args.Snapshot.Exists) return;

        string id = args.Snapshot.Key;
        string name = args.Snapshot.Value != null ? args.Snapshot.Value.ToString() : "Unknown";

        if (!onlineUsers.ContainsKey(id))
        {
            onlineUsers[id] = name;
            Debug.Log(name + " se ha conectado (UI).");
            RefreshUI();
        }
    }

    private void HandleUserRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.Snapshot == null) return;

        string id = args.Snapshot.Key;

        if (onlineUsers.ContainsKey(id))
        {
            Debug.Log(onlineUsers[id] + " se ha desconectado (UI).");
            onlineUsers.Remove(id);
            RefreshUI();
        }
    }

    private void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        // Cuando cambia toda la lista en Firebase, recargamos el estado completo
        if (args.Snapshot == null) return;
        RefreshFromSnapshot(args.Snapshot);
    }

    private void RefreshFromServer()
    {
        usersOnlineRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener lista de usuarios online: " + task.Exception);
                return;
            }

            if (!task.IsCompleted || task.Result == null) return;

            RefreshFromSnapshot(task.Result);
        });
    }

    private void RefreshFromSnapshot(DataSnapshot snapshot)
    {
        onlineUsers.Clear();

        foreach (DataSnapshot child in snapshot.Children)
        {
            string id = child.Key;
            string name = child.Value != null ? child.Value.ToString() : "Unknown";
            onlineUsers[id] = name;
        }

        Debug.Log("Lista actualizada desde Firebase (" + onlineUsers.Count + " usuarios).");
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (usersOnlineText == null) return;

        if (onlineUsers.Count == 0)
        {
            usersOnlineText.text = "Users Online:\n(ninguno)";
            return;
        }

        string text = "Users Online:\n";
        foreach (var kv in onlineUsers)
        {
            text += "- " + kv.Value + "\n";
        }

        usersOnlineText.text = text;
    }

    private void OnDestroy()
    {
        if (usersOnlineRef != null)
        {
            usersOnlineRef.ChildAdded -= HandleUserAdded;
            usersOnlineRef.ChildRemoved -= HandleUserRemoved;
            usersOnlineRef.ValueChanged -= HandleValueChanged;
        }
    }
}
