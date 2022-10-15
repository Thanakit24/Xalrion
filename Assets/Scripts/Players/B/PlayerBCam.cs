using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    private float xRotation;
    private float yRotation;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("CameraHorizontalB") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("CameraVerticalB") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);  
    }
}
