using UnityEngine;

public class LightFormDetection : MonoBehaviour, ILightHittable
{
    [SerializeField] private PlayerLightDetection playerLightDetection;
    public void OnLightEnter(Light lightSource)
    {
        playerLightDetection.OnLightEnter(lightSource);
    }

    public void OnLightExit(Light lightSource)
    {
        playerLightDetection.OnLightExit(lightSource);
    }

    public void OnLightStay(Light lightSource)
    {
        playerLightDetection.OnLightStay(lightSource);
    }
}
