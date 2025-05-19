using UnityEngine;
using System;

public class ShieldProjectile : MonoBehaviour
{
    public Transform player;
    public float returnSpeed = 25f;
    public float spinSpeed = 360f;

    private Rigidbody rb;
    private Collider shieldCollider;
    private bool isReturning = false;

    public event Action OnShieldReturned;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        shieldCollider = GetComponent<Collider>();

        // Ignore collisions with player
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && shieldCollider != null)
            {
                Physics.IgnoreCollision(shieldCollider, playerCollider);
            }
        }

        // Smooth motion
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        // Rotate around Y axis using physics rotation
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, spinSpeed * Time.fixedDeltaTime, 0f));

        if (isReturning)
        {
            // Disable collider to ignore collisions on return
            if (shieldCollider.enabled)
                shieldCollider.enabled = false;

            Vector3 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * returnSpeed;

            if (Vector3.Distance(transform.position, player.position) < 1f)
            {
                OnShieldReturned?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isReturning)
        {
            StartReturn();
        }
    }

    public void StartReturn()
    {
        if (!isReturning)
        {
            isReturning = true;
        }
    }
}
