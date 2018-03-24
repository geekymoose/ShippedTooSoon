using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

	// True if this room has been finished already
	private bool isDone = false;

	public int id = 0;

	public void Update() {
		
	}

	public void setId(int value) {
		this.id = value;
	}

	public int getId() {
		return this.id;
	}
}
