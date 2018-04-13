using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuWelcome : MonoBehaviour {

	public void Start() {
		AkSoundEngine.PostEvent("mus_menu", gameObject);
	}
	
	public void startGame() {
		AkSoundEngine.PostEvent("fx_select", gameObject);
       		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
	}

	public void quit() {
		AkSoundEngine.PostEvent("fx_select", gameObject);
		Application.Quit();
	}
}
