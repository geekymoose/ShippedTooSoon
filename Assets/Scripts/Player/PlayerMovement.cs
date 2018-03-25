using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Range(0, 100)]
    public float speed = 5f;

    private Rigidbody2D body2d;
    private bool canPickup = false;
    private Transform objectToPickUp;
    private Vector2 movementVector;
    private Vector2 forward;

    // Use this for initialization
    private void Start()
    {
        body2d = GetComponent<Rigidbody2D>();

        objectToPickUp = GameObject.FindGameObjectWithTag("Pickup").transform;
    }

    // Update is called once per frame
    private void Update()
    {
        //the player have to be in the collider of the pickup object if he want to pick up the object
        if (Input.GetButtonDown("A") && canPickup)
        {
            objectToPickUp.transform.parent = transform;

            objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = false;
        }

        //when the player release the button drop the object
        if (Input.GetButtonUp("A"))
        {
            canPickup = false;
            Drop();
        }
    }

    private void FixedUpdate()
    {
        //movement with the axis of the xbox gamepad
        movementVector.x = Input.GetAxis("LeftJoystickX") * speed;
        movementVector.y = Input.GetAxis("LeftJoystickY") * speed;

        
        //move the player
        body2d.velocity = new Vector2(movementVector.x, movementVector.y);

        //rotate the player forward the direction
        //float degrees = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;

        //to make only rotate in 90 degrees
        //if (degrees % 90 == 0)
        //{
        //    body2d.rotation = degrees;
        //}
    }

    //drop the object when the player release the button
    public void Drop()
    {
        objectToPickUp.parent = null;
        objectToPickUp = GameObject.FindGameObjectWithTag("Pickup").transform;
        objectToPickUp.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        objectToPickUp.gameObject.GetComponent<Rigidbody2D>().simulated = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(transform.position, movementVector.normalized);
    }

    //if the player collide on the object that he can pickup
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Pickup")
        {
            canPickup = true;
        }

        if (collision.gameObject.tag == "Wall")
        {
            
        }
    }

    //desactivate the pickup when the player exit the collider
    private void OnCollisionExit2D(Collision2D collision)
    {
        canPickup = false;
    }
}