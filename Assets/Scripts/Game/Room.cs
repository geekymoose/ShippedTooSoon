using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	private bool isDone = false; // True if this room has been finished already
	private int id = 0;


	// -------------------------------------------------------------------------
	// GamePlay methods
	// -------------------------------------------------------------------------

	/**
	 * Executed whenever player enter this room 
	 */
	public void onRoomEnter() {
		//Debug.Log("Room::onRoomEnter() - ID: " + this.id);
	}

	/**
	 * Executed whenever player exit this room 
	 */
	public void onRoomExit() {
		//Debug.Log("Room::onRoomExit() - ID: " + this.id);
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
