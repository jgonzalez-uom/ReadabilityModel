using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class HeatmapReceiver : MonoBehaviour
{
    public GameObject referencePoint; 


    private HeatmapLogger logger;

    public void SetLogger(HeatmapLogger lg)
    {
        logger = lg;
    }

    public void AddPoint(Vector3 point)
    {
        Vector3 localPoint = referencePoint.transform.InverseTransformPoint(point);
        logger.LogPoint(localPoint);
    }
}
