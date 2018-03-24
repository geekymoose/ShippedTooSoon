using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour {
	private bool isDone = false; // True if this room has been finished already
	private int id = 0;


	// -------------------------------------------------------------------------
	// Unity methods
	// -------------------------------------------------------------------------

	public void Start() {
	}


	// -------------------------------------------------------------------------
	// Methods
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
