using UnityEngine;

public class Pellet3D : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public LayerMask playerLayer;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;  // Fixed here

        // Ignore collisions with the player
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, 2f, playerLayer);
        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // Destroy the pellet on any collision (player collisions are ignored)
        Destroy(gameObject);
    }
}
