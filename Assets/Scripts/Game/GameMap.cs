using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "NewGameMap", menuName = "GameMap", order = 1)]
public class GameMap : ScriptableObject {
	// -------------------------------------------------------------------------
	// Data / Variables
	// -------------------------------------------------------------------------

	// Number of rooms per row in the GameMap
	[Tooltip("Number of rooms per row in the GameMap")]
	public int width = 2;

	// Number of rooms per column in the GameMap
	[Tooltip("Number of rooms per column in the GameMap")]
	public int height = 2;

	[Tooltip("All rooms. (Size must be equals to width * height)")]
	public Room[] listRooms;

	[Tooltip("The TileMap Grid where to place rooms")]
	public GameObject grid;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Awake() {
		Assert.IsTrue(width > 0, "Invalid GameMap size (width)");
		Assert.IsTrue(height >= 0, "Invalid GameMap size (height)");
		Assert.IsTrue(width * height == listRooms.Length, "Invalid number of rooms, must be width * height size");
		this.init();
	}


	// -------------------------------------------------------------------------
	// Methods
	// -------------------------------------------------------------------------

	private void init() {
		Debug.Log("Initialize GameMap");
		this.grid = Instantiate(this.grid);
		for(int k = 0; k < this.listRooms.Length; k++){
			this.instanciateRoomById(k);
		}
	}

	/**
	 * Get Room at specific position.
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
	 * Get room ID by its position.
	 */
	public int getRoomId(int x, int y) {
		int id = x + (x * y);
		Assert.IsTrue( id >= 0 && id < this.listRooms.Length);
		return id;
	}

	public void instanciateRoomById(int id) {
		Vector3 roomCenterPos = new Vector3(0.0f, 0.0f, 0.0f);
		Room room = this.listRooms[id];
		Object.Instantiate(room.prefabRoom, this.grid.transform);
	}
}
