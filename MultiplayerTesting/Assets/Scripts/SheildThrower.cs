using UnityEngine;

public class ShieldThrower : MonoBehaviour
{
    public GameObject shieldPrefab;
    public Transform throwPoint;
    public float throwForce = 40f;
    public GameObject heldShieldModel; // assign in inspector

    private GameObject currentShield;

    void Update()
    {
        if (Input.GetMouseButtonDown(2)) // Middle mouse button
        {
            if (currentShield == null)
            {
                ThrowShield();
            }
            else
            {
                ShieldProjectile shieldProj = currentShield.GetComponent<ShieldProjectile>();
                if (shieldProj != null)
                {
                    shieldProj.StartReturn();
                }
            }
        }
    }

    void ThrowShield()
    {
        currentShield = Instantiate(shieldPrefab, throwPoint.position, throwPoint.rotation);
        ShieldProjectile shieldProj = currentShield.GetComponent<ShieldProjectile>();

        if (shieldProj != null)
        {
            shieldProj.player = this.transform;
            shieldProj.OnShieldReturned += ShieldReturned;
        }

        Rigidbody rb = currentShield.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = throwPoint.forward * throwForce;
        }

        if (heldShieldModel != null)
            heldShieldModel.SetActive(false); // Hide when thrown
    }

    void ShieldReturned()
    {
        currentShield = null;

        if (heldShieldModel != null)
            heldShieldModel.SetActive(true); // Show when returned
    }
}
