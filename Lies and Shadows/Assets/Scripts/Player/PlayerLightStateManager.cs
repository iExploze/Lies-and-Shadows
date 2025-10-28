using UnityEngine;

public class PlayerLightStateManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Light Parts")]
    [SerializeField] private PlayerLightDetection lightDetect;
    [SerializeField] private PlayerDarkDetection darkDetect;

    private PlayerStateManager PlayerStateManager;

    private bool inLight;


    void Start()
    {
        inLight = true;

        PlayerStateManager = GetComponent<PlayerStateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inLight && !PlayerStateManager.isLightState())
        {
            PlayerStateManager.SwtichToLightForm();
        }
        else if (!inLight && PlayerStateManager.isLightState())
        {
            PlayerStateManager.SwtichToShadowForm();
        }
    }

    public void updateLight(bool change) 
    {
        inLight = change;
    }

    public bool isInLight() 
    {
        return inLight;
    }
}
