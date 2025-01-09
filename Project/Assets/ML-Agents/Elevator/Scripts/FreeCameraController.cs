using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float movementSpeed = 10f;  // Speed of movement
    public float mouseSensitivity = 100f; // Sensitivity of the mouse

    private float pitch = 0f; // Rotation around X-axis
    private float yaw = 0f;   // Rotation around Y-axis


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) // Check if left mouse button is held down
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping the camera

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Keyboard movement
        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"), // A/D or Left/Right
            Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E) ? 1 : (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q) ? -1 : 0), // Space/E for up, Control/Q for down
            Input.GetAxis("Vertical")  // W/S or Up/Down

        );

        transform.Translate(direction * (movementSpeed * Time.deltaTime), Space.Self);
    }
}
