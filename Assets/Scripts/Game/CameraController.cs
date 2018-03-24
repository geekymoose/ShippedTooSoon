using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Tooltip("Speed of the camera to move the a new location (Lerp)")]
	public float moveSpeed = 1.0f;

	private Vector3 targetPosition;

	// Use this for initialization
	void Start () {
		this.targetPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 v = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);
	}
}
