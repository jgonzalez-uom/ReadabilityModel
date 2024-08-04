using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class HeatmapReceiver : MonoBehaviour
{
    //public ParticleSystem heatParticles;
    //public int maxParticles;
    //private Vector3[] hitPoints;
    private int currentInd = 0;
    public GameObject referencePoint; 


    private HeatmapLogger logger;
    //private void Start()
    //{
    //    //hitPoints = new Vector3[(int)maxParticles];
    //    //currentInd = 0;
    //}

    //private void LateUpdate()
    //{

    //}

    public void SetLogger(HeatmapLogger lg)
    {
        logger = lg;
    }

    public void AddPoint(Vector3 point)
    {
        Vector3 localPoint = referencePoint.transform.InverseTransformPoint(point);
        //Vector3 localPoint = point - referencePoint.transform.position;

        logger.LogPoint(localPoint);
    }
}
