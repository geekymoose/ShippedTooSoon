using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
	private PlayerMovement playerMovement;
	private CircleCollider2D attackRangeCollider = null;
    private Animator anim = null;
	private Transform attackCenter = null;
	private bool canAttack = false;

	public float attackRange = 2.0f;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.playerMovement 		= this.GetComponent<PlayerMovement>();
		this.attackRangeCollider	= this.GetComponent<CircleCollider2D>();
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
				this.attack();
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
			this.canAttack = true;
			this.anim.SetTrigger("PickupSword");
			//TODO: sound?
		}
    }


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

	private void attack() {
		this.anim.SetTrigger("Attack");

		int layer_mask = LayerMask.GetMask("Destructable");
		Vector2 pos2D = new Vector2(this.transform.position.x, this.transform.position.y);
		RaycastHit2D hit = Physics2D.Raycast(pos2D, playerMovement.getDirection(), this.attackRange, layer_mask);

		if(hit.collider != null) {
			GameObject target = hit.collider.gameObject;
			Debug.Log(target);
			if(target.CompareTag("Destructable")) {
				GameObject.Destroy(target);
			}
		}
	}
}
