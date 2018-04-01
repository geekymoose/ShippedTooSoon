using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Game Debugger.
 * Gives cheatcodes / Godmode.
 * All possibles actions are listed (Variables started by 'key')
 */
public class GameDebugger : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Parameters
	// -------------------------------------------------------------------------
	private GameObject[] goals;
	private GameManager gameManager = null;
	private GameObject player = null;

	// Shortcuts keys
	private KeyCode	keyGiveWeapon 			= KeyCode.F6;
	private KeyCode keyRespawnPlayer 		= KeyCode.F7;
	private KeyCode keyForceWin 			= KeyCode.F8;
	private KeyCode keySlowDownGame 		= KeyCode.F9;
	private KeyCode keySpeedUpGame 			= KeyCode.F10;
	private KeyCode keyFreezeGame 			= KeyCode.F11;
	private KeyCode keyUnfreezeGame 		= KeyCode.F12;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
    private void Start() {
        if(!Debug.isDebugBuild) {
            // Script enabled ONLY in debug build
        	Debug.Log("[DEBUGGER] GameDebugger is not activated");
            this.gameObject.GetComponent<GameDebugger>().enabled = false;
            return;
        }
        Debug.LogWarning("[WARNING] GameDebugger activated");

		this.gameManager =  this.GetComponent<GameManager>();
		this.player = GameObject.FindGameObjectWithTag("Player");

		Assert.IsNotNull(this.gameManager);
		Assert.IsNotNull(this.player);
    }
	
	// Update is called once per frame
	void Update () {
		this.handleInputKey();
	}

	private void handleInputKey(){
		// Game state (Win etc..)
		if(Input.GetKeyDown(this.keyForceWin)) {
			Debug.LogWarning("[DEBUG]: Force win game");
			this.winRightNow();
		}
		else if(Input.GetKeyDown(this.keyRespawnPlayer)) {
			Debug.LogWarning("[DEBUG]: Respawn player");
			this.gameManager.respawnPlayer();
		}
		else if(Input.GetKeyDown(this.keyGiveWeapon)) {
			Debug.LogWarning("[DEBUG]: Give weapon");
			this.player.GetComponent<PlayerAction>().pickupWeapon();
		}

		// Game speed / Time
		else if(Input.GetKeyDown(this.keySlowDownGame)) {
			Debug.LogWarning("[DEBUG]: SlowDown game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().slowDownGame(0.1f);
		}
		else if(Input.GetKeyDown(this.keySpeedUpGame)) {
			Debug.LogWarning("[DEBUG]: SpeedUp game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().speedUpGame(0.2f);
		}
		else if(Input.GetKeyDown(this.keyFreezeGame)) {
			Debug.LogWarning("[DEBUG]: Freeze game. Current scale: " + Time.timeScale);
			this.gameManager.getTimeManager().freezeGame();
		}
		else if(Input.GetKeyDown(this.keyUnfreezeGame)) {
			Debug.LogWarning("[DEBUG]: Unfreeze game. Current scale: " + Time.timeScale);
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
