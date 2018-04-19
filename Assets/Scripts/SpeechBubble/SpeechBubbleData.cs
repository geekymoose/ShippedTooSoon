using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu(fileName = "SpeechBubbleData", menuName = "SpeechBubbleData", order = 1)]
public class SpeechBubbleData : ScriptableObject {
	public float duration;
	public string message;
}
