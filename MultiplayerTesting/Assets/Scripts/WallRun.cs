using UnityEngine;

public class WallRun : MonoBehaviour
{
    [SerializeField] int maxJumpsOnOneWall = 2;

    [Header("References")]
    [SerializeField] Transform orientation;
    [SerializeField] PlayerMovement pm;

    [Header("Wall Detection")]
    [SerializeField] float wallDistance = 0.5f;
    [SerializeField] float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    [SerializeField] float wallRunGravity;
    [SerializeField] float wallRunJumpForce;
    [SerializeField] float wallRunSpeed;

    [Header("Camera")]
    [SerializeField] Camera cam;
    [SerializeField] float fov;
    [SerializeField] float wallRunFov;
    [SerializeField] float wallRunFovTime;
    [SerializeField] float camTilt;
    [SerializeField] float camTiltTime;

    public float tilt { get; private set; }

    public bool wallLeft = false;
    public bool wallRight = false;
    public bool isWallJumping;
    bool DirectionChosen = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    private Rigidbody rb;
    public bool wallRunning = false;
    private Vector3 wallRunDirection;
    string lastWallName = "";
    int sameWallJumps = 0;
    int plyrLyr;
    int layermask;

    [SerializeField] private Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        plyrLyr = LayerMask.NameToLayer("Player");
        layermask = ~(1 << plyrLyr);
    }

    private void Update()
    {
        CheckWall();

        if (pm.isGrounded && sameWallJumps != 0)
        {
            sameWallJumps = 0;
            lastWallName = "";
        }

        if (CanWallRun())
        {

            if (wallLeft)
            {
                PreWallRun();
                StartWallRun();
                anim.SetBool("LeftWall", true);
                anim.SetBool("RightWall", false);
            }
            else if (wallRight)
            {
                PreWallRun();
                StartWallRun();
                anim.SetBool("RightWall", true);
                anim.SetBool("LeftWall", false);
            }
            else
            {
                StopWallRun();
                anim.SetBool("LeftWall", false);
                anim.SetBool("RightWall", false);
            }
        }
        else
        {
            StopWallRun();
            anim.SetBool("LeftWall", false);
            anim.SetBool("RightWall", false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && wallRunning)
        {
            WallJump();
        }
    }

    private void FixedUpdate()
    {
        if (wallRunning)
        {
            WallRunMove();
        }
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    private void CheckWall()
    {
        if (!wallRight)
        {
            wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance, layermask);
            Debug.Log(leftWallHit.ToString());
        }
        if (!wallLeft)
        {
            wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance, layermask);
            Debug.Log(leftWallHit.ToString());
        }
    }

    void PreWallRun()
    {
        if (!DirectionChosen)
        {
            wallRunDirection = orientation.forward;
            DirectionChosen = true;
        }
    }

    private void StartWallRun()
    {
        rb.useGravity = false;
        wallRunning = true;
        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunFov, wallRunFovTime * Time.deltaTime);
    }

    private void WallRunMove()
    {
        // Apply downward force to keep player pressed against the wall
        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        // Get the player's current velocity in the direction of wallRunDirection
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, wallRunDirection);

        // Accelerate only if under the desired wall run speed in that direction
        if (forwardSpeed < wallRunSpeed)
        {
            rb.AddForce(wallRunDirection * wallRunSpeed, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        wallRunning = false;
        rb.useGravity = true;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunFovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
        DirectionChosen = false;
    }

    private void WallJump()
    {
        Vector3 jumpDirection = transform.up;
        if (sameWallJumps <= maxJumpsOnOneWall)
        {
            if (wallLeft)
            {
                jumpDirection += leftWallHit.normal;
                if (lastWallName != null && lastWallName != leftWallHit.collider.gameObject.name)
                {
                    lastWallName = leftWallHit.collider.gameObject.name;
                    sameWallJumps = 0;
                }
                else if (lastWallName != null && lastWallName == leftWallHit.collider.gameObject.name)
                {
                    sameWallJumps += 1;
                }
            }
            else if (wallRight)
            {
                jumpDirection += rightWallHit.normal;
                if (lastWallName != null && lastWallName != rightWallHit.collider.gameObject.name)
                {
                    lastWallName = rightWallHit.collider.gameObject.name;
                    sameWallJumps = 0;
                }
                else if (lastWallName != null && lastWallName == rightWallHit.collider.gameObject.name)
                {
                    sameWallJumps += 1;
                }
            }

            // Preserve some horizontal momentum, but reset vertical speed
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(jumpDirection.normalized * wallRunJumpForce, ForceMode.Impulse);
        }
    }
}
