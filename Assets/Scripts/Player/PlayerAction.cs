using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
    private Transform currentPickedObj = null;
	private CircleCollider2D pickupCollider = null;

	public Transform playerHand = null;


	// -------------------------------------------------------------------------
	// Unity Methods
	// -------------------------------------------------------------------------
	public void Start() {
		this.pickupCollider = this.GetComponent<CircleCollider2D>();

		Assert.IsNotNull(this.playerHand, "Player's hand must be dragged on player script");
		Assert.IsNotNull(this.pickupCollider, "No pickup collider in player? :/");
	}

	void Update () {
        if (Input.GetButtonDown("Fire1")) {
			Debug.Log("PlayerMovement::PICKUP");
			this.currentPickedObj = this.getFirstPickupInRange();
			if(this.currentPickedObj != null) {
				this.currentPickedObj.transform.parent = this.playerHand;
			}
        }
        //when the player release the button drop the object
        else if (Input.GetButtonUp("Fire1")) {
			if(this.currentPickedObj != null) {
				Debug.Log("PlayerMovement::DROP");
				this.drop();
			}
        }
	}
	
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
            roger.activate();
        }
    }


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

    public void drop() {
        if(this.currentPickedObj != null) {
            Debug.Log("PlayerMovement::drop()");
            this.currentPickedObj.parent = null;
            //this.pickObj.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            //this.pickObj.gameObject.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

	private Transform getFirstPickupInRange() {
		Vector2 center = new Vector2(this.transform.position.x, this.transform.position.y);
		Collider2D[] cocos = Physics2D.OverlapCircleAll(center, this.pickupCollider.radius/2);
		
		foreach(Collider2D coco in cocos) {
			if(coco.CompareTag("Pickup") || coco.CompareTag("Destructable")) {
				return coco.transform;
			}
		}
		return null;
	}
}
