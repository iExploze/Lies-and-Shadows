using System.Collections.Generic;
using UnityEngine;

public class PlayerLightDetection : MonoBehaviour
{
    // True if at least one tracked light currently hits the player.
    public bool InLight => _activeLights.Count > 0;

    // Backwards-compatible getter the rest of your code already calls.
    public bool inLight() => InLight;

    // Track which lights are currently shining on the player.
    private readonly HashSet<Light> _activeLights = new HashSet<Light>();

    // Optional: cache last state to fire events or do work only on change.
    private bool _lastInLight = false;

    // (Optional) simple event hook if you want to react to changes.
    public System.Action<bool> OnInLightChanged;

    // --- Call these from your light hit logic (triggers, raycasts, etc.) ---

    public void OnLightEnter(Light lightSource)
    {
        if (!lightSource) return;
        if (_activeLights.Add(lightSource))
            CheckStateChanged();
    }

    public void OnLightStay(Light lightSource)
    {
        if (!lightSource) return;
        // Ensure we keep tracking the light while it's still hitting the player.
        if (_activeLights.Add(lightSource))
            CheckStateChanged();
    }

    public void OnLightExit(Light lightSource)
    {
        if (!lightSource) return;
        if (_activeLights.Remove(lightSource))
            CheckStateChanged();
    }

    // --- Housekeeping ---

    void Update()
    {
        // Clean up destroyed lights so they don't keep you "stuck" in light.
        if (_activeLights.Count == 0) return;

        // Collect null/destroyed entries without allocating each frame.
        _toPrune.Clear();
        foreach (var l in _activeLights)
            if (!l) _toPrune.Add(l);

        if (_toPrune.Count > 0)
        {
            foreach (var l in _toPrune) _activeLights.Remove(l);
            CheckStateChanged();
        }
    }

    private readonly List<Light> _toPrune = new List<Light>(8);

    private void CheckStateChanged()
    {
        bool now = InLight;
        if (now == _lastInLight) return;
        _lastInLight = now;
        OnInLightChanged?.Invoke(now);
        // Debug.Log($"Player InLight = {now}");
    }
}
