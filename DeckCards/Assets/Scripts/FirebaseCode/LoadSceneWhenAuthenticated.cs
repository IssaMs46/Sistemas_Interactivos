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

    // Start is called before the first frame update
    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }

    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
            SceneManager.LoadScene(_sceneToLoad);
        }
    }

    void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChange;
    }
}
