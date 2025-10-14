using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public string sceneName; // escribe aquí el nombre de la escena que quieres cargar

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}