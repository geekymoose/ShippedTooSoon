using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Destructable : MonoBehaviour {
	private Animator anim;

	// Use this for initialization
	void Start () {
		this.anim = this.GetComponent<Animator>();
		Assert.IsNotNull(this.anim, "Missing Animator component");
	}

	public void destruct() {

	}
}
