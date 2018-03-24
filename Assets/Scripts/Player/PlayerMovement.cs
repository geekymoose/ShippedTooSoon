using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {


    private Vector2 movementVector;
    
    [Range(0,100)]
    public float speed = 5f;
    public float distanceToPickup = 1f;
    public float throwForce = 1f;

    Rigidbody2D body2d;
    RaycastHit2D hit;
    private bool isGrab = false;
    public Transform pickupObject;
    public LayerMask no;

	// Use this for initialization
	void Start () {
        body2d = GetComponent<Rigidbody2D>();
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("A"))
        {
            if (!isGrab)
            {
                Physics2D.queriesStartInColliders = false;
                hit = Physics2D.Raycast(transform.position, movementVector, distanceToPickup);
                if(hit.collider!=null && hit.collider.tag == "Pickup")
                {
                    isGrab = true;
                }
                
            }
            else if (!Physics2D.OverlapPoint(pickupObject.position, no))
            {
                isGrab = false;

                if (hit.collider.gameObject.GetComponent<Rigidbody2D>() != null)
                {
                    //hit.collider.gameObject.GetComponent<Rigidbody2D>(). = new Vector2(transform.localScale.x, 1) * throwForce;
                }
            }

            if (isGrab)
            {
                hit.collider.gameObject.transform.position = pickupObject.position;
            }
            
        }
        
        
	}

    private void FixedUpdate()
    {
        movementVector.x = Input.GetAxis("LeftJoystickX") * speed;
        movementVector.y = Input.GetAxis("LeftJoystickY") * speed;
        
       body2d.velocity = new Vector2(movementVector.x, movementVector.y);
    }

    

    public void Drop()
    {
        pickupObject.parent = null;
        pickupObject.gameObject.AddComponent(typeof(Rigidbody2D));
        pickupObject = null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(transform.position, movementVector);
    }
}
