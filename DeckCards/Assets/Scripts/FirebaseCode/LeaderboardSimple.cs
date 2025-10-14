using Firebase.Database;
using Firebase.Extensions;
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
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        leaderboardText.text = "Cargando leaderboard...";

        dbRef.Child("users").OrderByChild("score").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                leaderboardText.text = "Error al cargar leaderboard.";
                return;
            }

            if (task.IsCompleted)
            {
                List<UserScore> leaderboard = new List<UserScore>();
                foreach (DataSnapshot userSnapshot in task.Result.Children)
                {
                    string username = userSnapshot.Child("username").Value?.ToString() ?? "Sin nombre";
                    int score = 0;
                    int.TryParse(userSnapshot.Child("score").Value?.ToString(), out score);
                    leaderboard.Add(new UserScore(username, score));
                }

                leaderboard.Sort((a, b) => b.score.CompareTo(a.score));

                string displayText = "TOP 10 JUGADORES\n\n";
                for (int i = 0; i < leaderboard.Count; i++)
                    displayText += $"{i + 1}. {leaderboard[i].username} — {leaderboard[i].score}\n";

                leaderboardText.text = displayText;
            }
        });
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