using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public float swayAmount = 0.1f;  // Amount of sway
    public float swaySmoothTime = 0.1f;  // How fast the sway smooths out

    private Vector3 initialPosition;
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        // Save the initial position of the weapon
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Calculate target sway position
        Vector3 targetPosition = initialPosition + new Vector3(-mouseX, -mouseY, 0) * swayAmount;

        // Smoothly move weapon to target position
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref currentVelocity, swaySmoothTime);
    }
}
