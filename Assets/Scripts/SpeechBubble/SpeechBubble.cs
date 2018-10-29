using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;


/**
 * Unity controller to display a Speech Bubble.
 * Bubble may be displayed for a given amount of time, with specific text.
 */
public class SpeechBubble : MonoBehaviour {
	// -------------------------------------------------------------------------
	// Attributes (Unity Editor)
	// -------------------------------------------------------------------------
	[SerializeField]
	[Tooltip("The speech bubble GameObject child reference")]
	private Transform _speechBubbleCanvasUI;

	[SerializeField]
	[Tooltip("Reference to the speech bubble text component")]
	private Text _speechBubbleTextUI;

	[SerializeField]
	[Tooltip("Reference to the speech bubble animator")]
	private Animator _animUI;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	void Start () {
		Assert.IsNotNull(_speechBubbleCanvasUI, "Unable to get the SpeechBubble Canvas. You probably forgot to drag it");
		Assert.IsNotNull(_speechBubbleCanvasUI, "Unable to get the SpeechBubble Text. You probably forgot to drag it");
		Assert.IsNotNull(_speechBubbleCanvasUI, "Unable to get the SpeechBubble Animator. You probably forgot to drag it");
	}


	// -------------------------------------------------------------------------
	// Gameplay Methods
	// -------------------------------------------------------------------------

	/**
	 * Show bubble on screen for certain amount of time with given message.
	 */
	public void showBubble(string message, float duration) {
		_speechBubbleTextUI.text = message;
		_animUI.ResetTrigger("Popup");
		_animUI.ResetTrigger("Close");
		_animUI.SetTrigger("Popup");
		Invoke("hiddeBubble", duration);
	}

	/**
	 * Hidde bubble.
	 */
	public void hiddeBubble() {
		_animUI.SetTrigger("Close");
	}
}
