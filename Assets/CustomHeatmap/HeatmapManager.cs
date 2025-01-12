using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapManager : MonoBehaviour
{
    public HeatmapDisplay HeatmapDisplay;
    public HeatmapLogger HeatmapLogger;


    private System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    private void Start()
    {


        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
    }

    public void GenerateHeatmap()
    {

        HeatmapDisplay.GenerateHeatmap(HeatmapLogger.points.ToArray());
    }

    public IEnumerator HeatmapSetup(bool trimBox = false)
    {
        yield return StartCoroutine(HeatmapDisplay.PrepareHeatmap(trimBox));
    }

    public IEnumerator LoadCurrentPointsIntoMatrix()
    {
        Vector3[] tempArr = new Vector3[HeatmapLogger.points.Count];

        for (int i = 0; i < tempArr.Length; i++)
        {
            tempArr[i] = HeatmapLogger.points[i];
        }
        yield return null;

        yield return StartCoroutine(HeatmapDisplay.LoadDictionary(tempArr, HeatmapDisplay.heatmapBoundBox));
    }

    public IEnumerator DisplayHeatmap()
    {
        yield return StartCoroutine(HeatmapDisplay.DisplayHeatmap());
    }

}
