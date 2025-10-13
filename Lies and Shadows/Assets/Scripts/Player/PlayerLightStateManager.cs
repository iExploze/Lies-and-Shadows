using UnityEngine;

public class PlayerLightStateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject playerLightForm;
    [SerializeField] private Camera LightCamera;

    [SerializeField] private GameObject playerDarkForm;
    [SerializeField] private Camera DarkCamera;

    private PlayerLightDetection playerLightDetection;

    void Start()
    {
        playerLightDetection = this.GetComponentInChildren<PlayerLightDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerLightDetection.inLight() && !playerLightForm.activeSelf)
        {
            playerLightForm.SetActive(true);
            playerDarkForm.SetActive(false);
        } 
        else if(!playerLightDetection.inLight() && !playerDarkForm.activeSelf)
        {
            playerDarkForm.SetActive(true);
            playerLightForm.SetActive(false);
        }
    }
}
