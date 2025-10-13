using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LightMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float sprintMultiplier = 1.7f;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Jump")]
    public float jumpHeight = 1.6f;   // meters
    public float gravity = -20f;      // negative value

    [Header("Mouse Look")]
    public Transform cameraTransform; // Drag your child Camera here
    public float mouseSensitivity = 120f;
    public float verticalLookLimit = 85f;

    [SerializeField] private Transform playerDarkForm; // PlayerDarkForm
    [SerializeField] private Transform lightHitbox;

    private CharacterController controller;
    private float xRot;               // camera pitch
    private float verticalVelocity;   // y velocity only

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMoveAndJump();
        mirrorDarkTransform();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -verticalLookLimit, verticalLookLimit);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }

    void mirrorDarkTransform() 
    {
        // --- Mirror root and dark form to this light form pose ---
        if (playerDarkForm)
        {
            playerDarkForm.SetPositionAndRotation(transform.position, transform.rotation);
            lightHitbox.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }

    void HandleMoveAndJump()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S
        Vector3 input = new Vector3(h, 0f, v).normalized;

        float speed = moveSpeed * (Input.GetKey(sprintKey) ? sprintMultiplier : 1f);

        Vector3 horizontalMove = transform.TransformDirection(input) * speed;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(2f * -gravity * jumpHeight);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = new Vector3(horizontalMove.x, verticalVelocity, horizontalMove.z);

        controller.Move(move * Time.deltaTime);
    }
}
