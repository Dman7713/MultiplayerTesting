using UnityEngine;

public class ShooterWithRaycastAndBullets : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint1;
    public Transform bulletSpawnPoint2;
    public float fireRate = 0.5f;
    public float raycastRange = 100f;
    public LayerMask enemyLayer;
    public int damage = 25;
    public bool showRaycast = true;

    [HideInInspector]
    public bool isShooting = false;

    private float nextFireTime = 0f;
    private LineRenderer lineRenderer;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on this object!");
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.red;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Determine if the player is firing
        bool firingNow = Input.GetButton("Fire1") && Time.time >= nextFireTime;

        if (firingNow)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        // Update isShooting
        isShooting = firingNow;

        // Set Animator bool
        if (animator != null)
        {
            animator.SetBool("IsShooting", isShooting);
            Debug.Log("Animator IsShooting set to: " + isShooting);
        }

        // Hide raycast line if needed
        if (!showRaycast)
        {
            lineRenderer.enabled = false;
        }
    }

    void Shoot()
    {
        SpawnBullet(bulletSpawnPoint1);
        SpawnBullet(bulletSpawnPoint2);
    }

    void SpawnBullet(Transform spawnPoint)
    {
        Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        Vector3 origin = spawnPoint.position;
        Vector3 direction = spawnPoint.forward;

        lineRenderer.enabled = showRaycast;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, raycastRange, enemyLayer))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + direction * raycastRange);
        }

        Invoke(nameof(DisableLineRenderer), 0.05f);
    }

    void DisableLineRenderer()
    {
        lineRenderer.enabled = false;
    }
}
