using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Tooltip("Speed of the camera to move the a new location (Lerp: 0.1=slow, 3=fast)")]
	public float moveSpeed = 1.0f;

	[Tooltip("Camera target. May be manually set but is automatically updated later")]
	public Vector3 targetPosition;
	
	// Update is called once per frame
	void Update () {
		Vector3 v = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);
		this.transform.position = new Vector3(v.x, v.y, this.transform.position.z);
	}
}
