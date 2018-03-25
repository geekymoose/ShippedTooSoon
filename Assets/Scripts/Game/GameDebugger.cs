using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameDebugger : MonoBehaviour {
	private GameObject[] goals;
	private GameManager gameManager = null;

    private void Start() {
        if(!Debug.isDebugBuild) {
            // Script enabled ONLY in debug build
            this.gameObject.GetComponent<GameDebugger>().enabled = false;
            return;
        }
        Debug.LogWarning("[WARNING] GameDebugger activated");
		this.gameManager =  this.GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
		this.handleInputKey();
	}

	private void handleInputKey(){
		// Game state (Win etc..)
		if(Input.GetKeyDown(KeyCode.F8)) {
			Debug.LogWarning("[DEBUG] : Force win game");
			this.winRightNow();
		}
		else if(Input.GetKeyDown(KeyCode.F7)) {
			this.gameManager.respawnPlayer();
		}

		// Game speed / Time
		else if(Input.GetKeyDown(KeyCode.F9)) {
			Debug.LogWarning("[DEBUG] : SlowDown game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().slowDownGame(0.1f);
		}
		else if(Input.GetKeyDown(KeyCode.F10)) {
			Debug.LogWarning("[DEBUG] : SpeedUp game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().speedUpGame(0.2f);
		}
		else if(Input.GetKeyDown(KeyCode.F11)) {
			Debug.LogWarning("[DEBUG] : Freeze game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().freezeGame();
		}
		else if(Input.GetKeyDown(KeyCode.F12)) {
			Debug.LogWarning("[DEBUG] : Unfreeze game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().unFreezeGame();
		}
	}

	// -------------------------------------------------------------------------
	// Debug / Cheat functions
	// -------------------------------------------------------------------------
	private void winRightNow() {
		// TODO: Has a bug and doesn't work

		// If done before, goals object may have not been created yet
		this.goals = GameObject.FindGameObjectsWithTag("Goal");
		Assert.IsNotNull(this.goals);
		Assert.IsTrue(this.goals.Length > 0, "No goal found for this map?");

		foreach(GameObject o in this.goals) {
			// Sounds like some object have the tag 'Goal' but shouldn't
			RoomGoal roger = o.GetComponent<RoomGoal>();
			if(roger!= null) {
				roger.activate();
			}
		}
	}
}
