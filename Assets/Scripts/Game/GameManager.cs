using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/**
 * General GameManager.
 */
public class GameManager : MonoBehaviour {
	// Game
	private GameMap gameMap = null;
	private PlayerMovement player = null;
	private CameraController roomCamera = null;
	private GameTimeManager timeManager = new GameTimeManager();

	// Gameplay
	private Transform spawnPoint;
	private float stopwatchTime = 0.0f;

	// Room management
	private Room currentRoom = null;
	private Room previousRoom = null;
	private bool hasSwitchedRoom = false;

	// UI
	private Text goalCounterTextUI = null;
	private Text timeCounterTextUI = null;

	private GameObject victoryPanelUI = null;
	private Text victoryScoreTextUI = null;

	// Debug / Editor
	private GameObject gameMapCreator; // Used in editor to create GameMap.


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	void Start () {
		Debug.Log("GameManager::Start()");

		GameObject gameMapObject 		= GameObject.Find("GameMap");
		GameObject cameraObject 		= GameObject.Find("Main Camera");
		GameObject spawnObject 			= GameObject.Find("SpawnPoint");
		GameObject goalCounterObject 	= GameObject.Find("GoalCounterTextUI");
		GameObject timeCounterObject 	= GameObject.Find("TimeCounterTextUI");
		GameObject scoreUIObject 		= GameObject.Find("ScoreTextUI");
		this.victoryPanelUI 			= GameObject.Find("VictoryPanelUI");
		GameObject playerObject 		= GameObject.FindGameObjectWithTag("Player");
		this.gameMapCreator 			= GameObject.Find("GameMapCreator");

		Assert.IsNotNull(gameMapObject, "Unable to find GameMap object in scene");
		Assert.IsNotNull(cameraObject, "Unable to find Main Camera GameObject");
		Assert.IsNotNull(playerObject, "Unable to recover the Player GameObject");
		Assert.IsNotNull(spawnObject, "Unable to recover the SpawnObject GameObject");
		Assert.IsNotNull(goalCounterObject, "Unable to find GoalCounter Object");
		Assert.IsNotNull(timeCounterObject, "Unable to find TimeCounter Object");
		Assert.IsNotNull(this.victoryPanelUI, "Unable to find Victory UI");
		Assert.IsNotNull(scoreUIObject, "Unable to find score UI");

		if(this.gameMapCreator != null) {
			// gameMapCreator is just used to create the map by game designer.
			// If env is still present in editor. Must be removed first!
			// (Because the env is re-generated at runtime)
			GameObject.Destroy(this.gameMapCreator); 
		}

		this.gameMap 					= gameMapObject.GetComponent<GameMap>();
		this.player 					= playerObject.GetComponent<PlayerMovement>();
		this.roomCamera 				= cameraObject.GetComponent<CameraController>();
		this.spawnPoint 				= spawnObject.transform;
		this.goalCounterTextUI 			= goalCounterObject.GetComponent<Text>();
		this.timeCounterTextUI 			= timeCounterObject.GetComponent<Text>();
		this.victoryScoreTextUI 		= scoreUIObject.GetComponent<Text>();

		Assert.IsNotNull(this.gameMap, "Unable to recover GameMap script from GameMap Object");
		Assert.IsNotNull(this.roomCamera, "Unable to recover CameraController script");
		Assert.IsNotNull(this.player, "Unable to recover the player script");
		Assert.IsNotNull(this.goalCounterTextUI, "Unable to recover Text component from Goal Counter");
		Assert.IsNotNull(this.timeCounterTextUI, "Unable to recover Text component from Time Counter");

		// Init setup (Don't forget that)
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);
		this.currentRoom.onRoomEnter();
		this.currentRoom.setActive(true);
		this.previousRoom = this.currentRoom;

		this.timeManager.startStopwatch();

		this.victoryPanelUI.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		this.inputKeyHandler();
		this.updateTimeCounter();
		this.updateGoalCounter();
		this.updateCurrentRoom();
		this.updateCameraPosition();
		this.updateVictory();

		if(this.hasSwitchedRoom) {
			this.previousRoom.onRoomExit();
			this.currentRoom.onRoomEnter();
			this.previousRoom = this.currentRoom;
		}
	}


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	private void inputKeyHandler() {
		if(Input.GetKeyDown(KeyCode.F7)) {
			// For debug purpose, but should be kept in release as well in case of blocked.
			this.respawnPlayer();
		}
	}

	public void respawnPlayer() {
		this.player.transform.position = this.spawnPoint.position;
	}


	// -------------------------------------------------------------------------
	// Update Methods
	// -------------------------------------------------------------------------
	private void updateCurrentRoom() {
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);

		if(this.currentRoom != null) {
			this.hasSwitchedRoom = false;
			if(this.currentRoom.getId() != this.previousRoom.getId()) {
				this.hasSwitchedRoom = true;
			}
		}
		else {
			this.respawnPlayer();
		}
	}

	private void updateCameraPosition() {
		if(this.currentRoom != null) {
			Vector3 center = this.gameMap.getCellCenterWorldFromId(this.currentRoom.getId());
			this.roomCamera.targetPosition = center;
		}
	}

	private void updateVictory() {
		int remaining = this.getNbRemainingGoals();
		if(remaining == 0) {
			// JUST WON
			this.stopwatchTime = this.timeManager.getStopwatchTime();
			this.victoryScoreTextUI.text = this.timeManager.getStopwatchTime().ToString("0.0");
			this.victoryPanelUI.SetActive(true);
			this.goalCounterTextUI.enabled = false;
	 		this.timeCounterTextUI.enabled = false;
			Debug.Log("GG, you won!");
			this.timeManager.freezeGame();
		}
	}

	private void updateGoalCounter() {
		int remaining = this.getNbRemainingGoals();
		this.goalCounterTextUI.text = "Rooms left: " + remaining;
	}

	private void updateTimeCounter() {
		string timeStr = this.timeManager.getStopwatchTime().ToString("0.00");
		this.timeCounterTextUI.text = timeStr;
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------
	public GameTimeManager getTimeManager() {
		return this.timeManager;
	}

	/**
	 * Get how many goals remain.
	 */
	public int getNbRemainingGoals() {
		int remaining = this.gameMap.listRooms.Length;
		foreach(Room roro in this.gameMap.listRooms){
			if(roro.getIsDone() == true) {
				remaining--;
			}
		}
		return remaining;
	}
}
