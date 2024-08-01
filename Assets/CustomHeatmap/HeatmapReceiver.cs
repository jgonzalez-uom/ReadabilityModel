using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class HeatmapReceiver : MonoBehaviour
{
    public ParticleSystem heatParticles;
    //public int maxParticles;
    //private Vector3[] hitPoints;
    private int currentInd = 0;


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
        Vector3 localPoint = heatParticles.transform.parent.InverseTransformPoint(point);

        logger.LogPoint(localPoint);
    }

    public void DisplayPoint(Vector3 point)
    {
        Vector3 localPoint = heatParticles.transform.parent.InverseTransformPoint(point);
        //print(point + " vs " + localPoint);

        //ParticleSystemSimulationSpace temp = heatParticles.main.simulationSpace;
        //heatParticles.simulationSpace = ParticleSystemSimulationSpace.World;
        //Vector3 localPoint = point;

        var particles = new ParticleSystem.Particle[heatParticles.main.maxParticles];
        var currentAmount = heatParticles.GetParticles(particles);

        //// Change only the particles that are alive
        //for (int i = 0; i < currentAmount; i++)
        //{
        //    particles[i].position = XYZ;
        //}

        particles[currentInd].position = localPoint;

        // Apply the particle changes to the Particle System
        heatParticles.SetParticles(particles, currentAmount);

        logger.LogPoint(localPoint);

        currentInd++;

        //heatParticles.simulationSpace = temp;
    }
}
