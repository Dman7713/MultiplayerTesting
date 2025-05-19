using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] WallRun wallRun;
    [SerializeField] PlayerMovement playerMovement;
    [Space(5)]

    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;

    float currentYRotation;
    bool rotationChosen;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;


        if (playerMovement.isSliding == true)
        {
            if (rotationChosen == false)
            {
                currentYRotation = yRotation;
                rotationChosen = true;
            }
            yRotation = Mathf.Clamp(yRotation, currentYRotation -90f, currentYRotation + 90);
            xRotation = Mathf.Clamp(xRotation, -90f, 30f);
        }
        else
        {
            rotationChosen = false;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }


        cam.transform.rotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
