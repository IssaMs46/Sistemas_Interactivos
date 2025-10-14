using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System;

public class ScoreManager : MonoBehaviour
{
    private DatabaseReference dbRef;

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SaveScore(int score)
    {
        var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("❌ No hay usuario autenticado, no se puede guardar el score.");
            return;
        }

        string userId = currentUser.UserId;
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        // Guardar puntaje actual
        dbRef.Child("users").Child(userId).Child("score").SetValueAsync(score);

        // Guardar historial (opcional)
        string key = dbRef.Child("users").Child(userId).Child("scores").Push().Key;
        dbRef.Child("users").Child(userId).Child("scores").Child(key).SetRawJsonValueAsync(
            JsonUtility.ToJson(new ScoreEntry(score, timestamp))
        );

        Debug.Log($"✅ Score {score} guardado para {userId}");
    }

    [Serializable]
    public class ScoreEntry
    {
        public int value;
        public string timestamp;

        public ScoreEntry(int value, string timestamp)
        {
            this.value = value;
            this.timestamp = timestamp;
        }
    }
}