using System.Collections.Generic;
using UnityEngine;

public class PlayerLightDetection : MonoBehaviour, ILightHittable
{
    // Track all current lights
    [SerializeField] private PlayerLightStateManager playerLightStateManager;

    // Track all current lights
    private HashSet<Light> currentLights = new HashSet<Light>();


    void Start()
    {
        if (playerLightStateManager == null) 
        {
            playerLightStateManager = FindFirstObjectByType<PlayerLightStateManager>();
        }
    }

    public void OnLightEnter(Light lightSource)
    {
        // Add this light
        bool wasInLight = currentLights.Count > 0;
        currentLights.Add(lightSource);

        if (!wasInLight && currentLights.Count > 0)
        {
            // Just entered *any* light
            //Debug.Log("Entered light");
        }
    }

    public void OnLightStay(Light lightSource)
    {
        // Optional: refresh logic (no-op if not needed)
    }

    public void OnLightExit(Light lightSource)
    {
        currentLights.Remove(lightSource);
    }

    void Update()
    {
        if (currentLights.Count == 0)
        {
            // Truly out of *all* lights
            playerLightStateManager.updateLight(false);
        }
        else 
        {
            playerLightStateManager.updateLight(true);
        }
    }
}
