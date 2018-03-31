using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------

    [Range(0, 100)]
    public float speed = 5f;

    private Rigidbody2D body2d;
    private Animator anim;
    private Vector2 movementVector;
    private bool canMove = true;


    // -------------------------------------------------------------------------
    // Unity Methods
    // -------------------------------------------------------------------------

    // Use this for initialization
    private void Start()
    {
        body2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void Update()
    {
        if(this.canMove)
        {
            this.HandleMovement();
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
    }

    public void FixedUpdate()
    {
        body2d.velocity = new Vector2(movementVector.x, movementVector.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, movementVector.normalized);
    }


    // -------------------------------------------------------------------------
    // Methods
    // -------------------------------------------------------------------------

    private void HandleMovement()
    {
        movementVector.x = Input.GetAxisRaw("Horizontal") * speed;
        movementVector.y = Input.GetAxisRaw("Vertical") * speed;

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
        {
            this.anim.SetBool("IsWalking", true);
            this.anim.SetFloat("MoveX", movementVector.x);
            this.anim.SetFloat("MoveY", movementVector.y);
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
    }

    public void SetCanMove(bool value)
    {
        this.canMove = value;
    }
}