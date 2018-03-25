using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour {
	private GameMap gameMap = null;
	private PlayerMovement player = null;
	private CameraController roomCamera = null;
	private GameTimeManager timeManager = new GameTimeManager();

	private Transform spawnPoint;

	// Room management
	private Room currentRoom = null;
	private Room previousRoom = null;
	private bool hasSwitchedRoom = false;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	void Start () {
		Debug.Log("GameManager::Start()");

		GameObject gameMapObject 	= GameObject.Find("GameMap");
		GameObject cameraObject 	= GameObject.Find("Main Camera");
		GameObject playerObject 	= GameObject.FindGameObjectWithTag("Player");
		GameObject spawnObject 		= GameObject.Find("SpawnPoint");

		Assert.IsNotNull(gameMapObject, "Unable to find GameMap object in scene");
		Assert.IsNotNull(cameraObject, "Unable to find Main Camera GameObject");
		Assert.IsNotNull(playerObject, "Unable to recover the Player GameObject");
		Assert.IsNotNull(spawnObject, "Unable to recover the SpawnObject GameObject");

		this.gameMap = gameMapObject.GetComponent<GameMap>();
		this.player = playerObject.GetComponent<PlayerMovement>();
		this.roomCamera = cameraObject.GetComponent<CameraController>();
		this.spawnPoint = spawnObject.transform;

		Assert.IsNotNull(this.gameMap, "Unable to recover GameMap script from GameMap Object");
		Assert.IsNotNull(this.roomCamera, "Unable to recover CameraController script");
		Assert.IsNotNull(this.player, "Unable to recover the player script");

		// Init setup
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);
		this.currentRoom.onRoomEnter();
		this.currentRoom.setActive(true);
		this.previousRoom = this.currentRoom;
	}
	
	// Update is called once per frame
	void Update () {
		this.inputKeyHandler();
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
		int remaining = this.gameMap.listRooms.Length;
		foreach(Room roro in this.gameMap.listRooms){
			if(roro.getIsDone() == true) {
				remaining--;
			}
		}
		if(remaining == 0) {
			// JUST WON
			this.timeManager.freezeGame();
			Debug.Log("GG, you won!");
		}
	}


	// -------------------------------------------------------------------------
	// Time Methods
	// -------------------------------------------------------------------------
	GameTimeManager GetTimeManager() {
		return this.timeManager;
	}
}
