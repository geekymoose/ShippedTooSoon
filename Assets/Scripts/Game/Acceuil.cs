using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Acceuil : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
		
	}
	
    public void StartButton(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ExitButton()
    {
        Application.Quit();
    }
}
