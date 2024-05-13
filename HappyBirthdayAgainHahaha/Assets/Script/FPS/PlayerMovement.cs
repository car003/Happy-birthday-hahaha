using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public bool switchToVerticalMove; 
    float moveSpeed = 7f; 
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;

    public float groundDrag = 5f;

    [Header("Jumping")]
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;
    bool doubleJump;

    [Header("Dash")]
    TrailRenderer tr;
    bool canDash = true;
    bool isDashing;
    float dashingPower = 50f;
    float dashingTime = 0.2f;
    float dashingCD = 1f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode dashKey = KeyCode.J;

    [Header("Ground Check")]
    [SerializeField] float playerHeight = 2f;
    //public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 35f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    Transform orientation;

    float horizontalInput;
    //float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.freezeRotation = true;
        orientation = GetComponent<Transform>();
        tr = GetComponent<TrailRenderer>();

        readyToJump = true;
    }

    private void Update()
    {
        //Disable other actions at dashing
        if (isDashing)
            return;

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");


        //on ground and reset doublejump
        if (grounded && !Input.GetKey(jumpKey))
        {
            doubleJump = false;
        }
        // when to jump
        if (Input.GetKeyDown(jumpKey) && (/*readyToJump &&*/ grounded|| doubleJump))
        {
            //readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }


        //dash
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void StateHandler()
    {
        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        if (!switchToVerticalMove)
            moveDirection = orientation.forward * 0 + orientation.right * horizontalInput;
        else
            moveDirection = orientation.forward * horizontalInput + orientation.right * 0;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, 0);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, 0);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, 0);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        doubleJump = !doubleJump;
    }
    private void ResetJump()
    {
        //readyToJump = true;

        exitingSlope = false;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x * dashingPower, 0f, 0);
        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        isDashing = false;
        rb.useGravity = true;
        tr.emitting = false;


        yield return new WaitForSeconds(dashingCD);
        canDash = true;

    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }





    //Collider col;
    //Rigidbody rb;

    //float horInput = 0f;
    //Vector2 currectInput = Vector2.zero;

    //float moveSpeed = 0f;
    //[SerializeField] float walkSpeed = 5f;
    //[SerializeField] float sprintSpeed = 15f;
    //[SerializeField] float jumpHeight = 5f;

    //[Header("Keybinds")]
    //public KeyCode sprintKey = KeyCode.LeftShift;

    ////bool grounded = false;

    //public MovementState state;

    //public enum MovementState
    //{
    //    walking,
    //    sprinting,
    //    air
    //}



    //// Start is called before the first frame update
    //void Start()
    //{

    //    col = GetComponent<Collider>();
    //    rb = GetComponent<Rigidbody>();
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    //grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);

    //    //Move();
    //    HandleMovementInput();
    //    StateHandler();
    //}
    //void HandleMovementInput()
    //{

    //}

    //void StateHandler()
    //{
    //    //mode sprinting
    //    if(Mathf.Approximately(rb.velocity.y, 0) && Input.GetKey(sprintKey))
    //    {
    //        state = MovementState.sprinting;
    //        moveSpeed = sprintSpeed;

    //    }

    //    //mode walking
    //    else if (Mathf.Approximately(rb.velocity.y, 0))
    //    {
    //        state = MovementState.walking;
    //        moveSpeed = walkSpeed;
    //    }

    //    //mode air
    //    else
    //    {
    //        state = MovementState.air;

    //    }
    //}

    //void Move()
    //{
    //    //get input
    //    horInput = Input.GetAxis("Horizontal") * moveSpeed;

    //    //move
    //    rb.velocity = new Vector3(horInput, rb.velocity.y, 0);

    //    //jump
    //    if (Input.GetButtonDown("Jump") && Mathf.Approximately(rb.velocity.y, 0))
    //    {
    //        rb.velocity = new Vector3(rb.velocity.x, jumpHeight, 0);
    //    }

    //    //player direction
    //    //transform.forward = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    //}





}
