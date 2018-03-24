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
	private int currentRoomId = 0;
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
		float x = this.player.gameObject.transform.position.x;
		float y = this.player.gameObject.transform.position.y;
		this.currentRoomId = this.gameMap.getRoomId((int)x, (int)y);
		// TODO
	}

	private void updateCameraPosition() {
		//this.camera.
		// TODO
		//this.camera.targetPosition = this.transform.position;
	}


	// -------------------------------------------------------------------------
	// Time Methods
	// -------------------------------------------------------------------------
	GameTimeManager GetTimeManager() {
		return this.timeManager;
	}
}
