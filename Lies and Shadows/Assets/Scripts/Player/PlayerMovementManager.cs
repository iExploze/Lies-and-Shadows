using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Children (under PlayerManager)")]
    [SerializeField] private GameObject playerLightForm; // has CharacterController + LightMovement
    [SerializeField] private GameObject playerDarkForm;  // has Rigidbody + DarkMovement

    [Header("State")]
    [SerializeField] private bool inLight = true;

    CharacterController lightCC;
    Rigidbody darkRB;

    void Awake()
    {
        lightCC = playerLightForm.GetComponent<CharacterController>();
        darkRB = playerDarkForm.GetComponent<Rigidbody>();

        // Start aligned to root
        AlignChildToRoot(playerLightForm.transform);
        AlignChildToRoot(playerDarkForm.transform);

        ApplyForm(inLight, snapPose: true);
    }

    void Update()
    {
        // Manual toggle to test
        if (Input.GetKeyDown(KeyCode.L))
            SetForm(!inLight);
    }

    void LateUpdate()
    {
        // 1) The active form moves itself (Light: Update via CC, Dark: FixedUpdate via RB)
        // 2) Copy active form's pose -> root (authoritative)
        if (inLight)
            CopyWorldPose(playerLightForm.transform, transform);
        else
            CopyWorldPose(playerDarkForm.transform, transform);

        // 3) Mirror inactive form to the root so swap is seamless
        if (inLight)
            MirrorInactive(playerDarkForm.transform, isRigidbody: true);
        else
            MirrorInactive(playerLightForm.transform, isCharacterController: true);
    }

    public void SetForm(bool toLight)
    {
        if (inLight == toLight) return;
        inLight = toLight;
        ApplyForm(inLight, snapPose: true);
    }

    private void ApplyForm(bool lightOn, bool snapPose)
    {
        // Toggle children
        playerLightForm.SetActive(lightOn);
        playerDarkForm.SetActive(!lightOn);

        // Gate movement components
        if (lightCC) lightCC.enabled = lightOn;

        if (darkRB)
        {
            // Inactive RB must be kinematic so we can teleport it cleanly
            darkRB.isKinematic = lightOn;      // when light is ON, dark is OFF -> kinematic = true
            darkRB.linearVelocity = Vector3.zero;
            darkRB.angularVelocity = Vector3.zero;
        }

        if (snapPose)
        {
            AlignChildToRoot(playerLightForm.transform);
            AlignChildToRoot(playerDarkForm.transform);
        }
    }

    private void MirrorInactive(Transform t, bool isRigidbody = false, bool isCharacterController = false)
    {
        if (!t) return;

        if (isCharacterController)
        {
            var cc = t.GetComponent<CharacterController>();
            if (cc)
            {
                // keep CC disabled while inactive and hard-set pose
                bool wasEnabled = cc.enabled;
                cc.enabled = false;
                AlignChildToRoot(t);
                cc.enabled = false;
                return;
            }
        }

        if (isRigidbody)
        {
            var rb = t.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true; // inactive
                rb.position = transform.position;
                rb.rotation = transform.rotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                return;
            }
        }

        AlignChildToRoot(t);
    }

    private void AlignChildToRoot(Transform child)
    {
        if (!child) return;
        child.SetPositionAndRotation(transform.position, transform.rotation);
    }

    private void CopyWorldPose(Transform src, Transform dst)
    {
        if (!src || !dst) return;
        dst.SetPositionAndRotation(src.position, src.rotation);
    }
}
