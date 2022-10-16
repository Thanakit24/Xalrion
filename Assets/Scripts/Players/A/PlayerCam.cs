using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    float mouseX;
    float mouseY;
    public Transform orientation;
    public bool playerA = false;
    public float xRotation;
    public float yRotation;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!playerA)
        {
            //change the cam to the desired look dir
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerA)
        {
            mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;
            yRotation -= mouseX;
            xRotation -= mouseY;
        }
        else
        {
            //print("Player B set to controller cam");                                                        
            mouseX = Input.GetAxis("CameraHorizontal") * Time.deltaTime * sensX;
            mouseY = Input.GetAxis("CameraVertical") * Time.deltaTime * sensY;
            yRotation += mouseX;
            xRotation += mouseY;
        }
       
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
