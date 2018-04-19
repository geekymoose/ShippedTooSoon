using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Game Debugger.
 * Gives cheatcodes / Godmode.
 * All possibles actions are listed (Variables started by 'key')
 *
 * \author	Constantin
 */
public class GameDebugger : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attributes
	// -------------------------------------------------------------------------
	private GameObject[] goals;
	private GameManager gameManager = null;
	private GameObject player = null;
	private ScoreData scoreData = null;

	// Shortcuts keys
	private KeyCode keySlowDownGame 		= KeyCode.F1;
	private KeyCode keySpeedUpGame 			= KeyCode.F2;
	private KeyCode keyFreezeGame 			= KeyCode.F3;
	private KeyCode keyUnfreezeGame 		= KeyCode.F4;

	private KeyCode	keyGiveWeapon 			= KeyCode.Alpha1;

	private KeyCode keyForceVictory 		= KeyCode.F5;
	private KeyCode keyForceGameOver 		= KeyCode.F6;
	private KeyCode keyRestartGame 			= KeyCode.F7;
	private KeyCode keyActivatedAllGoals 	= KeyCode.F8;

	private KeyCode keyDamagePlayer 		= KeyCode.F9;
	private KeyCode keyKillPlayer 			= KeyCode.F10;
	private KeyCode keyRespawnPlayer 		= KeyCode.F11;

	private KeyCode keyAddRandomScore 		= KeyCode.Alpha9;
	private KeyCode keyResetScores 			= KeyCode.Alpha0;

	private KeyCode keyGhostMode 			= KeyCode.Alpha2;


	// -------------------------------------------------------------------------
	// Attributes (Unity editor)
	// -------------------------------------------------------------------------
	[Tooltip("Amount of damage on keyDamagePlayer")]
	private float damageAmount = 5f;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
    private void Start() {
    	// Script enabled ONLY in debug build
        if(!Debug.isDebugBuild) {
        	Debug.Log("[DEBUGGER] GameDebugger is not activated");
            this.gameObject.GetComponent<GameDebugger>().enabled = false;
            return;
        }
        Debug.LogWarning("[DEBUGGER] GameDebugger is activated");

		this.gameManager =  this.GetComponent<GameManager>();
		this.player = GameObject.FindGameObjectWithTag("Player");
		this.scoreData = this.gameManager.getScoreData();

		Assert.IsNotNull(this.gameManager);
		Assert.IsNotNull(this.player);
		Assert.IsNotNull(this.scoreData);
    }
	
	// Update is called once per frame
	void Update () {
		this.handleInputKey();
	}

	private void handleInputKey(){

		// Game state (Win etc..)
		if(Input.GetKeyDown(this.keyActivatedAllGoals)) {
			Debug.LogWarning("[DEBUG]: Activate all goals");
			this.activateAllGoals();
		}
		else if(Input.GetKeyDown(this.keyRespawnPlayer)) {
			Debug.LogWarning("[DEBUG]: Respawn player");
			this.gameManager.respawnPlayer();
		}
		else if(Input.GetKeyDown(this.keyGiveWeapon)) {
			Debug.LogWarning("[DEBUG]: Give weapon");
			this.player.GetComponent<PlayerAction>().pickupWeapon();
		}
		else if(Input.GetKeyDown(this.keyForceVictory)) {
			Debug.LogWarning("[DEBUG]: Force Victory");
			this.gameManager.victory();
		}
		else if(Input.GetKeyDown(this.keyForceGameOver)) {
			Debug.LogWarning("[DEBUG]: Force GameOver");
			this.gameManager.gameOver();
		}
		else if(Input.GetKeyDown(this.keyRestartGame)) {
			Debug.LogWarning("[DEBUG]: Restart Game");
			this.gameManager.restart();
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

		// Player
		else if(Input.GetKeyDown(this.keyDamagePlayer)) {
			Debug.LogWarning("[DEBUG]: Damage player ("+damageAmount+")");
			this.player.GetComponent<PlayerHealth>().takeDammage(damageAmount);
		}
		else if(Input.GetKeyDown(this.keyKillPlayer)) {
			Debug.LogWarning("[DEBUG]: Kill player");
			this.player.GetComponent<PlayerHealth>().die();
		}
		else if(Input.GetKeyDown(this.keyGhostMode)) {
			Debug.LogWarning("[DEBUG]: Play is in Ghost mode");
			this.player.GetComponent<CapsuleCollider2D>().enabled = false;
		}

		else if(Input.GetKeyDown(this.keyAddRandomScore)) {
			int randScore = Random.Range(0, 999);
			Debug.LogWarning("[DEBUG]: Add Random score (" + randScore + ")");
			this.scoreData.addScoreEntry(randScore);
		}
		else if(Input.GetKeyDown(this.keyResetScores)) {
			Debug.LogWarning("[DEBUG]: Reset scores");
			this.scoreData.resetScores();
		}
	}


	// -------------------------------------------------------------------------
	// Debug / Cheat functions
	// -------------------------------------------------------------------------
	private void activateAllGoals() {
		// If done before, goals object may have not been created yet
		this.goals = GameObject.FindGameObjectsWithTag("Goal");
		Assert.IsNotNull(this.goals);
		Assert.IsTrue(this.goals.Length > 0, "No goal found for this map?");

		foreach(GameObject o in this.goals) {
			// Sounds like some object have the tag 'Goal' but shouldn't
			RoomGoal roger = o.GetComponent<RoomGoal>();
			Assert.IsNotNull(roger, "Goal tag without RoomGoal script!");
			if(roger!= null) {
				roger.activate();
			}
		}
	}
}
