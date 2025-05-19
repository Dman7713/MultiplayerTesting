using UnityEngine;

public class Pellet3D : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public LayerMask playerLayer; // Assign the Player layer in inspector

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        // Ignore collisions with all colliders on the player layer
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, 2f, playerLayer);
        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
        }

        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Handle damage or effects here if needed
        Destroy(gameObject);
    }
}
