using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    public HeatmapDisplay HeatmapDisplay;
    public HeatmapLogger HeatmapLogger;
    public void GenerateHeatmap()
    {
        HeatmapDisplay.GenerateHeatmap(HeatmapLogger.points);
    }

}
