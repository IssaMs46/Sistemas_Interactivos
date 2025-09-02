using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject endGamePanel; // Asigna el PanelEndGame desde el inspector

    // Llamar esto cuando termine el juego
    public void ShowEndGamePanel()
    {
        endGamePanel.SetActive(true);
    }

    public void Retry()
    {
        // Reinicia la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLeaderboard()
    {
        SceneManager.LoadScene("Marcador"); // 👈 asegúrate de que así se llama tu escena del marcador
    }
}
