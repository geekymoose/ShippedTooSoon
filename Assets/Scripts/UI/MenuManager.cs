using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attribute
	// -------------------------------------------------------------------------

	private Animator 	anim = null;

	private GameObject 	victoryPanelUI = null;
	private GameObject 	gameOverPanelUI = null;
	private GameObject 	welcomePanelUI = null;

	private Text 		victoryScoreTextUI = null;

	private GameManager gameManager = null;

	
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
		GameObject scoreUIObject = GameObject.Find("VictoryScoreTextUI");
		Assert.IsNotNull(scoreUIObject, "Unable to find score UI");
		Assert.IsNotNull(objGameManager);

		this.victoryPanelUI 	= GameObject.Find("VictoryPanelUI");
		this.gameOverPanelUI 	= GameObject.Find("GameOverUI");
		this.welcomePanelUI 	= GameObject.Find("WelcomePanelUI");

		this.anim 				= this.GetComponent<Animator>();
		this.victoryScoreTextUI	= scoreUIObject.GetComponent<Text>();
		this.gameManager		= objGameManager.GetComponent<GameManager>();

		Assert.IsNotNull(this.anim, "Missing Animator on MenuManager Component");
		Assert.IsNotNull(this.victoryPanelUI);
		Assert.IsNotNull(this.gameOverPanelUI);
		Assert.IsNotNull(this.welcomePanelUI);
		Assert.IsNotNull(this.gameManager);

		this.anim.SetBool("Hidden", true);
	}


	// -------------------------------------------------------------------------
	// Show menu methods
	// -------------------------------------------------------------------------
	public void showWelcome() {
		this.anim.SetBool("Hidden", false);
		this.anim.SetTrigger("Welcome");
	}

	public void showGameOver() {
		this.anim.SetBool("Hidden", false);
		this.anim.SetTrigger("GameOver");
	}

	public void showVictory() {
		this.anim.SetBool("Hidden", false);
		this.anim.SetTrigger("Victory");
		float time = this.gameManager.getTimeManager().getStopwatchTime();

		int min = (int)(time / 60f);
		int sec = (int)(time % 60f);

		string timeStr = min.ToString("00") + ":" + sec.ToString("00");

		this.victoryScoreTextUI.text = timeStr;
	}

	public void hideAll() {
		this.anim.SetBool("Hidden", true);

		this.gameOverPanelUI.SetActive(false);
		this.victoryPanelUI.SetActive(false);
		this.welcomePanelUI.SetActive(false);
	}
}
