using UnityEngine;

public class PlayerLightStateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject playerLightForm;
    [SerializeField] private Camera LightCamera;

    [SerializeField] private GameObject playerDarkForm;
    [SerializeField] private Camera DarkCamera;

    private PlayerLightDetection playerLightDetection;

    private bool inLight = true;

    void Start()
    {
        playerLightDetection = this.GetComponent<PlayerLightDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerLightDetection.inLight());
        if (playerLightDetection.inLight())
        {
            playerDarkForm.SetActive(false);
            playerLightForm.SetActive(true);
            inLight = true;

            SyncFormsPose();
        }
        else 
        {
            playerDarkForm.SetActive(true);
            playerLightForm.SetActive(false);
            inLight = false;

            SyncFormsPose();
        }
    }

    private void SyncFormsPose()
    {
        if (playerLightForm == null || playerDarkForm == null) return;

        Transform src = inLight ? playerLightForm.transform : playerDarkForm.transform;
        Transform dst = inLight ? playerDarkForm.transform : playerLightForm.transform;

        // Copy world pose
        Vector3 pos = src.position;
        Quaternion rot = src.rotation;

        // If the destination has a Rigidbody, set rigidbody pose & zero velocities
        if (dst.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.position = pos;
            rb.rotation = rot;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            dst.SetPositionAndRotation(pos, rot);
        }
    }
}
