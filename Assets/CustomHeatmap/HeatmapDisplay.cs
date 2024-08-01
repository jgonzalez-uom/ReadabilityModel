using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using System.IO;

public class HeatmapDisplay : MonoBehaviour
{
    public Collider heatmapBoundBox;
    private bool readyToDisplay = false;
    public LayerMask layersHit;

    [Header("Particle Settings")]
    public float particleSize = 1.0f;
    public float particleSpacing = 0.0f;
    public Material particleMaterial;
    public Gradient heatmapColors;


    //private Vector3 boundDimensions = Vector3.zero;
    //private Vector3Int gridDimensions = Vector3Int.zero;
    private int particleCount = 0;
    //private int[,,] dataGrid;
    private Dictionary<Vector3Int, int> gridValues = new Dictionary<Vector3Int, int>();
    private Vector3 minPoint;
    private Vector3 maxPoint;
    private int maxHeat = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (heatmapBoundBox == null && this.gameObject.TryGetComponent<Collider>(out heatmapBoundBox))
        {
            readyToDisplay = true;
        }

        //boundDimensions = calculateBoundDimensions(heatmapBoundBox);
    }

    public void GenerateHeatmap(Vector3[] points)
    {
        LoadDictionary(points);
        ParticleSystem system = CreateParticleSystem();

        system.Emit(particleCount);

        var particles = new ParticleSystem.Particle[particleCount];
        int index = 0;

        foreach (KeyValuePair<Vector3Int, int> pair in gridValues)
        {
            Vector3 finalPos = ((Vector3)pair.Key * (particleSize * 2 + particleSpacing)) + minPoint;
            DisplayPoint(particles, index, finalPos, (float)pair.Value / (float)maxHeat);
        }
    }



    private void DisplayPoint(Particle[] particles, int particleIndex, Vector3 point, float gradientColor)
    {
        particles[particleIndex].position = point;
        particles[particleIndex].startColor = heatmapColors.Evaluate(gradientColor);
    }

    private void LoadDictionary(Vector3[] points)
    {
        CreateDictionary(heatmapBoundBox);

        foreach (var point in points)
        {
            Vector3 vec = (point - minPoint) / (particleSize * 2 + particleSpacing);
            Vector3Int index = new Vector3Int(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));

            if (!gridValues.ContainsKey(index))
            {
                Debug.LogError("INDEX NOT FOUND: " + index.ToString());
                return;
            }

            gridValues[index]++;
        }
    }

    private void TrimHeatmapBoundBox()
    {
        //boundDimensions = calculateBoundDimensions(heatmapBoundBox);

        RaycastHit hit;
        Vector3 tempOrigin = Vector3.zero;
        Vector3 raycastThickness = heatmapBoundBox.bounds.extents * 2;
        maxPoint = heatmapBoundBox.bounds.center;
        minPoint = heatmapBoundBox.bounds.center;

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
            maxPoint.x = hit.point.x;
        }

        tempOrigin = -transform.up * float.PositiveInfinity;

        if (Physics.SphereCast(tempOrigin, raycastThickness.y, transform.up, out hit, Mathf.Infinity, layersHit))
        {
            maxPoint.y = hit.point.y;
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
        maxHeat = 0;
        Vector3Int gridDimensions = Vector3Int.zero;

        gridDimensions.x = Mathf.FloorToInt((heatmapBoundBox.bounds.extents.x - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;
        gridDimensions.y = Mathf.FloorToInt((heatmapBoundBox.bounds.extents.y - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;
        gridDimensions.z = Mathf.FloorToInt((heatmapBoundBox.bounds.extents.z - particleSize * 2) / (particleSize * 2 + particleSpacing)) + 1;

        Vector3 margins = new Vector3(
            ((heatmapBoundBox.bounds.extents.x * 2) - (gridDimensions.x * particleSize * 2) - ((gridDimensions.x - 1) * particleSpacing))/2,
            ((heatmapBoundBox.bounds.extents.y * 2) - (gridDimensions.y * particleSize * 2) - ((gridDimensions.y - 1) * particleSpacing))/2,
            ((heatmapBoundBox.bounds.extents.z * 2) - (gridDimensions.z * particleSize * 2) - ((gridDimensions.z - 1) * particleSpacing))/2);


        Vector3 minParticlePos = heatmapBoundBox.bounds.min + transform.right * margins.x + transform.up * margins.y + transform.forward * margins.z;

        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                for (int z = 0; z < gridDimensions.z; z++)
                {
                    Vector3 tempPos = minParticlePos
                        + transform.right * x * (particleSize * 2 + particleSpacing)
                        + transform.up * y * (particleSize * 2 + particleSpacing)
                        + transform.forward * z * (particleSize * 2 + particleSpacing);

                    Collider[] results = new Collider[1];
                    if (Physics.OverlapSphereNonAlloc(tempPos, particleSize, results, layersHit) > 0)
                    {
                        gridValues.Add(new Vector3Int(x, y, z), 0);
                        particleCount++;

                        if (particleCount > Constants.maxParticleCount)
                        {
                            Debug.LogError("ERROR, MAX PARTICLE SIZE REACHED");
                            return;
                        }
                    }
                }
            }
        }
    }

    private ParticleSystem CreateParticleSystem()
    {
        if (gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem output))
            Destroy(output);
            

        ParticleSystem newParticleSystem = gameObject.AddComponent<ParticleSystem>();

        EmissionModule emission = newParticleSystem.emission;
        emission.enabled = false;

        ShapeModule shape = newParticleSystem.shape;
        shape.enabled = false;

        ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();
        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.allowRoll = false;
        renderer.alignment = ParticleSystemRenderSpace.Facing;

        MainModule main = newParticleSystem.main;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particleCount;
        main.playOnAwake = false;


        renderer.material = particleMaterial;

        return newParticleSystem;
    }

    public void DeleteParticleSystem()
    {

    }
}
