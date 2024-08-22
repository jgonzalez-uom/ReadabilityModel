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
        //print(HeatmapLogger.points.Length);
        ////return;

        //string tmp = "";
        //foreach (var pt in HeatmapLogger.points)
        //{
        //    tmp += pt.ToString() + ";";
        //}
        //print(tmp);

        HeatmapDisplay.GenerateHeatmap(HeatmapLogger.points.ToArray());
    }

    public IEnumerator HeatmapSetup(bool trimBox = false)
    {
        yield return StartCoroutine(HeatmapDisplay.PrepareHeatmap(trimBox));
    }

    public IEnumerator LoadCurrentPointsIntoMatrix()
    {
        Vector3[] tempArr = new Vector3[HeatmapLogger.points.Count];
        //Vector3[] tempArr = new Vector3[HeatmapLogger.points.Length];

        for (int i = 0; i < tempArr.Length; i++)
        {
            tempArr[i] = HeatmapLogger.points[i];
            //Debug.Log(tempArr[i].ToString());
        }

        //yield return StartCoroutine(HeatmapDisplay.LoadDictionary(HeatmapLogger.points, HeatmapDisplay.heatmapBoundBox));
        yield return StartCoroutine(HeatmapDisplay.LoadDictionary(tempArr, HeatmapDisplay.heatmapBoundBox));
    }

    public IEnumerator DisplayHeatmap()
    {
        yield return StartCoroutine(HeatmapDisplay.DisplayHeatmap());
    }

    //public IEnumerator AddPointsFromFiles(string[] files)
    //{


    //    foreach (string file in files)
    //    {
    //        HeatmapLogger.ClearPointCache();

    //        HeatmapLogger.LoadFileName(file);

    //        if (watch.ElapsedTicks > tickBudget)
    //        {
    //            yield return null;
    //            watch.Restart();
    //        }
    //    }
    //}
}
