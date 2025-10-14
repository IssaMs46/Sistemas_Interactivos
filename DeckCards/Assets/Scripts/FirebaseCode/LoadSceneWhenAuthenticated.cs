using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneWhenAuthenticated : MonoBehaviour
{
    [SerializeField]
    private string _sceneToLoad = "Jueguito";
    [SerializeField]
    private bool _whenAuthenticated = true;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }

    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        if (_whenAuthenticated)
        {
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                SceneManager.LoadScene(_sceneToLoad);
            }
        }
        else
        {
            if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            {
                SceneManager.LoadScene(_sceneToLoad);
            }
        }
        
    }

    void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChange;
    }
}
