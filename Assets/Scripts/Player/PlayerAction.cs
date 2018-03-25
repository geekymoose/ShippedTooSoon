using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
    private Transform pickObj = null;
	private Collider2D pickupCollider = null;

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
			if(this.pickObj != null) {
				Debug.Log("PlayerMovement::PICKUP");
				this.pickObj.transform.parent = this.playerHand;
				//this.pickObj.gameObject.GetComponent<Rigidbody2D>().simulated = false;
			}
        }

        //when the player release the button drop the object
        else if (Input.GetButtonUp("Fire1")) {
			if(this.pickObj != null) {
				Debug.Log("PlayerMovement::DROP");
				this.drop();
			}
        }
	}


	// -------------------------------------------------------------------------
	// GamePlay Methods
	// -------------------------------------------------------------------------

    //drop the object when the player release the button
    public void drop() {
        if(this.pickObj != null) {
            Debug.Log("PlayerMovement::drop()");
            this.pickObj.parent = null;
            //this.pickObj.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            //this.pickObj.gameObject.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

    //if the player collide on the object that he can pickup
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Pickup") {
            this.pickObj = other.gameObject.transform;
        }
        else if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
            roger.activate();
        }
    }

    //deactivate the pickup when the player exit the collider
    private void OnTriggerExit2D(Collider2D other) {
		if(this.pickObj != null && this.pickObj.name == other.gameObject.name) {
			this.pickObj = null;
		}
    }


}
