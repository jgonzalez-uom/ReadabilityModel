using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    public HeatmapDisplay HeatmapDisplay;
    public HeatmapLogger HeatmapLogger;
    public void GenerateHeatmap()
    {
        //print(HeatmapLogger.points.Length);
        ////return;

        //string tmp = "";
        //foreach (var pt in HeatmapLogger.points)
        //{
        //    tmp += pt.ToString() + ";";
        //}
        //print(tmp);

        HeatmapDisplay.GenerateHeatmap(HeatmapLogger.points);
    }

}
