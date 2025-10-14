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
    private bool _LoadSceneWhenAuthenticated = true;

    // Start is called before the first frame update
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }

    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        bool isAuthenticated = FirebaseAuth.DefaultInstance.CurrentUser != null;
        if (_LoadSceneWhenAuthenticated)
        {
            if (isAuthenticated == _LoadSceneWhenAuthenticated)
            {
                Invoke("LoadScene", 1.5f);
            }
        }
        
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(_sceneToLoad);
    }
    void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChange;
    }
}
