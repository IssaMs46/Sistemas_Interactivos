using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonRecoverPassword : MonoBehaviour
{
    [SerializeField] private Button _recoverButton;
    [SerializeField] private TMP_InputField _emailInputField;

    // (Opcional) Texto para feedback al usuario
    [SerializeField] private TMP_Text _feedbackText;

    void Reset()
    {
        _recoverButton = GetComponent<Button>();
        _emailInputField = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>();
        _feedbackText = GameObject.Find("TextFeedback")?.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        _recoverButton.onClick.AddListener(HandleRecoverButtonClicked);
    }

    private void HandleRecoverButtonClicked()
    {
        string email = _emailInputField.text;

        if (string.IsNullOrEmpty(email))
        {
            ShowMessage("Por favor ingresa tu correo.");
            return;
        }

        var auth = FirebaseAuth.DefaultInstance;
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                ShowMessage("Operación cancelada.");
                return;
            }
            if (task.IsFaulted)
            {
                ShowMessage($"Error: {task.Exception?.Message}");
                return;
            }

            ShowMessage("Se envió un correo para restablecer la contraseña.");
        });
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);
        _feedbackText.text = message;
        if (_feedbackText != null)
            _feedbackText.text = message;
    }
}