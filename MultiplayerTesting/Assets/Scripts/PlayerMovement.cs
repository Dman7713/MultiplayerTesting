using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerHeight = 2f;

    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] Transform groundCheck;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 6f;
    [SerializeField] public float movementMultiplier = 10f;
    [SerializeField] float airMovementMultiplier = 0.4f;
    [SerializeField] float maxVelocity;

    [Header("Sprinting")]
    [SerializeField] public float walkSpeed = 4f;
    [SerializeField] public float sprintSpeed = 6f;
    [SerializeField] float acceleration = 10f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] KeyCode slideKey = KeyCode.LeftControl;

    [Header("Jumping/Ground")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask ground;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    [Header("Ceiling Settings")]
    [SerializeField] private float ceilingCheckRange;

    [Header("Crouch Settings")]
    [SerializeField] private Vector3 crouchHeight;
    [SerializeField] private float crouchMoveSpeed;
    [SerializeField] private float crouchSprintSpeed;
    [SerializeField] private float crouchFloorDetectDist;

    [Header("Slide Settings")]
    [SerializeField] float requiredSlideSpeed;
    [SerializeField] float slideForce;
    [SerializeField] float maxSlideForce;
    [SerializeField] float minimumslideForce;

    [Header("Slide Jump/Fall Settings")]
    [SerializeField] float slideJumpForce = 10f;
    [SerializeField] float slideAirDrag;
    [SerializeField] float slideFallDelay;
    [SerializeField] float slideCoyoteTime;

    [Header("Jump Boost Settings")]
    [SerializeField] float boostedJumpForce = 8f;
    [SerializeField] float jumpBoostWindow = 0.4f;
    float jumpBoostTimer;
    bool canBoostJump;

    // Runtime variables
    public float currentVelocity;
    float horizontalMovement;
    float verticalMovement;
    float currentSlideSpeed;
    float currentSlideDelayTime;
    float currentSlideCoyoteTime;
    float dAngle;

    public bool isSliding;
    public bool isSprinting;
    public bool isGrounded;
    public bool isCrouching;
    bool ableToCrouch;
    public bool isJumping;
    bool slidingOnSlope;
    bool isUnderCeiling;

    int impulseCounter;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    Vector3 slideDirection;

    RaycastHit slopeHit;
    RaycastHit ceilingHit;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (maxSlideForce < slideForce)
        {
            maxSlideForce = slideForce;
        }

        rb.sleepThreshold = 0;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, isSliding ? groundDistance * 2 : groundDistance, ground);
        ableToCrouch = Physics.CheckSphere(groundCheck.position, crouchFloorDetectDist, ground);

        CeilingCheck();
        MyInput();
        StartSlide();

        if (Input.GetKeyDown(jumpKey) && isGrounded && Time.timeScale != 0)
        {
            Jump();
        }

        if (isJumping && isGrounded)
        {
            isJumping = false;
            canBoostJump = true;
            jumpBoostTimer = jumpBoostWindow;
        }

        if (canBoostJump)
        {
            jumpBoostTimer -= Time.deltaTime;
            if (jumpBoostTimer <= 0)
                canBoostJump = false;
        }

        if ((isSliding && Input.GetKeyDown(jumpKey)) || (isSliding && Input.GetKeyUp(slideKey)))
        {
            if ((Input.GetKeyDown(jumpKey) && isGrounded) || Input.GetKeyDown(jumpKey) && currentSlideCoyoteTime <= 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(transform.up * slideJumpForce, ForceMode.Impulse);
                isJumping = true;
            }

            StopSlide();
        }

        Crouch();
        currentVelocity = rb.linearVelocity.magnitude;
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void FixedUpdate()
    {
        ControlDrag();
        ControlSpeed();
        MovePlayer();

        if (isSliding)
            SlideMovement();
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope() && !isSliding)
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        else if (isGrounded && OnSlope() && !isSliding)
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        else if (!isGrounded && !isSliding)
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMovementMultiplier, ForceMode.Acceleration);
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            dAngle = angle;
            return slopeHit.normal != Vector3.up;
        }
        return false;
    }

    void Jump()
    {
        rb.WakeUp();

        if (!isGrounded) return;

        float actualJumpForce = canBoostJump ? boostedJumpForce : jumpForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(transform.up * actualJumpForce, ForceMode.Impulse);

        isJumping = true;
        canBoostJump = false;
    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && isGrounded && !isCrouching)
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
        else if (isCrouching && !Input.GetKey(sprintKey))
            moveSpeed = Mathf.Lerp(moveSpeed, crouchMoveSpeed, acceleration * Time.deltaTime);
        else if (Input.GetKey(sprintKey) && isCrouching)
            moveSpeed = Mathf.Lerp(moveSpeed, crouchSprintSpeed, acceleration * Time.deltaTime);
        else
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);

        isSprinting = Input.GetKey(sprintKey);
    }

    void ControlDrag()
    {
        if (isSliding && !isGrounded)
            rb.linearDamping = slideAirDrag;
        else if (!isGrounded && !isSliding)
            rb.linearDamping = airDrag;
        else
            rb.linearDamping = groundDrag;
    }

    void Crouch()
    {
        if (Input.GetKey(crouchKey) && ableToCrouch && !isSliding || isUnderCeiling && ableToCrouch && !isSliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight.y, transform.localScale.z);
            isCrouching = true;
            if (impulseCounter < 1)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                impulseCounter = 1;
            }
        }
        else if (!isSliding)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            isCrouching = false;
            impulseCounter = 0;
        }
    }

    void CeilingCheck()
    {
        if (isSliding || isCrouching)
        {
            isUnderCeiling = Physics.Raycast(transform.position, Vector3.up, ceilingCheckRange);
        }
        else
        {
            isUnderCeiling = false;
        }
    }

    void StartSlide()
    {
        if (Input.GetKeyDown(slideKey) && !isCrouching && currentVelocity >= requiredSlideSpeed && !isSliding && isGrounded)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight.y, transform.localScale.z);
            isSliding = true;
            currentSlideSpeed = slideForce;

            slideDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
            currentSlideDelayTime = slideFallDelay;
            currentSlideCoyoteTime = slideCoyoteTime;
        }
    }

    void SlideMovement()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, orientation.forward);

        if (forwardSpeed < maxSlideForce)
        {
            if (!isGrounded)
            {
                if (slideFallDelay >= 0)
                {
                    slideFallDelay -= Time.deltaTime;
                }
                else
                {
                    StopSlide();
                    return;
                }
            }

            if (OnSlope())
                rb.AddForce(slopeMoveDirection * currentSlideSpeed, ForceMode.Force);
            else
                rb.AddForce(slideDirection.normalized * currentSlideSpeed, ForceMode.Force);
        }

        currentSlideCoyoteTime -= Time.deltaTime;
    }

    void StopSlide()
    {
        isSliding = false;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
