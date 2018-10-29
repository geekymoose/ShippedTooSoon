using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Any game element that player may pickup.
 * Can be picked up only one time.
 */
public class Pickable : MonoBehaviour {
	private bool _isDone = false; // True if already picked up

	[Tooltip("Speech that player says when pickup this pickable")]
	public SpeechBubbleData data;

	public void Start() {
		_isDone = false;
	}

	public void pickup() {
		_isDone = true;
	}

	public bool isPickedup() {
		return _isDone;
	}
}
