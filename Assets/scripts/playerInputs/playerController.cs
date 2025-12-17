using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CameraController))]
public class playerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CameraController cameraController;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void OnMove(InputValue value)
    {
        cameraController.MoveInput = value.Get<Vector2>();   
    }

    void OnLook(InputValue value)
    {
        cameraController.LookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        cameraController.IsRunning = value.isPressed;
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            cameraController.TryJump();
        }
    }


    void OnValidate()
    {
        if (cameraController == null)
        {
            cameraController = GetComponent<CameraController>();
        }
    }
}
