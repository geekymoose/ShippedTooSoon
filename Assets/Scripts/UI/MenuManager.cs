using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attribute
	// -------------------------------------------------------------------------
	private Animator 	anim 						= null;
	private GameManager gameManager 				= null;
	private Text 		victoryScoreTextUI 			= null;
	private Text 		victoryBestScoreTextUI 		= null;
	private Text 		gameOverBestScoreTextUI 	= null;
	private ScoreData 	scoreData 					= null;

	
	// -------------------------------------------------------------------------
	// Unity methods
	// -------------------------------------------------------------------------

	void Start () {
		// These lines may be awkward (And there is probably a better way)
		// If any child element was desibled in Editor, this enable at beginning.
		this.gameObject.SetActive(true);
		for(int k = 0; k < this.transform.childCount; ++k) {
			this.transform.GetChild(k).gameObject.SetActive(true);
		}

		GameObject objGameManager = GameObject.Find("GameManager");
		GameObject obj_VictoryScoreTextUI = GameObject.Find("VictoryScoreTextUI");
		GameObject obj_VictoryBestScoreTextUI = GameObject.Find("VictoryBestScoreTextUI");
		GameObject obj_GameOverVictoryBestScoreTextUI = GameObject.Find("GameOverBestScorePanelUI");
		Assert.IsNotNull(obj_VictoryScoreTextUI, "Unable to find score UI");
		Assert.IsNotNull(objGameManager);

		this.anim 						= this.GetComponent<Animator>();
		this.victoryScoreTextUI			= obj_VictoryScoreTextUI.GetComponent<Text>();
		this.victoryBestScoreTextUI 	= obj_VictoryBestScoreTextUI.GetComponent<Text>();
		this.gameOverBestScoreTextUI 	= obj_GameOverVictoryBestScoreTextUI.GetComponent<Text>();
		this.gameManager				= objGameManager.GetComponent<GameManager>();

		Assert.IsNotNull(this.anim, "Missing Animator on MenuManager Component");
		Assert.IsNotNull(this.gameManager);

		this.scoreData = this.gameManager.getScoreData();
		Assert.IsNotNull(this.scoreData, "Unable to find the ScoreData resources");

		// Desactivate all by default
		for(int k = 0; k < this.transform.childCount; ++k) {
			this.transform.GetChild(k).gameObject.SetActive(false);
		}
	}


	// -------------------------------------------------------------------------
	// Show menu methods
	// -------------------------------------------------------------------------

	public void showGameOver() {
		this.anim.SetTrigger("GameOver");
		
		this.gameOverBestScoreTextUI.text = this.scoreData.getScoreDataAsString();
	}

	public void showVictory() {
		this.anim.SetTrigger("Victory");

		float time = this.gameManager.getTimeManager().getStopwatchTime();
		string timeStr = ScoreData.formatScoreTimestamp(time);

		this.victoryScoreTextUI.text = timeStr;
		this.victoryBestScoreTextUI.text = this.scoreData.getScoreDataAsString();
	}


	// -------------------------------------------------------------------------
	// Application methods
	// -------------------------------------------------------------------------

	public void quit() {
		AkSoundEngine.PostEvent("fx_select", gameObject);
		
		Application.Quit();
	}

	public void restart() {
		AkSoundEngine.PostEvent("fx_select", gameObject);

		Scene loadedLevel = SceneManager.GetActiveScene();
     	SceneManager.LoadScene(loadedLevel.buildIndex);
	}

	public void leave() {
		AkSoundEngine.PostEvent("fx_select", gameObject);

		// Should be better with GetSceneAt etc, but was not working
		Scene loadedLevel = SceneManager.GetActiveScene();
     	SceneManager.LoadScene(loadedLevel.buildIndex - 1);
	}
}
