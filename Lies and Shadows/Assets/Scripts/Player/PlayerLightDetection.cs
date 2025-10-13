using System.Collections.Generic;
using UnityEngine;

public class PlayerLightDetection : MonoBehaviour, ILightHittable
{
    // Track all current lights
    private HashSet<Light> currentLights = new HashSet<Light>();
    private bool isInLight = false;

    public void OnLightEnter(Light lightSource)
    {
        currentLights.Add(lightSource);
    }

    public void OnLightExit(Light lightSource)
    {
        currentLights.Remove(lightSource);
    }

    public void OnLightStay(Light lightSource)
    {
        isInLight = true;
    }

    void Update() 
    {
        if (currentLights.Count == 0)
        {
            isInLight = false;
        }
        else 
        {
            isInLight = true;
        }
    }

    public bool inLight() 
    {
        return isInLight;
    }
}
