using UnityEngine;
using Unity.Cinemachine;
using System;

[RequireComponent(typeof(CharacterController))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float MoveSpeed => IsRunning ? runSpeed : walkSpeed;
    public float Acceleration = 15f;

    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float JumpHeight = 2f;

    // Está a sprintar apenas se IsRunning for true e tiver alguma velocidade
    public bool Sprinting => IsRunning && CurrentSpeed > 0.1f;

    [Header("Look Settings")]
    public Vector2 LookSensitivity = new Vector2(1f, 1f);
    public float MaxLookAngle = 90f;
    [SerializeField] float currentLook = 0f;

    public float CurrentLook
    {
        get => currentLook;
        set
        {
            currentLook = Mathf.Clamp(value, -MaxLookAngle, MaxLookAngle);
        }
    }

    [Header("Camera parameters")]
    [SerializeField] float cameraFovNormal = 60f;
    [SerializeField] float cameraFovRunning = 80f;
    [SerializeField] float cameraSmoothing = 1f;

    float TargetCameraFov
    {
        get
        {
            return IsRunning ? cameraFovRunning : cameraFovNormal;
        }
    }

    [Header("Physics parameters")]
    [SerializeField] float gravityScale = 3f;

    public float VerticalVelocity = 0f;

    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }

    public bool IsGrounded => characterController.isGrounded;




    [Header("Inputs")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool IsRunning;

    [Header("Components")]
    [SerializeField] CharacterController characterController;
    [SerializeField] CinemachineCamera fpCamera;

    void Update()
    {
        MoveUptade();
        LookUpdate();
        CameraUpdate();
    }

    void OnValidate()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    void MoveUptade()
    {
        Vector3 move = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        move.y = 0;
        move.Normalize();

        if (move.sqrMagnitude >= 0.01f)
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, move * MoveSpeed, Acceleration * Time.deltaTime);
        }
        else
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, Vector3.zero, Acceleration * Time.deltaTime);
        }

        if (IsGrounded && VerticalVelocity < 0f)
        {
            VerticalVelocity = -3f;
        }
        else
        {
            VerticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
        }

        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);

        characterController.Move(fullVelocity * Time.deltaTime);

        CurrentSpeed = CurrentVelocity.magnitude;
    }

    void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);

        // olhar para cima e para baixo
        CurrentLook -= input.y;
        fpCamera.transform.localRotation = Quaternion.Euler(CurrentLook, 0f, 0f);

        // olhar para os lados
        transform.Rotate(Vector3.up * input.x);
    }

    void CameraUpdate()
    {
        float targetFOV = TargetCameraFov;

        if (Sprinting)
        {
            float speedRatio = CurrentSpeed / runSpeed;
            targetFOV = Mathf.Lerp(cameraFovNormal, cameraFovRunning, speedRatio);
        }

        fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, cameraSmoothing * Time.deltaTime);
    }

    public void TryJump()
    {
        if (IsGrounded == false)
        {
            return;
        }
        VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y * gravityScale);
    }
}
