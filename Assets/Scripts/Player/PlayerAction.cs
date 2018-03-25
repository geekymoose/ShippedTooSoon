using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerAction : MonoBehaviour {
    private bool canPickup = false;
    private Transform objectToPickUp;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //the player have to be in the collider of the pickup object if he want to pick up the object
        if (Input.GetButtonDown("Fire1") && canPickup)
        {
            Debug.Log("PlayerMovement::PICKUP");
            objectToPickUp.transform.parent = transform;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = false;
        }

        //when the player release the button drop the object
        else if (Input.GetButtonUp("Fire1"))
        {
            Debug.Log("PlayerMovement::DROP");
            canPickup = false;
            this.Drop();
        }
	}
	
    //drop the object when the player release the button
    public void Drop()
    {
        if(this.objectToPickUp != null) {
            Debug.Log("PlayerMovement::Drop()");
            objectToPickUp.parent = null;
            //objectToPickUp = GameObject.FindGameObjectWithTag("Pickup").transform;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

    //if the player collide on the object that he can pickup
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Pickup")
        {
            this.objectToPickUp = other.gameObject.transform;
            canPickup = true;
        }
        else if(other.CompareTag("Goal")) {
            RoomGoal roger = other.gameObject.GetComponent<RoomGoal>();
            Assert.IsNotNull(roger, "Goal object doesn't have a RoomGoal script");
            roger.activate();
        }
    }

    //deactivate the pickup when the player exit the collider
    private void OnTriggerExit2D(Collider2D collision)
    {
        canPickup = false;
    }
}
