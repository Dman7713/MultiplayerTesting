using UnityEngine;

public class PlayerGravScaling : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerMovement pm;
    [SerializeField] WallRun wallRun;
    [SerializeField] Rigidbody rb;

    [Header("Falling Settings")]
    [SerializeField] float fallingGrav;
    [SerializeField] float maxFallGrav;
    [SerializeField] float timeToMaxGrav;

    public float currentFallGrav;
    float timeElapsed;

    void Start()
    {
        if (maxFallGrav < fallingGrav)
        {
            maxFallGrav = fallingGrav;
        }
    }

    void FixedUpdate()
    {
        FallingGrav();
    }

    void FallingGrav()
    {
        float t = timeElapsed / timeToMaxGrav;

        if (!pm.isGrounded && !wallRun.wallRunning)
        {
            if (timeElapsed < timeToMaxGrav)
            {
                currentFallGrav = Mathf.Lerp(currentFallGrav, maxFallGrav, t);
                rb.AddForce(Vector3.down * currentFallGrav, ForceMode.Force);

                timeElapsed += Time.deltaTime;
            }
        }
        else
        {
            currentFallGrav = fallingGrav;
            timeElapsed = 0;
        }
    }
    public void ResetGrav()
    {
        currentFallGrav = fallingGrav;
    }
}
