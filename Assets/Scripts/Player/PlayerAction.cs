using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
	private CircleCollider2D attackRangeCollider = null;
    private Animator anim = null;
	private Transform attackCenter = null;

	public bool canAttack = false;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.attackRangeCollider	= this.GetComponent<CircleCollider2D>();
		this.attackCenter 			= GameObject.Find("PlayerAttackCenter").transform;
        this.anim 					= this.GetComponent<Animator>();

		Assert.IsNotNull(this.attackRangeCollider, "No range collider in player? :/");
		Assert.IsNotNull(this.attackCenter, "Player's hand must be dragged on player script");
		Assert.IsNotNull(this.anim, "Unable to get the player animator");

		this.attackRangeCollider.enabled = false; // Important. Used only for range value.
	}

	void Update () {
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) {
			this.canAttack = true; //TODO TMP DEBUG
			if(this.canAttack) {
				this.attack();
			}
			else {
				Debug.Log("You can't attack...");
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
			this.canAttack = true;
			this.anim.SetTrigger("PickupSword");
		}
    }


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	private void attack() {
		Debug.Log("ATTACK!!");
		this.anim.SetTrigger("Attack");
		
		Vector2 center = new Vector2(this.attackCenter.position.x, this.attackCenter.position.y);
		Collider2D[] cocos = Physics2D.OverlapCircleAll(center, this.attackRangeCollider.radius/2);
		
		foreach(Collider2D coco in cocos) {
			// TODO
			/*
			if(coco.CompareTag("Pickup") || coco.CompareTag("Destructable")) {
				return coco.transform;
			}
			*/
		}
	}
}
