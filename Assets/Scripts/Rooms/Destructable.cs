using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Destructable : MonoBehaviour {
	private Animator anim;
	private Collider2D[] colliders;

	// Use this for initialization
	void Start () {
		this.anim = this.GetComponent<Animator>();
		this.colliders = this.GetComponents<Collider2D>();
		Assert.IsNotNull(this.anim, "Missing Animator component");
	}

	public void destroy() {
		foreach(Collider2D coco in colliders) {
			coco.enabled = false;
		}
		this.anim.SetTrigger("Destroy");

		// TODO: Play a sounds?
	}
}
