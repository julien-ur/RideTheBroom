using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatControl : MonoBehaviour {

    private Dictionary<int, float> heatSources;
    private List<int> heatSourceIds;
    private float defaultHeatPercent = 0;

    private void Start()
    {
        heatSources = new Dictionary<int, float>();
        heatSourceIds = new List<int>();
    }

    public void SetDefaultHeatPercent(float h)
    {
        defaultHeatPercent = h;
    }

    public void AddHeatSource(int instanceId, float heatPercent)
    {
        heatSources.Add(instanceId, heatPercent);
        heatSourceIds.Add(instanceId);
    }

    public void RemoveHeatSource(int instanceId)
    {
        heatSources.Remove(instanceId);
        heatSourceIds.Remove(instanceId);
    }

    public float GetCurrentHeatPercent()
    {
        int sourceCount = heatSourceIds.Count;
        float heatPercent;

        if (sourceCount == 0)
        {
            heatPercent = defaultHeatPercent;
        }
        else
        {
            heatSources.TryGetValue(heatSourceIds[sourceCount-1], out heatPercent);
        }
        Debug.Log(heatPercent);
        return heatPercent;
    }
}
