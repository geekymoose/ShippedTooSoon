using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DEV NOTE: Don't delete: used to recover goal elements (See Room class)

public class RoomGoal : MonoBehaviour {
	private bool isDone = false;

	// Use this for initialization
	void Start () {
		
	}

	public bool getIsDone() {
		return this.isDone;
	}
	
	public void activate() {
		this.isDone = true;
	}
}
