using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class lightDetection : MonoBehaviour
{
    private Light spotLight;

    private GameObject[] currentPlayers;
    private readonly Dictionary<GameObject, bool> playerLightStates = new Dictionary<GameObject, bool>();

    // everything except IgnoreLightRaycast
    private int occlusionMask;

    void Start()
    {
        spotLight = GetComponent<Light>();
        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }

        occlusionMask = ~LayerMask.GetMask("IgnoreLightRaycast");
    }

    void Update()
    {
        currentPlayers = GameObject.FindGameObjectsWithTag("LightDetectable");

        // prune stale entries
        var stillHere = new HashSet<GameObject>(currentPlayers);
        foreach (var old in new List<GameObject>(playerLightStates.Keys))
            if (!stillHere.Contains(old))
                playerLightStates.Remove(old);

        foreach (var playerObj in currentPlayers)
        {
            if (!playerObj.activeInHierarchy) continue;

            bool isInLight = IsPlayerInCone(playerObj.transform);
            var hittable = playerObj.GetComponent<ILightHittable>();
            if (hittable == null)
            {
                Debug.LogWarning($"No ILightHittable on {playerObj.name}");
                continue;
            }

            bool wasInLight = playerSunState(playerObj);

            if (isInLight && !wasInLight)
            {
                hittable.OnLightEnter(spotLight);
                playerLightStates[playerObj] = true;
            }
            else if (isInLight && wasInLight)
            {
                hittable.OnLightStay(spotLight);
            }
            else if (!isInLight && wasInLight)
            {
                hittable.OnLightExit(spotLight);
                playerLightStates[playerObj] = false;
            }
            // else remained out of light → no-op
        }
    }

    private bool playerSunState(GameObject go) =>
        playerLightStates.ContainsKey(go) && playerLightStates[go];

    // Cast from the player toward the spotlight, with cone + range checks
    private bool IsPlayerInCone(Transform target)
    {
        if (spotLight == null || spotLight.type != LightType.Spot) return false;

        // Use collider center if possible for stability
        Collider col = target.GetComponent<Collider>();
        Vector3 targetPoint = col ? col.bounds.center : target.position;

        // 1) Cone + range checks (in light space)
        Vector3 toTargetFromLight = (targetPoint - spotLight.transform.position);
        float dist = toTargetFromLight.magnitude;
        if (dist > spotLight.range) return false;

        float halfAngle = spotLight.spotAngle * 0.5f;
        float angle = Vector3.Angle(spotLight.transform.forward, toTargetFromLight.normalized);
        if (angle > halfAngle) return false;

        // 2) Occlusion check — anything between player and light blocks it
        Vector3 dirToLight = (-toTargetFromLight).normalized; // from target → light
        Vector3 origin = targetPoint + dirToLight * 0.05f;    // nudge outside own collider

        // Optional: SphereCast to reduce flicker on thin geometry
        // if (Physics.SphereCast(origin, 0.05f, dirToLight, out var hitS, dist, occlusionMask))
        // {
        //     Debug.DrawLine(origin, hitS.point, Color.red);
        //     return false;
        // }

        if (Physics.Raycast(origin, dirToLight, out RaycastHit hit, dist, occlusionMask))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            return false; // hit an occluder before reaching light
        }

        Debug.DrawLine(origin, spotLight.transform.position, Color.green);
        return true;
    }
}
