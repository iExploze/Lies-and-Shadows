using UnityEngine;
using UnityEngine.Rendering;

public class DarkMovement : MonoBehaviour
{
    [Header("Horizontal Movement (Forces)")]
    public float acceleration = 18f;      // camera-relative push when holding WASD
    public float maxHorizontalSpeed = 7f; // cap on XZ speed
    public float idleBrake = 10f;         // extra braking (XZ only) when no input
    public float turnResponsiveness = 1f; // >=1 keeps feel snappy; lower = more floaty

    [Header("Levitation")]
    public KeyCode levitateKey = KeyCode.Space;
    public float levitateUpAccel = 6f;    // upward accel while holding
    public float levitateUpMax = 2.2f;    // cap ascent speed while levitating
    public float dropDownSpeed = 14f;     // instant downward speed on release

    [Header("Camera-Relative Input")]
    [SerializeField] private Transform cameraTransform;   // leave empty to auto-grab Camera.main



    private Rigidbody rb;
    private Vector2 input;                // XZ input
    private bool levitating;
    private bool justReleased;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // don't spin the particle blob
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        // Lock cursor for FPS control (Cinemachine handles the rotation elsewhere)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // Read inputs
        input.x = Input.GetAxisRaw("Horizontal"); // A/D
        input.y = Input.GetAxisRaw("Vertical");   // W/S

        bool down = Input.GetKeyDown(levitateKey);
        bool hold = Input.GetKey(levitateKey);
        bool up = Input.GetKeyUp(levitateKey);

        if (down) { levitating = true; justReleased = false; }
        if (up) { levitating = false; justReleased = true; }
        if (!hold && !up) levitating = false;

    }

    void FixedUpdate()
    {
        // OLD (world-relative):
        // Vector3 wishDir = new Vector3(input.x, 0f, input.y);

        // NEW (camera-relative on XZ):
        Vector3 wishDir = Vector3.zero;
        if (cameraTransform != null)
        {
            // flatten camera forward/right onto XZ so looking up/down doesn't affect movement
            Vector3 camFwd = cameraTransform.forward;  camFwd.y = 0f;  camFwd.Normalize();
            Vector3 camRight = cameraTransform.right;  camRight.y = 0f; camRight.Normalize();

            wishDir = camRight * input.x + camFwd * input.y;  // A/D + W/S
        }
        else
        {
            // fallback to world-relative if no camera found
            wishDir = new Vector3(input.x, 0f, input.y);
        }
        float wishMag = Mathf.Clamp01(wishDir.magnitude);
        if (wishMag > 1e-4f) wishDir /= wishMag;

        // Apply horizontal force (no rotation of the body)
        // Turn responsiveness can bias force toward desired direction vs current velocity
        Vector3 horizontalVel = rb.linearVelocity; horizontalVel.y = 0f;
        Vector3 desiredVel = wishDir * maxHorizontalSpeed;
        Vector3 velDelta = desiredVel - horizontalVel;
        Vector3 accelDir = (wishDir * turnResponsiveness + velDelta.normalized * (1f - Mathf.Clamp01(turnResponsiveness))).normalized;

        if (wishMag > 0f)
        {
            // Push toward desired direction
            rb.AddForce(accelDir * acceleration, ForceMode.Acceleration);
        }
        else
        {
            // No input: apply gentle braking only on XZ to curb drift (without affecting Y)
            Vector3 brake = -horizontalVel.normalized * idleBrake;
            if (horizontalVel.sqrMagnitude < 0.01f) brake = Vector3.zero;
            rb.AddForce(new Vector3(brake.x, 0f, brake.z), ForceMode.Acceleration);
        }

        // Cap horizontal speed
        horizontalVel = rb.linearVelocity; horizontalVel.y = 0f;
        float hSpeed = horizontalVel.magnitude;
        if (hSpeed > maxHorizontalSpeed)
        {
            Vector3 capped = horizontalVel.normalized * maxHorizontalSpeed;
            rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
        }

        // Vertical: levitate or drop
        if (levitating)
        {
            rb.useGravity = false;
            // Upward acceleration
            rb.AddForce(Vector3.up * levitateUpAccel, ForceMode.Acceleration);

            // Cap upward speed while levitating
            float y = rb.linearVelocity.y;
            if (y > levitateUpMax) rb.linearVelocity = new Vector3(rb.linearVelocity.x, levitateUpMax, rb.linearVelocity.z);
        }
        else
        {
            if (justReleased)
            {
                rb.useGravity = true;
                // Instant strong drop
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -Mathf.Abs(dropDownSpeed), rb.linearVelocity.z);
                justReleased = false;
            }
            else
            {
                rb.useGravity = true; // normal gravity
            }
        }
    }


}
