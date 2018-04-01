using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
    private Animator anim = null;
	private PlayerMovement playerMovement;
	private bool canAttack = false;
	
	private Transform attackCenter = null;
	private CircleCollider2D attackRangeCollider = null;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.playerMovement 		= this.GetComponent<PlayerMovement>();
		this.attackRangeCollider	= this.GetComponentInChildren<CircleCollider2D>();
        this.anim 					= this.GetComponent<Animator>();
		this.attackCenter 			= GameObject.Find("PlayerAttackCenter").transform;

		Assert.IsNotNull(this.playerMovement, "Unable to get playerMovement");
		Assert.IsNotNull(this.attackRangeCollider, "No range collider in player? :/");
		Assert.IsNotNull(this.attackCenter, "Player's hand must be dragged on player script");
		Assert.IsNotNull(this.anim, "Unable to get the player animator");

		this.attackRangeCollider.enabled = false; // Important. Used only for range value.
	}

	void Update () {
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) {
			if(this.canAttack) {
				this.attack();
			}
			else {
				//TODO: Can't attack, sound?
			}
        }
	}
	
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
            roger.activate();
        }
		else if(other.gameObject.name == "sword") {
			GameObject.Destroy(other.gameObject);
			this.pickupWeapon();
		}
    }


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	private void attack() {
		this.anim.SetTrigger("Attack");

		// Update position of attack center position
		float range = Vector3.Distance(this.attackCenter.transform.position, this.transform.position);
		Vector3 dir = this.playerMovement.getDirection();
		Vector3 center = this.transform.position + (dir * range);
		Vector2 attackCenter = new Vector2(center.x, center.y);

		Debug.DrawLine(this.transform.position, center, Color.red, 2.0f);
		Debug.Log(this.attackRangeCollider.radius);
		Debug.DrawLine(center, center + (Vector3.up * this.attackRangeCollider.radius), Color.yellow, 2.0f);

		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCenter, this.attackRangeCollider.radius/2);

		foreach(Collider2D coco in colliders) {
			if(coco.gameObject.CompareTag("Destructable")) {
				GameObject.Destroy(coco.gameObject);
			}
		}
		// TODO: sound?
	}

	/**
	 * Pickup a weapon. Player can now attack.
	 * Plays pickup animation.
	 */
	public void pickupWeapon() {
		this.canAttack = true;
		this.anim.SetTrigger("PickupSword");
		//TODO: sound?
	}
}
