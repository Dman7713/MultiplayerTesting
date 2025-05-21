using UnityEngine;

public class ShooterWithRaycastAndBullets : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float fireRate = 0.5f;
    public float raycastRange = 100f;
    public LayerMask enemyLayer;
    public int damage = 25;

    public bool showRaycast = true;
    private float nextFireTime = 0f;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
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
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }

        if (!showRaycast)
        {
            lineRenderer.enabled = false;
        }
    }

    void Shoot()
    {
        // Spawn visual bullet (no damage from bullet)
        Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Raycast for damage
        Vector3 origin = bulletSpawnPoint.position;
        Vector3 direction = bulletSpawnPoint.forward;

        RaycastHit hit;
        lineRenderer.enabled = showRaycast;

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
