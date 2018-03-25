using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	private GameMap gameMap = null;
	private PlayerMovement player = null;
	private CameraController roomCamera = null;
	private GameTimeManager timeManager = new GameTimeManager();

	private Text goalCounterTextUI = null;
	private Text timeCounterTextUI = null;

	private Transform spawnPoint;

	// Room management
	private Room currentRoom = null;
	private Room previousRoom = null;
	private bool hasSwitchedRoom = false;

	private float stopwatchTime = 0.0f;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	void Start () {
		Debug.Log("GameManager::Start()");

		GameObject gameMapObject 	= GameObject.Find("GameMap");
		GameObject cameraObject 	= GameObject.Find("Main Camera");
		GameObject playerObject 	= GameObject.FindGameObjectWithTag("Player");
		GameObject spawnObject 		= GameObject.Find("SpawnPoint");
		GameObject goalCounterObject = GameObject.Find("Goal Counter TextUI");
		GameObject timeCounterObject = GameObject.Find("Time Counter TextUI");

		Assert.IsNotNull(gameMapObject, "Unable to find GameMap object in scene");
		Assert.IsNotNull(cameraObject, "Unable to find Main Camera GameObject");
		Assert.IsNotNull(playerObject, "Unable to recover the Player GameObject");
		Assert.IsNotNull(spawnObject, "Unable to recover the SpawnObject GameObject");
		Assert.IsNotNull(goalCounterObject, "Unable to find GoalCounter Object");
		Assert.IsNotNull(timeCounterObject, "Unable to find TimeCounter Object");

		this.gameMap = gameMapObject.GetComponent<GameMap>();
		this.player = playerObject.GetComponent<PlayerMovement>();
		this.roomCamera = cameraObject.GetComponent<CameraController>();
		this.spawnPoint = spawnObject.transform;
		this.goalCounterTextUI = goalCounterObject.GetComponent<Text>();
		this.timeCounterTextUI = timeCounterObject.GetComponent<Text>();

		Assert.IsNotNull(this.gameMap, "Unable to recover GameMap script from GameMap Object");
		Assert.IsNotNull(this.roomCamera, "Unable to recover CameraController script");
		Assert.IsNotNull(this.player, "Unable to recover the player script");
		Assert.IsNotNull(this.goalCounterTextUI, "Unable to recover Text component from Goal Counter");
		Assert.IsNotNull(this.timeCounterTextUI, "Unable to recover Text component from Time Counter");

		// Init setup
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);
		this.currentRoom.onRoomEnter();
		this.currentRoom.setActive(true);
		this.previousRoom = this.currentRoom;

		this.timeManager.startStopwatch();
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

	private void inputKeyHandler() {
		if(Input.GetButtonDown("Jump")) {
			this.player.transform.position = this.spawnPoint.position;
		}
	}


	// -------------------------------------------------------------------------
	// Core Methods
	// -------------------------------------------------------------------------
	private void updateCurrentRoom() {
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);
		Assert.IsNotNull(this.currentRoom, "Player is not in a room (But should be)");

		if(this.currentRoom != null) {
			this.hasSwitchedRoom = false;
			if(this.currentRoom.getId() != this.previousRoom.getId()) {
				this.hasSwitchedRoom = true;
			}
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
			this.timeManager.freezeGame();
			Debug.Log("GG, you won!");
		}
	}

	private void updateGoalCounter() {
		int remaining = this.getNbRemainingGoals();
		this.goalCounterTextUI.text = "Rooms left: " + remaining;
	}

	private void updateTimeCounter() {
		string timeStr = this.timeManager.getStopwatchTime().ToString("0.00");
		this.timeCounterTextUI.text = "Time: " + timeStr;
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------
	public GameTimeManager getTimeManager() {
		return this.timeManager;
	}

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
