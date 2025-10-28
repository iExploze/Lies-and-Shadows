using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerStateManager : MonoBehaviour
{
    [Header("Body")]
    [SerializeField] private GameObject LightForm;
    [SerializeField] private GameObject DarkForm;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera lightCamera;
    [SerializeField] private CinemachineCamera darkCamera;

    [Header("Light Detection")]
    [SerializeField] private PlayerLightDetection playerLightDetection;

    [Header("Dark Detection")]
    [SerializeField] private PlayerDarkDetection playerDarkDetection;

    [SerializeField] private Rigidbody darkRB;

    private enum FormState { LightMode, DarkMode }
    private FormState currentState = FormState.LightMode;

    void Start()
    {
        playerLightDetection = LightForm.GetComponent<PlayerLightDetection>();
        playerDarkDetection = DarkForm.GetComponent<PlayerDarkDetection>();

        if (darkRB != null)
        {
            darkRB = DarkForm.GetComponentInChildren<Rigidbody>();
        }
    }

    public void SwtichToShadowForm()
    {
        if (currentState == FormState.DarkMode) return;

        // move/activate dark form
        darkRB.isKinematic = true;

        LightForm.SetActive(false);
        DarkForm.transform.position = LightForm.transform.position;
        DarkForm.transform.rotation = LightForm.transform.rotation;

        darkRB.isKinematic = false;

        DarkForm.SetActive(true);

        // sync yaw light -> dark camera, then priorities
        SyncYaw_OrbitalPanAgnostic(lightCamera, darkCamera);
        UpdateCameraPriorities(light: 0, dark: 1);

        currentState = FormState.DarkMode;
    }

    public void SwtichToLightForm()
    {
        if (currentState == FormState.LightMode) return;

        // move/activate light form
        DarkForm.SetActive(false);
        LightForm.transform.position = DarkForm.transform.position;
        LightForm.transform.rotation = LightForm.transform.rotation;

        LightForm.SetActive(true);

        // sync yaw dark -> light camera, then priorities
        SyncYaw_OrbitalPanAgnostic(darkCamera, lightCamera);
        UpdateCameraPriorities(light: 1, dark: 0);

        currentState = FormState.LightMode;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            if (currentState == FormState.LightMode) SwtichToShadowForm();
            else SwtichToLightForm();
    }

    private void UpdateCameraPriorities(int light, int dark)
    {
        if (lightCamera) lightCamera.Priority = light;
        if (darkCamera) darkCamera.Priority = dark;
    }

    // --------- CM3 yaw-only sync between OrbitalFollow and PanTilt ----------
    static float GetWorldYaw(Transform t)
    {
        Vector3 f = t.forward; f.y = 0f;
        if (f.sqrMagnitude < 1e-6f) return t.eulerAngles.y;
        f.Normalize();
        return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
    }

    private void SyncYaw_OrbitalPanAgnostic(CinemachineCamera source, CinemachineCamera target)
    {
        if (!source || !target) return;

        float desiredYaw = GetWorldYaw(source.transform);
        float currentYaw = GetWorldYaw(target.transform);
        float delta = Mathf.DeltaAngle(currentYaw, desiredYaw); // shortest delta to desired

        // If target is PanTilt: adjust Pan only
        if (target.TryGetComponent(out CinemachinePanTilt panTilt))
        {
            panTilt.PanAxis.Value += delta;  // yaw only
            return;                          // leave panTilt.TiltAxis alone
        }

        // If target is OrbitalFollow: adjust HorizontalAxis only
        if (target.TryGetComponent(out CinemachineOrbitalFollow orbital))
        {
            orbital.HorizontalAxis.Value += delta; // yaw only
            return;                                // leave vertical alone
        }

        // Fallback: set transform yaw directly
        var e = target.transform.eulerAngles;
        e.y = Mathf.Repeat(desiredYaw + 360f, 360f);
        target.transform.eulerAngles = e;
    }
    public bool isLightState() 
    {
        if (currentState == FormState.LightMode)
            return true;
        else return false;
    }
}
