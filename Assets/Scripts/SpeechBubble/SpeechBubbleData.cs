using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * A bubble data message.
 */
[CreateAssetMenu(fileName = "SpeechBubbleData", menuName = "SpeechBubbleData", order = 1)]
public class SpeechBubbleData : ScriptableObject {

	[Tooltip("Duration the bubble is displayed (In seconds)")]
	public float duration;

	[Tooltip("Message to display in the beautiful bubble")]
	public string message;
}
