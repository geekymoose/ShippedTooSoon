using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Attributes
    // -------------------------------------------------------------------------

    [Range(0, 30)]
    public float speed = 5f;

    private Rigidbody2D body2d;
    private Animator anim;
    private Vector2 playerDirection; // Orientation of the player
    private Vector2 movementVector;
    private bool canMove = true;


    // -------------------------------------------------------------------------
    // Unity Methods
    // -------------------------------------------------------------------------

    private void Start()
    {
        body2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void Update()
    {
        this.HandleMovement();
    }

    public void FixedUpdate()
    {
        if(this.canMove)
        {
            body2d.velocity = new Vector2(movementVector.x, movementVector.y);
        }
        else
        {
            body2d.velocity = Vector2.zero;
        }
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

        if (movementVector.x != 0.0f || movementVector.y != 0.0f)
        {
            this.anim.SetBool("IsWalking", true);
            this.anim.SetFloat("MoveX", movementVector.x);
            this.anim.SetFloat("MoveY", movementVector.y);
            this.playerDirection = this.movementVector.normalized;
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
    }

    public void FreezeMovement() {
        this.canMove = false;
    }

    public void AllowMovement() {
        this.canMove = true;
    }

    public Vector2 getDirection() {
        return this.playerDirection;
    }
}