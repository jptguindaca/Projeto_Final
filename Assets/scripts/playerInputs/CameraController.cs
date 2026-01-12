using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Movement Settings")]
    public float MoveSpeed => IsRunning ? runSpeed : walkSpeed;

    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float JumpHeight = 2f;

    public bool Sprinting => IsRunning && CurrentSpeed > 0.1f;

    [Header("Look Settings")]
    public Vector2 LookSensitivity = new Vector2(1f, 1f);
    public float MaxLookAngle = 90f;

    [Header("Look Smooth")]
    [SerializeField] float lookSmoothTime = 0.05f;

    float currentLook = 0f;
    Vector2 smoothLookInput;
    Vector2 lookVelocity;

    [Header("Camera parameters")]
    [SerializeField] float cameraFovNormal = 60f;
    [SerializeField] float cameraFovRunning = 80f;
    [SerializeField] float cameraSmoothing = 4f;

    float TargetCameraFov => Sprinting ? cameraFovRunning : cameraFovNormal;

    [Header("Physics parameters")]
    [SerializeField] float gravityScale = 3f;

    public float VerticalVelocity = 0f;
    public Vector3 CurrentVelocity { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool IsGrounded => characterController.isGrounded;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrain = 20f;
    [SerializeField] float staminaRegen = 15f;
    public float CurrentStamina { get; private set; }

    [Header("Inputs")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool IsRunning;
    public bool updatingRotation;

    [Header("Components")]
    [SerializeField] CharacterController characterController;
    [SerializeField] CinemachineCamera fpCamera;

    [Header("Animation")]
    [SerializeField] Animator animator;

    [Header("Footsteps")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] float walkPitch = 1f;
    [SerializeField] float runPitch = 1.4f;
    [SerializeField] float minSpeedToPlay = 0.1f;

    // Grounded buffer
    float lastGroundedTime;
    [SerializeField] float groundedBufferTime = 0.1f; // permite saltar até 0.1s após sair do chão

    void Awake()
    {
        Instance = this;
        CurrentStamina = maxStamina;
    }

    void Update()
    {
        if (updatingRotation) return;

        MoveUpdate();
        LookUpdate();
        CameraUpdate();
        StaminaUpdate();
        AnimationUpdate();
        FootstepUpdate();
    }

    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    void MoveUpdate()
    {
        Vector3 move = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        move.y = 0;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (move.sqrMagnitude >= 0.01f)
            CurrentVelocity = move * MoveSpeed;
        else
            CurrentVelocity = Vector3.zero;

        // Atualiza o buffer do chão
        if (IsGrounded)
            lastGroundedTime = Time.time;

        // Gravidade
        if (IsGrounded && VerticalVelocity < 0f)
            VerticalVelocity = -3f;
        else
            VerticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;

        Vector3 fullVelocity = new Vector3(CurrentVelocity.x, VerticalVelocity, CurrentVelocity.z);
        characterController.Move(fullVelocity * Time.deltaTime);

        CurrentSpeed = CurrentVelocity.magnitude;
    }

    void LookUpdate()
    {
        Vector2 targetLook = new Vector2(
            LookInput.x * LookSensitivity.x,
            LookInput.y * LookSensitivity.y
        );

        smoothLookInput = Vector2.SmoothDamp(
            smoothLookInput,
            targetLook,
            ref lookVelocity,
            lookSmoothTime
        );

        currentLook -= smoothLookInput.y;
        currentLook = Mathf.Clamp(currentLook, -MaxLookAngle, MaxLookAngle);
        fpCamera.transform.localRotation = Quaternion.Euler(currentLook, 0f, 0f);

        transform.Rotate(Vector3.up * smoothLookInput.x);
    }

    void CameraUpdate()
    {
        float targetFOV = TargetCameraFov;

        if (Sprinting)
        {
            float speedRatio = CurrentSpeed / runSpeed;
            targetFOV = Mathf.Lerp(cameraFovNormal, cameraFovRunning, speedRatio);
        }

        fpCamera.Lens.FieldOfView = Mathf.Lerp(
            fpCamera.Lens.FieldOfView,
            targetFOV,
            cameraSmoothing * Time.deltaTime
        );
    }

    void StaminaUpdate()
    {
        if (Sprinting)
        {
            CurrentStamina -= staminaDrain * Time.deltaTime;

            if (CurrentStamina <= 0f)
            {
                CurrentStamina = 0f;
                IsRunning = false;
            }
        }
        else
        {
            if (CurrentStamina < maxStamina)
            {
                CurrentStamina += staminaRegen * Time.deltaTime;
                CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
            }
        }
    }

    void AnimationUpdate()
    {
        if (animator == null) return;

        float speed01 = Mathf.InverseLerp(0f, runSpeed, CurrentSpeed);

        animator.SetFloat("Speed", speed01);
        animator.SetBool("IsRunning", Sprinting);
        animator.SetBool("Grounded", IsGrounded);
    }


    void FootstepUpdate()
{
    if (footstepSource == null) return;

    bool shouldPlay = IsGrounded && CurrentSpeed > minSpeedToPlay;

    if (shouldPlay)
    {
        if (!footstepSource.isPlaying)
            footstepSource.Play();

        footstepSource.pitch = Sprinting ? runPitch : walkPitch;
    }
    else
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }
}

    public void TryJump()
    {
        if (Time.time - lastGroundedTime > groundedBufferTime) return;

        VerticalVelocity = 0f;
        VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y * gravityScale);

        if (animator != null)
            animator.SetTrigger("Jump");
    }

    public void SetSensitivity(float value)
    {
        LookSensitivity = new Vector2(value, value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    public void SetRunning(bool value)
    {
        IsRunning = value && CurrentStamina > 0f && MoveInput.sqrMagnitude > 0.01f;
    }
}
