using UnityEngine;
using System;

public class ShieldProjectile : MonoBehaviour
{
    public Transform player;
    public float returnSpeed = 25f;
    public float spinSpeed = 360f;
    public float detectRadius = 10f;
    public LayerMask enemyLayer; // Set this in the Inspector

    private Rigidbody rb;
    private Collider shieldCollider;
    private bool isReturning = false;
    private bool isAttackingEnemy = false;
    private Transform targetEnemy;

    public event Action OnShieldReturned;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        shieldCollider = GetComponent<Collider>();

        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider != null && shieldCollider != null)
            {
                Physics.IgnoreCollision(shieldCollider, playerCollider);
            }
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, spinSpeed * Time.fixedDeltaTime, 0f));

        if (!isReturning && !isAttackingEnemy)
        {
            DetectEnemies();
        }

        if (isAttackingEnemy && targetEnemy != null)
        {
            Vector3 dirToEnemy = (targetEnemy.position - transform.position).normalized;
            rb.linearVelocity = dirToEnemy * returnSpeed;

            if (Vector3.Distance(transform.position, targetEnemy.position) < 1f)
            {
                isAttackingEnemy = false;
                targetEnemy = null;
                StartReturn();
            }
        }
        else if (isReturning)
        {
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

    void DetectEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRadius, enemyLayer);
        if (enemies.Length > 0)
        {
            float minDist = Mathf.Infinity;
            Transform closest = null;

            foreach (Collider enemy in enemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy.transform;
                }
            }

            if (closest != null)
            {
                targetEnemy = closest;
                isAttackingEnemy = true;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isReturning && !isAttackingEnemy)
        {
            StartReturn();
        }
    }

    public void StartReturn()
    {
        if (!isReturning)
        {
            isReturning = true;
            isAttackingEnemy = false;
            targetEnemy = null;
        }
    }

    // Always draw detection radius in scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
