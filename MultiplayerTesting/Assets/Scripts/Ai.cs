using UnityEngine;
using UnityEngine.AI;

public class SimpleAINavShooter : MonoBehaviour
{
    [Header("Targeting")]
    public float followRange = 15f;
    public float shootingRange = 10f;
    public float bodyRotationSpeed = 5f;
    public float gunRotationSpeed = 10f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform gunPoint;
    public Transform gunPivot;
    public float fireRate = 1.5f; // Time between shots
    private float shootTimer;

    [Header("Patrolling")]
    public float patrolRadius = 20f;
    public float patrolDelay = 3f;
    private float patrolTimer;

    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolTimer = patrolDelay;
        shootTimer = 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player with tag 'Player' not found.");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        shootTimer += Time.deltaTime;

        if (distanceToPlayer <= followRange)
        {
            agent.SetDestination(player.position);
            RotateBodyAndGun();

            if (distanceToPlayer <= shootingRange && shootTimer >= fireRate)
            {
                Shoot();
                shootTimer = 0f;
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolDelay)
        {
            Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1);
            agent.SetDestination(newPos);
            patrolTimer = 0;
        }
    }

    void RotateBodyAndGun()
    {
        // Rotate BODY horizontally
        Vector3 flatDirection = player.position - transform.position;
        flatDirection.y = 0f;
        if (flatDirection != Vector3.zero)
        {
            Quaternion bodyRot = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRot, Time.deltaTime * bodyRotationSpeed);
        }

        // Rotate GUN toward player in 3D
        if (gunPivot != null)
        {
            Vector3 gunDirection = player.position - gunPivot.position;
            Quaternion gunRot = Quaternion.LookRotation(gunDirection);
            gunPivot.rotation = Quaternion.Slerp(gunPivot.rotation, gunRot, Time.deltaTime * gunRotationSpeed);
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && gunPoint != null)
        {
            Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation);
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
