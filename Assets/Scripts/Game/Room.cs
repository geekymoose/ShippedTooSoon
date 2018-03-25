using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	private int id = 0; // Internal use for TileMap Grid
	private bool isDone = false; // True if this room has been finished already
	public bool isActive = false; // Active when player is inside

	[Tooltip("Condition of victory for this room")]
	public VictoryCondition victoryCondition;


	// -------------------------------------------------------------------------
	// Unity methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.isDone = false;
		this.isActive = false;
		Assert.IsNotNull(this.victoryCondition, "VictoryCondition is not set");
	}

	public void Update() {
		if(this.isActive) {
			Debug.Log("Room::Update() - ID: " + this.id);
			if(this.victoryCondition.isValidated() && !this.isDone) {
				this.onRoomSuccess();
			}
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
		this.victoryCondition.initConditions();
		// TODO: Sound + init
	}

	/**
	 * Executed whenever player exit this room 
	 */
	public void onRoomExit() {
		Debug.Log("Room::onRoomExit() - ID: " + this.id);
		this.isActive = false;
		// TODO: Sound + destroye things
	}

	public void onRoomSuccess() {
		Debug.Log("Room::onRoomSuccess() - ID: " + this.id);
		this.isDone = true;
		// TODO: Open doors + sounds + success crap things etc etc
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
