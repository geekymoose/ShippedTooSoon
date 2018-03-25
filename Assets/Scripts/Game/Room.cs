using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	private int id = 0; // Internal use for TileMap Grid
	private bool isDone = false; // True if this room has been finished already
	public bool isActive = false; // Active when player is inside

	private RoomDoor[] doors; // TheDoors
	
	[Tooltip("Condition of victory for this room")]
	public VictoryCondition victoryCondition;

	[Tooltip("When doors are closing or opening, this is the delay before it is actually done")]
	public float doorDelay = 1.0f;


	// -------------------------------------------------------------------------
	// Unity methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.isDone = false;
		this.isActive = false;

		this.doors = this.GetComponentsInChildren<RoomDoor>();
		Assert.IsNotNull(this.victoryCondition, "VictoryCondition is not set");
		
		this.openAllDoors();
	}

	public void Update() {
		if(this.isActive) {
			if(this.victoryCondition.isValidated() && !this.isDone) {
				this.onRoomSuccess(); // TODO To un-comment later
				Debug.Log("VICTORY");
			}
			else {
				Debug.Log("TRY AGAIN");
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
		Debug.Log("Room::onRoomEnter() - ID: " + this.id);
		this.isActive = true;
		if(!this.isDone) {
		// TODO: Sound + init
			this.victoryCondition.initConditions();
			Invoke("closeAllDoors", this.doorDelay);
		}

	}

	/**
	 * Executed whenever player exit this room 
	 */
	public void onRoomExit() {
		Debug.Log("Room::onRoomExit() - ID: " + this.id);
		this.isActive = false;
		// TODO: Sound + destroye things
		this.openAllDoors();
	}

	public void onRoomSuccess() {
		Debug.Log("Room::onRoomSuccess() - ID: " + this.id);
		this.isDone = true;
		// TODO: Open doors + sounds + success crap things etc etc
		Invoke("openAllDoors", this.doorDelay);
	}

	public void closeAllDoors() {
		foreach(RoomDoor dc in this.doors) {
			dc.closeDoor();
		}
	}

	public void openAllDoors() {
		foreach(RoomDoor dc in this.doors) {
			dc.openDoor();
		}
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
}
