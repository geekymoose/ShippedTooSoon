using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour {
	private GameMap gameMap = null;
	private PlayerMovement player = null;
	private CameraController camera = null;
	private GameTimeManager timeManager = new GameTimeManager();

	// Room management
	public Room currentRoom = null;
	private int oldRoomId = 0;
	private bool hasSwitchedRoom = false;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	void Start () {
		Debug.Log("GameManager::Start()");

		GameObject gameMapObject 	= GameObject.Find("GameMap");
		GameObject cameraObject 	= GameObject.Find("Main Camera");
		GameObject playerObject 	= GameObject.FindGameObjectWithTag("Player");

		Assert.IsNotNull(gameMapObject, "Unable to find GameMap object in scene");
		Assert.IsNotNull(cameraObject, "Unable to find Main Camera GameObject");
		Assert.IsNotNull(playerObject, "Unable to recover the Player GameObject");

		this.gameMap = gameMapObject.GetComponent<GameMap>();
		this.player = playerObject.GetComponent<PlayerMovement>();
		this.camera = cameraObject.GetComponent<CameraController>();

		Assert.IsNotNull(this.gameMap, "Unable to recover GameMap script from GameMap Object");
		Assert.IsNotNull(this.camera, "Unable to recover CameraController script");
		Assert.IsNotNull(this.player, "Unable to recover the player script");

	}
	
	// Update is called once per frame
	void Update () {
		this.updateCurrentRoom();
		this.updateCameraPosition();
		if(this.hasSwitchedRoom) {
			// TODO
		}
	}


	// -------------------------------------------------------------------------
	// Core Methods
	// -------------------------------------------------------------------------
	private void updateCurrentRoom() {
		this.currentRoom = this.gameMap.getRoomUnderWorldPos(this.player.transform.position);

		Assert.IsNotNull(this.currentRoom, "Player is not in a room (But should be)");

		if(this.currentRoom != null) {
		Debug.Log("CURRENT: " + this.currentRoom + " -- CUrrent ID: " + this.currentRoom.getId());
			this.hasSwitchedRoom = false;
			if(this.currentRoom.getId() != this.oldRoomId) {
				this.hasSwitchedRoom = true;
			}
		}

	}

	private void updateCameraPosition() {
		//this.camera.
		// TODO
		//this.camera.targetPosition = this.transform.position;

		if(this.currentRoom != null) {
			Vector3 center = this.gameMap.getCellCenterWorldFromId(this.currentRoom.getId());
			this.camera.targetPosition = center;
			/*
			Tilemap tilemap = this.currentRoom.GetComponent<Tilemap>();
			Assert.IsNotNull(tilemap, "Unable to recover tile map from a room (Probably invalid room prefab)");

			Debug.Log(this.currentRoom.transform.position);
			*/
		}
	}


	// -------------------------------------------------------------------------
	// Time Methods
	// -------------------------------------------------------------------------
	GameTimeManager GetTimeManager() {
		return this.timeManager;
	}
}
