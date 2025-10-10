using UnityEngine;

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

    [Header("Camera (Orbit, no zoom)")]
    public Transform cameraTransform;
    public Vector3 cameraTargetOffset = new Vector3(0f, 1.4f, 0f);
    public float cameraDistance = 5f;
    public float yawSpeed = 180f;         // Mouse X
    public float pitchSpeed = 120f;       // Mouse Y
    public float minPitch = -20f;
    public float maxPitch = 70f;

    private Rigidbody rb;
    private Vector2 input;                // XZ input
    private bool levitating;
    private bool justReleased;

    // orbit state
    private float yaw;
    private float pitch = 15f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // don’t spin the particle blob

        // Optional cursor lock for consistent orbit
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // Initialize camera yaw from current facing projection (not required since we don't rotate body)
        Vector3 fwd = transform.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude > 1e-4f) yaw = Quaternion.LookRotation(fwd).eulerAngles.y;
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

        // Orbit camera (no zoom)
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        yaw += mx * yawSpeed * Time.deltaTime;
        pitch -= my * pitchSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void FixedUpdate()
    {
        // Camera-relative basis for forces
        Vector3 camF = Vector3.forward;
        Vector3 camR = Vector3.right;
        if (cameraTransform)
        {
            camF = cameraTransform.forward; camF.y = 0f; camF.Normalize();
            camR = cameraTransform.right; camR.y = 0f; camR.Normalize();
        }

        Vector3 wishDir = (camR * input.x + camF * input.y);
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

    void LateUpdate()
    {
        if (!cameraTransform) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 target = transform.position + cameraTargetOffset;
        Vector3 camPos = target - rot * Vector3.forward * cameraDistance;

        cameraTransform.SetPositionAndRotation(camPos, rot);
    }
}
