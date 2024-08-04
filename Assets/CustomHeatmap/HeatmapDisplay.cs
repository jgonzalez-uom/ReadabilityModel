using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using System.IO;
using System;
using UnityEditor.TerrainTools;

public class HeatmapDisplay : MonoBehaviour
{
    public Collider heatmapBoundBox;
    //private bool readyToDisplay = false;
    public LayerMask layersHit;
    public GameObject referencePoint;

    [Header("Particle Settings")]
    public float particleSize = 1.0f;
    public float particleSpacing = 0.0f;
    public Material particleMaterial;
    public Gradient heatmapColors;
    public bool createNewSystemAlways = false;
    public ParticleSystem particleSys;



    //private Vector3 boundDimensions = Vector3.zero;
    //private Vector3Int gridDimensions = Vector3Int.zero;
    private int particleCount = 0;
    //private int[,,] dataGrid;
    private Dictionary<Vector3Int, int> gridValues = new Dictionary<Vector3Int, int>();
    //private Vector3 minPoint;
    //private Vector3 maxPoint;
    private int maxHeat = 0;
    private Vector3 localMinPoint = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //if (heatmapBoundBox == null && this.gameObject.TryGetComponent<Collider>(out heatmapBoundBox))
        //{
        //    readyToDisplay = true;
        //}

        //boundDimensions = calculateBoundDimensions(heatmapBoundBox);
    }

    public void GenerateHeatmap(Vector3[] points)
    {


        //Vector3[] pts = new Vector3[points.Length];
        
        //for (int i = 0; i < points.Length; i++) 
        //    pts[i] = points[i];

        StartCoroutine(GenerateHeatmapCoroutine(points));
    }

    IEnumerator GenerateHeatmapCoroutine(Vector3[] points)
    {
        TimeSpan timeA, timeB;
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        TrimHeatmapBoundBox();
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Box trimmed: " + (timeB - timeA).TotalSeconds);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        CreateDictionary(heatmapBoundBox);
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Dictionary Created: " + (timeB - timeA).TotalSeconds);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        LoadDictionary(points, heatmapBoundBox);
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Dictionary loaded: " + (timeB - timeA).TotalSeconds);

        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        particleSys = CreateParticleSystem(particleCount, heatmapBoundBox);
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Particle System created: " + (timeB - timeA).TotalSeconds);

        timeA = new TimeSpan(System.DateTime.Now.Ticks);

        //system.Play();

        //system.Emit(particleCount);
        //particleSystem.

        yield return null;

        Debug.Log("Particles Emitted: " + particleCount);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);

        var particles = new ParticleSystem.Particle[particleSys.main.maxParticles];
        particleSys.GetParticles(particles);

        int index = 0;
        float maxTime = 1 / 60f;

        //foreach (var p in points)
        //{
        //    DisplayPoint(particles, index++, p, 0);
        //}

        foreach (KeyValuePair<Vector3Int, int> pair in gridValues)
        {
            Vector3 finalPos = ((Vector3)pair.Key * (particleSize * 2 + particleSpacing))
                + referencePoint.transform.TransformPoint(localMinPoint);
            finalPos = particleSys.transform.InverseTransformPoint(finalPos);

            //finalPos = (Vector3)pair.Key;

            float colorValue = 0;

            if (maxHeat > 0)
            {
                colorValue = (float)pair.Value / (float)maxHeat;
            }


            //DisplayPoint(particles, index, referencePoint.transform.TransformPoint(finalPos), colorValue);
            DisplayPoint(particles, index, finalPos, colorValue);
            index++;

            if (Time.timeScale > maxTime)
            {
                yield return null;
            }
        }

        particleSys.SetParticles(particles);

        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Points Displayed. " + (timeB - timeA).TotalSeconds);

        yield return null;
    }


    private void DisplayPoint(Particle[] particles, int particleIndex, Vector3 point, float gradientColor)
    {
        particles[particleIndex].position = point;
        particles[particleIndex].startColor = heatmapColors.Evaluate(gradientColor);
        Debug.Log("Particle " + particleIndex + " set to " 
            + particles[particleIndex].position.ToSafeString() 
            + " with color value " + particles[particleIndex].startColor.ToString());
    }

    private void LoadDictionary(Vector3[] points, Collider collider)
    {
        //CreateDictionary(heatmapBoundBox);

        Debug.DebugBreak();

        foreach (var point in points)
        {
            Vector3 vec = (referencePoint.transform.TransformPoint(point) - collider.bounds.min) / 
                (particleSize * 2 + particleSpacing);
            Vector3Int index = new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));

            if (!gridValues.ContainsKey(index))
            {
                Debug.LogWarning("INDEX NOT FOUND: " + index.ToString() + "\nLOCAL POSITION: " + point.ToString());
                
                continue;
            }

            gridValues[index]++;

            if (gridValues[index] > maxHeat)
            {
                maxHeat = gridValues[index];
            }

            
        }
    }

    private void TrimHeatmapBoundBox()
    {
        //boundDimensions = calculateBoundDimensions(heatmapBoundBox);

        RaycastHit hit;
        Vector3 tempOrigin = Vector3.zero;
        Vector3 raycastThickness = heatmapBoundBox.bounds.extents * 2;
        Vector3 maxPoint = heatmapBoundBox.bounds.center;
        Vector3 minPoint = heatmapBoundBox.bounds.center;

        //Positive max
        tempOrigin = transform.right * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.x, -transform.right,out hit, Mathf.Infinity, layersHit))
        {
            maxPoint.x = hit.point.x;
        }

        tempOrigin = transform.up * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.y, -transform.up, out hit, Mathf.Infinity, layersHit))
        {
            maxPoint.y = hit.point.y;
        }

        tempOrigin = transform.forward * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.z, -transform.forward, out hit, Mathf.Infinity, layersHit))
        {
            maxPoint.z = hit.point.z;
        }

        //Negative min

        tempOrigin = -transform.right * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.x, transform.right, out hit, Mathf.Infinity, layersHit))
        {
            minPoint.x = hit.point.x;
        }

        tempOrigin = -transform.up * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.y, transform.up, out hit, Mathf.Infinity, layersHit))
        {
            minPoint.y = hit.point.y;
        }

        tempOrigin = -transform.forward * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.z, transform.forward, out hit, Mathf.Infinity, layersHit))
        {
            minPoint.z = hit.point.z;
        }

        //Calculate
        heatmapBoundBox.bounds.SetMinMax(minPoint, maxPoint);
    }

    private void CreateDictionary(Collider boundingBox)
    {
        gridValues.Clear();

        maxHeat = 0;
        Vector3Int gridDimensions = Vector3Int.zero;

        gridDimensions.x = Mathf.RoundToInt((boundingBox.bounds.extents.x * 2 - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;
        gridDimensions.y = Mathf.RoundToInt((boundingBox.bounds.extents.y * 2 - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;
        gridDimensions.z = Mathf.RoundToInt((boundingBox.bounds.extents.z * 2 - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;

        Vector3 margins = ((boundingBox.bounds.extents * 2) - ((Vector3)gridDimensions * particleSize * 2) - ((gridDimensions - Vector3.one) * particleSpacing)) / 2;

        //Vector3 margins = new Vector3(
        //    ((heatmapBoundBox.bounds.extents.x * 2) - (gridDimensions.x * particleSize * 2) - ((gridDimensions.x - 1) * particleSpacing))/2,
        //    ((heatmapBoundBox.bounds.extents.y * 2) - (gridDimensions.y * particleSize * 2) - ((gridDimensions.y - 1) * particleSpacing))/2,
        //    ((heatmapBoundBox.bounds.extents.z * 2) - (gridDimensions.z * particleSize * 2) - ((gridDimensions.z - 1) * particleSpacing))/2);


        Vector3 minParticlePos = boundingBox.bounds.min + margins;
        localMinPoint = referencePoint.transform.InverseTransformPoint(minParticlePos);
        Debug.Log(minParticlePos.ToSafeString());
        Debug.Log("Grid Dimensions: " + gridDimensions.ToSafeString());

        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                for (int z = 0; z < gridDimensions.z; z++)
                {
                    
                    Vector3 tempPos = minParticlePos
                        + Vector3.right * x * (particleSize * 2 + particleSpacing)
                        + Vector3.up * y * (particleSize * 2 + particleSpacing)
                        + Vector3.forward * z * (particleSize * 2 + particleSpacing);

                    Collider[] results = new Collider[1];
                    if (Physics.OverlapSphereNonAlloc(tempPos, particleSize, results, layersHit) > 0)
                    {
                        //Vector3 temp = boundingBox.transform.InverseTransformPoint(tempPos);
                        //Vector3Int vector3Int = new Vector3Int(Mathf.FloorToInt(temp.x), Mathf.FloorToInt(temp.y), Mathf.FloorToInt(temp.z));
                        Vector3Int vector3Int = new Vector3Int(x, y, z);
                        //Debug.Log(string.Format("Point found to collide with object ({0},{1},{2}): {3}", x, y, z, (tempPos.ToString())));
                        gridValues.Add(vector3Int, 0);
                        particleCount++;

                        if (particleCount > Constants.maxParticleCount)
                        {
                            Debug.LogError("ERROR, MAX PARTICLE SIZE REACHED");
                            return;
                        }
                    }
                    else
                    {
                        //Debug.Log("Point is not touching the object: " + (tempPos.ToString()));
                        //GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //temp.transform.position = tempPos;
                        //temp.transform.localScale = particleSize * Vector3.one;
                    }
                }
            }
        }
    }

    private ParticleSystem CreateParticleSystem(int partCount, Collider collider)
    {
        if (referencePoint.TryGetComponent<ParticleSystem>(out ParticleSystem output))
            Destroy(output);
            

        ParticleSystem newParticleSystem = referencePoint.AddComponent<ParticleSystem>();

        EmissionModule emission = newParticleSystem.emission;
        emission.enabled = true;
        emission.rateOverDistance = 0;
        emission.rateOverTime = 0;
        Burst[] bursts = new Burst[1];
        bursts[0].count = partCount;
        bursts[0].time = 0;
        emission.SetBursts(bursts);


        ShapeModule shape = newParticleSystem.shape;
        shape.enabled = false;
        //shape.shapeType = ParticleSystemShapeType.Sphere;
        //shape.radius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y, collider.bounds.extents.z);

        ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.allowRoll = false;
        renderer.alignment = ParticleSystemRenderSpace.Facing;

        MainModule main = newParticleSystem.main;
        main.loop = false;
        //main.duration = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        //main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSize = particleSize;
        main.startLifetime = 9999f;
        main.maxParticles = partCount;
        main.playOnAwake = true;
        main.startSpeed = 0;


        renderer.material = particleMaterial;

        return newParticleSystem;
    }

    public void DeleteParticleSystem()
    {

    }
}
