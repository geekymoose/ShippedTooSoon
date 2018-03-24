using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Room", order = 1)]
public class Room : ScriptableObject {

	// True if this room has been finished already
	private bool isDone;

	[Tooltip("Prefab of the room")]
	public GameObject prefabRoom;
}
