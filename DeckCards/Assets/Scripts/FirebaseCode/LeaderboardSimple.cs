using Firebase.Database;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardSimple : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;
    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");
        dbRef.OrderByChild("score").ValueChanged += HandleValueChanged;
        leaderboardText.text = "Esperando datos...";
    }

    private void HandleValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            leaderboardText.text = "Error al cargar leaderboard.";
            Debug.LogError("Firebase error: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot == null || e.Snapshot.ChildrenCount == 0)
        {
            leaderboardText.text = "Sin datos aún.";
            return;
        }

        List<UserScore> leaderboard = new List<UserScore>();

        foreach (DataSnapshot userSnapshot in e.Snapshot.Children)
        {
            string username = userSnapshot.Child("username").Value?.ToString() ?? "Sin nombre";
            int score = 0;
            int.TryParse(userSnapshot.Child("score").Value?.ToString(), out score);
            leaderboard.Add(new UserScore(username, score));
        }

        leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

        string displayText = "TOP 10 JUGADORES\n\n";
        int limit = Mathf.Min(10, leaderboard.Count);
        for (int i = 0; i < limit; i++)
            displayText += $"{i + 1}. {leaderboard[i].username} — {leaderboard[i].score}\n";

        leaderboardText.text = displayText;
    }

    void OnDestroy()
    {
        if (dbRef != null)
            dbRef.ValueChanged -= HandleValueChanged;
    }

    [Serializable]
    public class UserScore
    {
        public string username;
        public int score;
        public UserScore(string username, int score)
        {
            this.username = username;
            this.score = score;
        }
    }
}
