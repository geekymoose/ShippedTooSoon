using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/**
 * Anything player can destroye with his weapon.
 */
public class Destructable : MonoBehaviour {
	private Animator _anim;
	private Collider2D[] _colliders;

	void Start () {
		_anim = this.GetComponent<Animator>();
		_colliders = this.GetComponents<Collider2D>();
		Assert.IsNotNull(_anim, "Missing Animator component");
		Assert.IsNotNull(_colliders, "Unable to recover the colliders");
	}

	public void destroy() {
		foreach(Collider2D coco in _colliders) {
			coco.enabled = false;
		}
		
		_anim.SetTrigger("Destroy");
		AkSoundEngine.PostEvent("fx_destroy", gameObject);
	}
}
