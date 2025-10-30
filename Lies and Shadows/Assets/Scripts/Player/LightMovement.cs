using UnityEngine;

// Make sure your Main Camera has a CinemachineBrain, and your FPS VCam looks at a Head/CameraPivot.
// This script ONLY handles movement; Cinemachine handles look (Pan Tilt / POV).

[RequireComponent(typeof(CharacterController))]
public class LightMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float sprintMultiplier = 1.7f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Jump / Gravity")]
    [Tooltip("Use a negative number (e.g., -20)")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.6f;   // in meters

    [Header("Camera Reference (optional)")]
    [Tooltip("Leave empty to auto-use Camera.main (recommended with Cinemachine).")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private float verticalVelocity;   // y-only velocity accumulator

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Lock cursor for FPS control (Cinemachine handles the rotation elsewhere)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnsureCameraRef();
    }

    void Update()
    {
        // In case the main camera changes (scene reloads, etc.)
        if (cameraTransform == null) EnsureCameraRef();

        HandleMoveAndJump();
    }

    private void EnsureCameraRef()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void HandleMoveAndJump()
    {
        // Ground snap
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // Raw is snappy; switch to GetAxis for smoothing if desired
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D or left/right
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S or up/down

        Vector3 input = new Vector3(inputX, 0f, inputZ);
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = moveSpeed * (Input.GetKey(sprintKey) ? sprintMultiplier : 1f);

        // Movement relative to camera facing (Cinemachine controls this)
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right   = cameraTransform.right;

            // Keep it horizontal
            forward.y = 0f;
            right.y   = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 horizontalMove = (forward * input.z + right * input.x) * speed;

        // Jump
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            // v = sqrt(2 * g * h); with g negative, use -gravity
            verticalVelocity = Mathf.Sqrt(2f * -gravity * jumpHeight);
        }

        // Apply gravity over time
        verticalVelocity += gravity * Time.deltaTime;

        // Final velocity vector (units/second)
        Vector3 velocity = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);

        // CharacterController.Move expects displacement (units), not velocity
        controller.Move(velocity * Time.deltaTime);
    }
}
