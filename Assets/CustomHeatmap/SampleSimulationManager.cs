using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleSimulationManager : MonoBehaviour
{
    public HeatmapSender[] senders;
    public HeatmapLogger[] loggers;
    public int testIterations;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AutomaticHeatTest(testIterations));
    }


    IEnumerator AutomaticHeatTest(int iterations)
    {
        for (int n = 0; n < iterations; n++)
        {
            foreach (var s in senders)
            {
                s.CameraViewHeat();
                yield return null;
            }
        }


        //foreach (var l in loggers)
        //{
            //l.SaveFile();
        //}
    }
}
