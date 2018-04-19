using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchPickableController : MonoBehaviour {
	private bool _isDone = false;


	[Tooltip("Speech that player will say when pickup this glitch")]
	public SpeechBubbleData data;

	public void Start() {
		_isDone = false;
	}

	public void pickupGlitch() {
		_isDone = true;
	}
}
