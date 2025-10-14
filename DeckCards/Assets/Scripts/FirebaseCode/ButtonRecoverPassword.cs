using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRecoverPassword : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _recoverButton;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _usernameInput;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        _recoverButton.onClick.AddListener(HandleRecoverClicked);
    }

    private void HandleRecoverClicked()
    {
        string email = _emailInput.text.Trim();
        string password = _passwordInput.text.Trim();
        string username = _usernameInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Debes llenar todos los campos para recuperar la contraseña.");
            return;
        }

        StartCoroutine(RecoverPassword(email, password, username));
    }

    private IEnumerator RecoverPassword(string email, string newPassword, string username)
    {
        var getUsersTask = dbRef.Child("users").GetValueAsync();
        yield return new WaitUntil(() => getUsersTask.IsCompleted);

        if (getUsersTask.Exception != null)
        {
            Debug.LogError("Error al obtener usuarios: " + getUsersTask.Exception);
            yield break;
        }

        bool userFound = false;
        string userId = "";

        foreach (DataSnapshot user in getUsersTask.Result.Children)
        {
            string dbEmail = user.Child("email").Value?.ToString();
            string dbUsername = user.Child("username").Value?.ToString();

            if (dbEmail == email && dbUsername == username)
            {
                userFound = true;
                userId = user.Key;
                break;
            }
        }

        if (!userFound)
        {
            Debug.LogWarning("No se encontró un usuario con ese email y username.");
            yield break;
        }

        // Iniciar sesión temporalmente para permitir cambio de contraseña
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, newPassword);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.Log("🔄 Creando sesión temporal para actualizar la contraseña...");
        }

        var userAuth = auth.CurrentUser;

        if (userAuth != null)
        {
            var updateTask = userAuth.UpdatePasswordAsync(newPassword);
            yield return new WaitUntil(() => updateTask.IsCompleted);

            if (updateTask.Exception != null)
            {
                Debug.LogError("Error al actualizar la contraseña: " + updateTask.Exception);
            }
            else
            {
                Debug.Log("✅ Contraseña actualizada en Firebase Auth.");
                dbRef.Child("users").Child(userId).Child("password").SetValueAsync(newPassword);
                Debug.Log("✅ Contraseña actualizada también en la base de datos.");
            }
        }
        else
        {
            Debug.LogError("No se pudo autenticar el usuario para cambiar la contraseña.");
        }
    }
}
