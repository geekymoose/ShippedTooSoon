using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Manage Player Action such as Attacks and Pickup Goals.
 */
public class PlayerAction : MonoBehaviour {
	private PlayerMovement 		playerMovement 	= null;
	private bool 				canAttack 		= false;
	private Transform 			attackCenter 	= null;
	private CircleCollider2D 	attackCollider 	= null;

    private Animator 			anim 			= null;
    private Animator        	animUI 			= null;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.playerMovement 		= this.GetComponent<PlayerMovement>();
		this.attackCollider			= this.GetComponentInChildren<CircleCollider2D>();
		this.attackCenter 			= GameObject.Find("PlayerAttackCenter").transform;
        this.anim 					= this.GetComponent<Animator>();
		this.animUI 				= GameObject.Find("CanvasUI_GameStat").GetComponent<Animator>();

		Assert.IsNotNull(this.playerMovement, "Unable to get playerMovement");
		Assert.IsNotNull(this.attackCollider, "No range collider in player? :/");
		Assert.IsNotNull(this.attackCenter, "Player's hand must be dragged on player script");
		Assert.IsNotNull(this.anim, "Unable to get the player animator");
		Assert.IsNotNull(this.animUI, "Unable to get UI Animator");

		this.attackCollider.enabled = false; // Important. Used only for range value.
	}

	void Update () {
        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) {
			if(this.canAttack) {
				this.attack();
			}
			else {
				// TODO SOUND: Can't attack, sound?
			}
        }
	}
	
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
			if(roger.getIsDone() == false) {
            	roger.activate();
				this.animUI.SetTrigger("PickupGoal");
			}
			
			// TODO SOUND: GG sound
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
		
		// TODO SOUND: attack sound

		// Update position of attack center position
		float distance = Vector3.Distance(this.attackCenter.transform.position, this.transform.position);
		Vector3 dir = this.playerMovement.getDirection();
		Vector3 center = this.transform.position + (dir * distance);
		Vector2 attackCenter = new Vector2(center.x, center.y);

		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCenter, this.attackCollider.radius/2);

		foreach(Collider2D coco in colliders) {
			if(coco.gameObject.CompareTag("Destructable")) {
				Destructable dd = coco.gameObject.GetComponent<Destructable>();
				Assert.IsNotNull(dd, "Destructable object without Destructable script :/");
				dd.destroy();
			}
		}
	}

	/**
	 * Pickup a weapon. Player can now attack.
	 * Plays pickup animation.
	 */
	public void pickupWeapon() {
		this.playerMovement.FreezeMovement();
		this.anim.SetTrigger("PickupSword");
		this.canAttack = true;
		Invoke("internalUnfreeze", 0.6f);
		
		// TODO SOUND: pickup weapon sound (GG)
	}

	// This is ugly, just to unfreeze movement after pickup animation
	private void internalUnfreeze() {
		this.playerMovement.AllowMovement();
	}
}
