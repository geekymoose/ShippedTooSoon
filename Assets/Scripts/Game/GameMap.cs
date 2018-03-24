using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;


public class GameMap : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Data / Variables (PUBLIC)
	// -------------------------------------------------------------------------

	[Tooltip("Number of rooms per row in the GameMap")]
	public int width = 2;

	[Tooltip("Number of tiles per room (RoomRow)")]
	public int roomWidth = 16;

	[Tooltip("Number of tiles per room (RoomColumn)")]
	public int roomHeight = 9;

	[Tooltip("All rooms. (Size must be equals to width * height)")]
	public Room[] listRooms;


	// -------------------------------------------------------------------------
	// Data / Variables (PRIVATE - INTERNAL)
	// -------------------------------------------------------------------------
	private GameObject grid = null;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		Debug.Log("GameMap::Start()");

		this.grid = this.gameObject;
		Assert.IsNotNull(this.grid, "Missing GameMap. Script may be applied on the wrong GameObject.");
		Assert.IsTrue(width > 0, "Invalid GameMap size (width)");

		Vector3 gridPos = new Vector3(0.0f, 0.0f, 0.0f);
		for(int k = 0; k < this.listRooms.Length; k++){
			this.instanciateRoomById(k);
		}
	}


	// -------------------------------------------------------------------------
	// Methods
	// -------------------------------------------------------------------------

	/**
	 * Get Room at specific position. (Grid Coordinates)
	 */
	public Room getRoomAt(int x, int y) {
		int id = this.getRoomId(x, y);
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length);
		if(id < 0 || id >= this.listRooms.Length) {
			return null;
		}
		return this.listRooms[id];
	}

	/**
	 * Get room ID by its position. (Grid Coordinates)
	 */
	public int getRoomId(int x, int y) {
		int id = x + (x * y);
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length, "Unexpected Room ID value");
		return id;
	}

	/**
	 * Instanciate the specific room
	 */
	public void instanciateRoomById(int id) {
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length, "Unexpected Room ID value");

		int xPos = (id % this.width) * 16;
		int yPos = (id / this.width) * 9;

		Vector3 pos = new Vector3(xPos, yPos, 0.0f);
		Room room = this.listRooms[id];

		Object.Instantiate(room, pos, Quaternion.identity, this.grid.transform);
	}
}
