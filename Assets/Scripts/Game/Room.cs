using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	private int id = 0; // Internal use for TileMap Grid
	private bool isDone = false; // True if this room has been finished already
	private bool isActive = false; // Active when player is inside

	private RoomDoor[] doors; // TheDoors
	private RoomGoal[] goals;

	private float doorDelay = 1.0f;


	// -------------------------------------------------------------------------
	// Unity methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.isDone = false;

		this.doors = this.GetComponentsInChildren<RoomDoor>();
		this.goals = this.GetComponentsInChildren<RoomGoal>();

		Debug.Log(this.goals.Length);
		
		Assert.IsTrue(this.doors.Length > 0, "There is a room without doors?");
		Assert.IsTrue(this.goals.Length > 0, "There is a room without goal?");
		
		this.openAllDoors();
	}

	public void Update() {
		if(this.isActive && !this.isDone) {
			if(this.isGoalDone()) {
				this.onRoomSuccess();
			}
		}
		else {
			this.openAllDoors();
		}
	}


	// -------------------------------------------------------------------------
	// GamePlay methods
	// -------------------------------------------------------------------------

	/**
	 * Executed whenever player enter this room 
	 */
	public void onRoomEnter() {
		//Debug.Log("Room::onRoomEnter() - ID: " + this.id);
		this.isActive = true;
		if(!this.isDone){
			Invoke("closeAllDoors", this.doorDelay);
		}
	}

	/**
	 * Executed whenever player exit this room 
	 */
	public void onRoomExit() {
		//Debug.Log("Room::onRoomExit() - ID: " + this.id);
		this.isActive = false;
		this.openAllDoors();
		// TODO: Sound + destroye things
	}

	public void onRoomSuccess() {
		//Debug.Log("Room::onRoomSuccess() - ID: " + this.id);
		this.isDone = true;
		Invoke("openAllDoors", this.doorDelay);
		// TODO: Open doors + sounds + success crap things etc etc
	}

	private void closeAllDoors() {
		foreach(RoomDoor dc in this.doors) {
			dc.closeDoor();
		}
	}

	private void openAllDoors() {
		foreach(RoomDoor dc in this.doors) {
			dc.openDoor();
		}
	}

	private bool isGoalDone() {
		for(int k = 0; k < this.goals.Length; ++k) {
			if(this.goals[k] != null) {
				return false;
			}
		}
		return true;
	}


	// -------------------------------------------------------------------------
	// Getter / Setters
	// -------------------------------------------------------------------------
	public void setId(int value) {
		this.id = value;
	}

	public int getId() {
		return this.id;
	}

	public Tilemap getTilemap() {
		Assert.IsNotNull(this.GetComponent<Tilemap>(), "Unable to recover the TileMap component");
		return this.GetComponent<Tilemap>();
	}

	public void setActive(bool value) {
		this.isActive = value;
	}

	public bool getIsDone() {
		return this.isDone;
	}
}
