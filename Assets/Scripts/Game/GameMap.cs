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

	[Tooltip("Number of tiles per room (Row)")]
	public int roomWidth = 16;

	[Tooltip("Number of tiles per room (Column)")]
	public int roomHeight = 9;

	[Tooltip("The TileMap Grid where to place rooms")]
	public GameObject grid;

	[Tooltip("All rooms. (Size must be equals to width * height)")]
	public Room[] listRooms;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Awake() {
		Assert.IsNotNull(this.grid, "Missing Grid prefabs");
		Assert.IsTrue(width > 0, "Invalid GameMap size (width)");

		this.init();
	}


	// -------------------------------------------------------------------------
	// Methods
	// -------------------------------------------------------------------------

	private void init() {
		Debug.Log("Initialize GameMap");
		Vector3 gridPos = new Vector3(0.0f, 0.0f, 0.0f);
		this.grid = Instantiate(this.grid, gridPos, Quaternion.identity);
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
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length);
		return id;
	}

	/**
	 * Instanciate the specific room
	 */
	public void instanciateRoomById(int id) {
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length);

		int xPos = (id % this.width) * 16;
		int yPos = (id / this.width) * 9;

		Vector3 pos = new Vector3(xPos, yPos, 0.0f);
		Room room = this.listRooms[id];

		Object.Instantiate(room.prefabRoom, pos, Quaternion.identity, this.grid.transform);
	}
}
