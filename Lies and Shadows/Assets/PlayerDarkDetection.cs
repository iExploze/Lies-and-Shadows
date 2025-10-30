using UnityEngine;

public class PlayerDarkDetection : MonoBehaviour, ILightHittable
{
    // Track all current lights
    [SerializeField] private PlayerLightStateManager playerLightStateManager;

    private bool inLight = false;

    void Awake() 
    {
        inLight = false;
    }

    void Start() 
    {
        if (playerLightStateManager == null)
            playerLightStateManager = FindAnyObjectByType<PlayerLightStateManager>();
    }
    public void OnLightEnter(Light lightSource)
    {
        playerLightStateManager.updateLight(true);
    }

    public void OnLightExit(Light lightSource)
    {

    }

    public void OnLightStay(Light lightSource)
    {
        inLight = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
