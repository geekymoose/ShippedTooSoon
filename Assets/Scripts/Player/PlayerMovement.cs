using UnityEngine;
using UnityEngine.Assertions;

public class PlayerMovement : MonoBehaviour
{
    [Range(0, 100)]
    public float speed = 5f;

    private Rigidbody2D body2d;
    private bool canPickup = false;
    private Transform objectToPickUp;
    private Vector2 movementVector;
    private Vector2 forward;
    private Animator anim;

    // Use this for initialization
    private void Start()
    {
        body2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void Update()
    {
        //the player have to be in the collider of the pickup object if he want to pick up the object
        if (Input.GetButtonDown("Fire1") && canPickup)
        {
            objectToPickUp.transform.parent = transform;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = false;
        }

        //when the player release the button drop the object
        else if (Input.GetButtonUp("Fire1"))
        {
            canPickup = false;
            Drop();
        }
        //movement with the axis of the xbox gamepad
        movementVector.x = Input.GetAxisRaw("Horizontal") * speed;
        movementVector.y = Input.GetAxisRaw("Vertical") * speed;

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
        {
            anim.SetBool("canMove", true);
            DefinePlayerDirection(movementVector);
            Debug.Log("canMove");
        }
        else
        {
            anim.SetBool("canMove", false);
        }

        /*
        Debug.Log(anim.GetFloat("MoveX"));

        if (anim.GetFloat("MoveX") < 0.0f)
        {
            anim.SetTrigger("moveLeft");
        }

        if (anim.GetFloat("MoveX") > 0.0f)
        {
            anim.SetTrigger("moveRight");
        }

        if (anim.GetFloat("MoveY") < 0.0f)
        {
            anim.SetTrigger("moveDown");
        }

        if (anim.GetFloat("MoveY") > 0.0f)
        {
            anim.SetTrigger("moveUp­");
        }*/
    }

    public void FixedUpdate()
    {
       
        //move the player
        body2d.velocity = new Vector2(movementVector.x, movementVector.y);
    }

    //drop the object when the player release the button
    public void Drop()
    {
        if(this.objectToPickUp != null) {
            objectToPickUp.parent = null;
            objectToPickUp = GameObject.FindGameObjectWithTag("Pickup").transform;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, movementVector.normalized);
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

    public void DefinePlayerDirection(Vector2 move)
    {
        anim.SetFloat("MoveX", movementVector.x);
        anim.SetFloat("MoveY", movementVector.y);
    }
}