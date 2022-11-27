using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public InputMaster playerInput;
    Vector2 lastMousePos;
    Vector2 lastGamepadPos;
    [HideInInspector] public Vector2 lookDir;
    // Start is called before the first frame update
    void Start()
    {
        //playerInput.Player.Camera.performed += ctx => OnLook(ctx);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //InputSystem.onDeviceChange += ChangeController;
        //InputSystem.onDeviceCommand +=
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDestroy()
    {
        //InputSystem.onDeviceChange -= ChangeController;
    }
    //void ChangeController(InputDevice device, InputDeviceChange deviceChange)
    //{
    //    switch (deviceChange)
    //    {
    //        case InputDeviceChange.Added:
    //            print(device as Gamepad);
    //            if(device as Gamepad != null)
    //            {
    //                InputSystem.DisableDevice(Mouse.current);
    //                //playerInput.Player.Move.Disable();
    //            }
    //            break;

    //        case InputDeviceChange.Removed:
    //            if(device as Gamepad != null)
    //            {
    //                InputSystem.EnableDevice(Mouse.current);
    //            }
    //            break;

    //    }
    //}
    //void OnLook(InputValue value)
    //{
    //    lookDir = value.Get<Vector2>();
    //}
    void Update()
    {   
        // lookDir = playerInput.Player.Camera.ReadValue<Vector2>();
        
        //var lookMouseDir = lastMousePos - Mouse.current.position.ReadValue();
        //Debug.Log(Mouse.current.position.ReadValue());
        //var lookGamepadDir  = Gamepad.current.rightStick.ReadValue();
        //var lookDir = lookMouseDir + lookGamepadDir;

        yRotation += lookDir.x * Time.deltaTime * sensX;
        xRotation -= lookDir.y * Time.deltaTime * sensY;
        //lastGamepadPos = Gamepad.current.rightStick.ReadValue();
        //lastMousePos = Mouse.current.position.ReadValue();
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        //if (playerA)
        //{
        //    //mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        //    //mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;
        //    yRotation -= mouseX;
        //    xRotation -= mouseY;
        //}
        //else
        //{
        //    //print("Player B set to controller cam");                                                        
        //    //mouseX = Input.GetAxis("CameraHorizontal") * Time.deltaTime * sensX;
        //    //mouseY = Input.GetAxis("CameraVertical") * Time.deltaTime * sensY;
        //    yRotation += mouseX;
        //    xRotation += mouseY;
        //}


    }
    //public void OnLook(InputAction.CallbackContext ctx)
    //{
    //    var lookDir = ctx.ReadValue<Vector2>();

    //    yRotation += lookDir.x;
    //    xRotation -= lookDir.y;
    //}

    // Update is called once per frame

    //public Vector2 GetMouseDelta()
    //{
    //    return playerInput.Player.Camera.ReadValue<Vector2>();
    //}

}
