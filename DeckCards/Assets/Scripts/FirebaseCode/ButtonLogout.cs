using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonLogout : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string sceneToLoad = "Trabajo4";

    public void OnPointerClick(PointerEventData eventData)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;

        if (user == null)
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        var db = FirebaseDatabase.DefaultInstance;
        var userRef = db.GetReference("users-online").Child(user.UserId);

        userRef.SetValueAsync(null).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al eliminar de users-online: " + task.Exception);
                return;
            }

            Debug.Log("Usuario removido de users-online correctamente.");
            auth.SignOut();
            SceneManager.LoadScene(sceneToLoad);
        });
    }
}