using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;


/**
 * GameMap is a matrix of Room.
 * All rooms have the same fixed dimension.
 * Number of rooms in each row and column is configurable.
 *
 * Each room is displayed in unity using TileMap.
 */
public class GameMap : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attributes (Unity Editor)
	// -------------------------------------------------------------------------

	[SerializeField]
	[Tooltip("Number of rooms per row in the GameMap")]
	private int width = 0;

	[SerializeField]
	[Tooltip("Number of tiles per room (RoomRow)")]
	private int roomWidth = 16;

	[SerializeField]
	[Tooltip("Number of tiles per room (RoomColumn)")]
	private int roomHeight = 9;

	[Tooltip("All rooms. (Size must be equals to width * height)")]
	public Room[] listRooms; // Public cuz used elsewhere. It's okay for now


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

		for(int k = 0; k < this.listRooms.Length; k++){
			this.instanciateRoomById(k);
		}
	}


	// -------------------------------------------------------------------------
	// Private methods / Assets
	// -------------------------------------------------------------------------

	/**
	 * Instanciate the specific room using its ID.
	 */
	private void instanciateRoomById(int id) {
		Assert.IsTrue(id >= 0 && id < this.listRooms.Length, "Unexpected Room ID value");

		float xPos = (id % this.width) * 16;
		float yPos = (id / this.width) * 9;

		// This is to make bottom left corner at 0:0
		xPos += (float)this.roomWidth / 2.0f; 
		yPos += (float)this.roomHeight / 2.0f;

		Vector3 pos = new Vector3(xPos, yPos, 0.0f);
		
		this.listRooms[id] = Object.Instantiate(this.listRooms[id], pos, Quaternion.identity, this.grid.transform);
		this.listRooms[id].setId(id);
	}

	/**
	 * Get room ID by its position in grid. (Grid Coordinates)
	 * Start at 0:0
	 */
	private int convertCellPosToID(int x, int y) {
		int id = x + (y * this.width);
		return id;
	}

	/**
	 * Convert Cell ID to Grid coordinate.
	 */
	private Vector2Int convertCellIDtoPos(int id) {
		int x = id % this.width;
		int y = id / this.width;
		return new Vector2Int(x, y);
	}

	private bool isValidCellPos(int x, int y) {
		int h = this.listRooms.Length / this.width;
		return (x >= 0 && x < this.width && y >= 0 && y < h);
	}

	private bool isValidCellID(int id) {
		return (id >= 0 && id < this.listRooms.Length);
	}


	// -------------------------------------------------------------------------
	// Getters / Setters
	// -------------------------------------------------------------------------

	/**
	 * Get the room under world position (World Coordinates)
	 * Returns null if no room under this positin.
	 */
	public Room getRoomUnderWorldPos(Vector3 worldPos) {
		Grid grid = this.GetComponent<Grid>();
		Assert.IsNotNull(grid);

		Vector3Int gridPos = grid.WorldToCell(worldPos);
		int x = gridPos.x / this.roomWidth;
		int y = gridPos.y / this.roomHeight;
		int id = this.convertCellPosToID(x, y);

		if(!this.isValidCellID(id)) {
			return null;
		}
		return this.listRooms[id];
	}

	/**
	 * Get the center position in world space of the specific room.
	 */
	public Vector3 getCellCenterWorldFromId(int id) {
		if(!this.isValidCellID(id)) {
			return new Vector3(0.0f, 0.0f, 0.0f);
		}
		Vector2Int cellPos = this.convertCellIDtoPos(id);

		float x = cellPos.x * this.roomWidth + this.roomWidth / 2;
		float y = cellPos.y * this.roomHeight + this.roomHeight / 2;
		Vector3 center = new Vector3(x, y, 0.0f);

		return center;
	}
}
