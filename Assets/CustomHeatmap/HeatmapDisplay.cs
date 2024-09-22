using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using System;
using System.Linq;

public class HeatmapDisplay : MonoBehaviour
{
    public enum brushes { Sphere, Cube, Point };
    public enum MeshCheckType { None, DoubleRadius, DoubleRaycast };

    public Collider heatmapBoundBox;
    public bool DEBUG = false;
    //private bool readyToDisplay = false;
    [Header("Collider Settings")]
    public LayerMask layersHit;
    public GameObject referencePoint;
    public Vector3 boundBoxMargins;
    [Tooltip("Whether the script should allow multiple points to be loaded at the same time. Useful when data is points are logged multiple times and we want to make sure multiple rays aren't rounded to the same grid point.")]
    public bool allowDuplicatesWhenLoadingData = false;

    [Header("Point Space Check")]
    public MeshCheckType checkForInsideMesh = MeshCheckType.DoubleRadius;
    public float doubleRaycastDistance = 100f;

    [Header("Particle Settings")]
    public float particleSize = 1.0f;
    [Range(0.0001f, 1.0f)]
    public float particleSpacing = 0.0f;
    public Material particleMaterial;
    public int minVisibleHeat = 0;
    public long maxVisibleHeat = 100000;
    public bool useHighestValueFound = false;
    public bool limitHighestValueFound = true;
    public Gradient heatmapColors;
    public bool createNewSystemAlways = false;
    public ParticleSystem[] particleSystems;

    [Header("Brush Size")]
    public brushes brushType;
    [Range(0, 10)]
    public float brushSize = 1;



    //private Vector3 boundDimensions = Vector3.zero;
    //private Vector3Int gridDimensions = Vector3Int.zero;
    private int particleCount = 0;
    private int MAX_PARTICLE_PER_HEATMAP = int.MaxValue;
    //private int[,,] dataGrid;
    private Dictionary<Vector3Int, long> gridValues = new Dictionary<Vector3Int, long>();
    //private Vector3 minPoint;
    //private Vector3 maxPoint;
    private long maxHeat = 0;
    private Vector3 localMinPoint = Vector3.zero;
    private float maxFrameLength = 0.016f;
    private float doubleRadiusSmallRadius = 0.00001f;

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

    public IEnumerator PrepareHeatmap(bool trimBox = true)
    {
        if (trimBox)
            TrimHeatmapBoundBox();
        
        yield return StartCoroutine(CreateDictionary(heatmapBoundBox));
        particleSystems = CreateParticleSystems(particleCount, heatmapBoundBox);
        yield return null;
    }

    public IEnumerator DisplayHeatmap(long maxTempHeat = -1)
    {
        List<Vector3Int> pointKeys = new List<Vector3Int>(gridValues.Keys);
        int keyInd = 0;

        Debug.Log("max heat is " + maxHeat);

        foreach (var particleSystem in particleSystems)
        { 
            var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            particleSystem.GetParticles(particles);

            int index = 0;

            var watch = new System.Diagnostics.Stopwatch();

            long tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                     * ((maxFrameLength)));
            watch.Restart();

            if (maxTempHeat < 0)
            {
                maxTempHeat = maxVisibleHeat;
            }

            if (!useHighestValueFound || (limitHighestValueFound && maxHeat > maxTempHeat))
            {
                maxHeat = maxTempHeat;
            }
            //Debug.Log("New value of max heat is " + maxHeat);
            //string values = string.Empty;

            int endInd = Mathf.Clamp(keyInd + particleSystem.main.maxParticles, keyInd, pointKeys.Count);

            //foreach (KeyValuePair<Vector3Int, long> pair in gridValues)
            for (; keyInd < endInd; keyInd++)
            {
                if (index >= particles.Length)
                {
                    Debug.LogError("ERROR: More datapoints than particles in the system! (" + gridValues.Count + ")");
                    break;
                }

                Vector3 finalPos = ((Vector3)pointKeys[keyInd] * (particleSpacing))
                    + referencePoint.transform.TransformPoint(localMinPoint);
                finalPos = particleSystem.transform.InverseTransformPoint(finalPos);

                float colorValue = 0;

                if (maxHeat > 0)
                {
                    colorValue = (float)gridValues[pointKeys[keyInd]] / (float)maxHeat;
                }
                else
                {
                    colorValue = (colorValue > 0 ? 1f : 0f);
                }


                DisplayPoint(particles, index, finalPos, colorValue);
                index++;

                if (watch.ElapsedTicks > tickBudget)
                {
                    //Debug.Log(string.Format("Particle #{0} being displayed with value {1}.", index, pair.Value));
                    yield return null;
                    watch.Restart();
                }

                //values = string.Format("{0}, ({1} = {2})", values, pair.Key.ToString(), pair.Value.ToSafeString());
            }

            //Debug.Log("Max heat recorder: " + maxHeat);
            //Debug.Log(values);

            particleSystem.SetParticles(particles);
        }

        yield return null;
    }

    IEnumerator GenerateHeatmapCoroutine(Vector3[] points)
    {
        TimeSpan timeA, timeB;
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        TrimHeatmapBoundBox();
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Box trimmed: " + (timeB - timeA).TotalSeconds);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        yield return StartCoroutine(CreateDictionary(heatmapBoundBox));
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Dictionary Created: " + (timeB - timeA).TotalSeconds);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        yield return StartCoroutine(LoadDictionary(points, heatmapBoundBox));
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Dictionary loaded: " + (timeB - timeA).TotalSeconds);

        timeA = new TimeSpan(System.DateTime.Now.Ticks);
        particleSystems = CreateParticleSystems(particleCount, heatmapBoundBox);
        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Particle System created: " + (timeB - timeA).TotalSeconds);

        timeA = new TimeSpan(System.DateTime.Now.Ticks);

        //system.Play();

        //system.Emit(particleCount);
        //particleSystem.

        yield return null;

        Debug.Log("Particles Emitted: " + particleCount);
        timeA = new TimeSpan(System.DateTime.Now.Ticks);

        List<Vector3Int> pointKeys = new List<Vector3Int>(gridValues.Keys);
        int keyInd = 0;

        foreach (var particleSystem in particleSystems)
        {
            var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            particleSystem.GetParticles(particles);

            int index = 0;

            //foreach (var p in points)
            //{
            //    DisplayPoint(particles, index++, p, 0);
            //}

            var watch = new System.Diagnostics.Stopwatch();

            long tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                     * ((maxFrameLength)));
            watch.Restart();

            if (!useHighestValueFound || (maxHeat > maxVisibleHeat && limitHighestValueFound))
            {
                maxHeat = maxVisibleHeat;
            }

            int endInd = Mathf.Clamp(keyInd + particleSystem.main.maxParticles, keyInd, pointKeys.Count);

            //foreach (KeyValuePair<Vector3Int, long> pair in gridValues)
            for (; keyInd < endInd; keyInd++)
            {
                Vector3 finalPos = ((Vector3)pointKeys[keyInd] * (particleSpacing))
                    + referencePoint.transform.TransformPoint(localMinPoint);
                finalPos = particleSystem.transform.InverseTransformPoint(finalPos);

                //finalPos = (Vector3)pair.Key;

                float colorValue = 0;

                if (maxHeat > 0)
                {
                    colorValue = (float)gridValues[pointKeys[keyInd]] / (float)maxHeat;
                }


                //DisplayPoint(particles, index, referencePoint.transform.TransformPoint(finalPos), colorValue);
                DisplayPoint(particles, index, finalPos, colorValue);
                index++;

                if (watch.ElapsedTicks > tickBudget)
                {
                    yield return null;
                    watch.Restart();
                }
            }

            particleSystem.SetParticles(particles);
        }

        timeB = new TimeSpan(System.DateTime.Now.Ticks);
        Debug.Log("Points Displayed. " + (timeB - timeA).TotalSeconds);

        yield return null;
    }

    private void DisplayPoint(Particle[] particles, int particleIndex, Vector3 point, float gradientColor)
    {
        particles[particleIndex].position = point;
        particles[particleIndex].startColor = heatmapColors.Evaluate(gradientColor);
        //Debug.Log("Particle " + particleIndex + " set to " 
        //    + particles[particleIndex].position.ToSafeString() 
        //    + " with color value " + particles[particleIndex].startColor.ToString());
    }

    public IEnumerator LoadDictionary(Vector3[] points, Collider collider)
    {
        //CreateDictionary(heatmapBoundBox);

        //HashSet<Vector3Int> uniqueVectors = new HashSet<Vector3Int>();

        var watch = new System.Diagnostics.Stopwatch();

        long tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
        watch.Restart();

        int minBound = 0, maxBound = 1;
        int brushGridSize = Mathf.CeilToInt(brushSize / particleSpacing);

        if (brushType != brushes.Point)
        {
            minBound = -brushGridSize;
            maxBound = brushGridSize;
        }

        foreach (var point in points)
        {
            List<Vector3Int> uniqueVectors = new List<Vector3Int>();
            //Dictionary<Vector3Int, bool> uniqueVectors = new Dictionary<Vector3Int, bool>();
            //Vector3 vec = (referencePoint.transform.TransformPoint(point - localMinPoint)) / (particleSpacing);
            Vector3 vec = (referencePoint.transform.TransformPoint(point) - referencePoint.transform.TransformPoint(localMinPoint)) / (particleSpacing);
            vec = new Vector3(Mathf.Abs(vec.x), Math.Abs(vec.y), Mathf.Abs(vec.z));

            Vector3Int index = new Vector3Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y), Mathf.RoundToInt(vec.z));

            if (DEBUG)
                Debug.Log(string.Format("{0} point, {1} local min point, {2} world distance, {3} index", point.ToString(), localMinPoint.ToString(),
                (referencePoint.transform.TransformPoint(point - localMinPoint)), index.ToString()));

            for (int x = minBound; x < maxBound; x++)
            {
                for (int y = minBound; y < maxBound; y++)
                {
                    for (int z = minBound; z < maxBound; z++)
                    {
                        Vector3Int tempInd = index + new Vector3Int(x, y, z);

                        if (!gridValues.ContainsKey(tempInd))
                        {
                            //Debug.LogWarning("INDEX NOT FOUND: " + index.ToString() + "\nLOCAL POSITION: " + point.ToString());

                            continue;
                        }

                        if (brushType == brushes.Sphere && (new Vector3(x, y, z) * particleSpacing).magnitude > brushSize)
                        {
                            continue;
                        }

                        if (allowDuplicatesWhenLoadingData)
                            gridValues[tempInd]++;
                        //else
                        //else if (!uniqueVectors.ContainsKey(tempInd))
                        else if (uniqueVectors.FindIndex(x => x == tempInd) == -1)
                        {
                            //uniqueVectors.Add(tempInd, true);
                            uniqueVectors.Add(tempInd);
                        }

                        if (DEBUG)
                            Debug.Log(string.Format("{0} new value is {1}", tempInd.ToString(), gridValues[tempInd]));

                        if (gridValues[tempInd] > maxHeat)
                        {
                            maxHeat = gridValues[tempInd];
                        }

                        if (watch.ElapsedTicks > tickBudget)
                        {
                            yield return null;
                            watch.Restart();
                        }

                    }
                }
            }

            if (!allowDuplicatesWhenLoadingData)
            {
                foreach (var tempInd in uniqueVectors)
                //foreach (KeyValuePair<Vector3Int, bool> tempInd in uniqueVectors)
                {
                    //gridValues[tempInd.Key]++;
                    gridValues[tempInd]++;
                }
            }
        }
    }

    private void TrimHeatmapBoundBox()
    {
        //boundDimensions = calculateBoundDimensions(heatmapBoundBox);

        RaycastHit hit;
        Vector3 tempOrigin = Vector3.zero;
        Vector3 raycastThickness = heatmapBoundBox.bounds.extents * 2;
        Vector3 maxPoint = heatmapBoundBox.bounds.max;
        Vector3 minPoint = heatmapBoundBox.bounds.min;

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

        minPoint -= boundBoxMargins;
        maxPoint += boundBoxMargins;

        //Calculate
        heatmapBoundBox.bounds.SetMinMax(minPoint, maxPoint);
    }

    IEnumerator CreateDictionary(Collider boundingBox)
    {
        var watch = new System.Diagnostics.Stopwatch();

        long tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
        watch.Restart();

        gridValues.Clear();

        maxHeat = 0;
        Vector3Int gridDimensions = Vector3Int.zero;

        gridDimensions.x = Mathf.FloorToInt((boundingBox.bounds.extents.x * 2 - particleSize * 2) / (particleSpacing));
        gridDimensions.y = Mathf.FloorToInt((boundingBox.bounds.extents.y * 2 - particleSize * 2) / (particleSpacing));
        gridDimensions.z = Mathf.FloorToInt((boundingBox.bounds.extents.z * 2 - particleSize * 2) / (particleSpacing));

        Vector3 margins = ((boundingBox.bounds.extents * 2) - ((Vector3)gridDimensions * particleSpacing) - (Vector3.one * particleSize * 2)) / 2;

        Vector3 minParticlePos = boundingBox.bounds.min + margins + Vector3.one * particleSize;
        localMinPoint = referencePoint.transform.InverseTransformPoint(minParticlePos);
        //Debug.Log(minParticlePos.ToSafeString());
        //Debug.Log(referencePoint.transform.position);
        //Debug.Log(boundingBox.bounds.min);
        //Debug.Log(localMinPoint);
        Debug.Log("Grid Dimensions: " + gridDimensions.ToSafeString());
        int debugRadiusPoints = 0;

        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                for (int z = 0; z < gridDimensions.z; z++)
                {
                    
                    Vector3 tempPos = minParticlePos
                        + Vector3.right * (x * particleSpacing)
                        + Vector3.up * (y * particleSpacing)
                        + Vector3.forward * (z * particleSpacing);

                    //Debug.Log(string.Format("{0} = {1} + {2} + {3}", minParticlePos, Vector3.right * (x * particleSpacing), Vector3.up * (y * particleSpacing), Vector3.forward * (z * particleSpacing)));
                    //Vector3 tempPos = minParticlePos
                    //    + boundingBox.transform.right * (x * particleSpacing + particleSize)
                    //    + boundingBox.transform.up * (y * particleSpacing + particleSize)
                    //    + boundingBox.transform.forward * (z * particleSpacing + particleSize);

                    //tempPos = referencePoint.transform.TransformPoint(tempPos);

                    Collider[] results = new Collider[1];

                    bool radiusCollider;
                    bool validPoint;

                    switch (checkForInsideMesh)
                    {
                        case MeshCheckType.DoubleRadius:

                            radiusCollider = (Physics.OverlapSphereNonAlloc(tempPos, doubleRadiusSmallRadius, results, layersHit) > 0);
                            debugRadiusPoints += (radiusCollider ? 1 : 0);
                            validPoint = !radiusCollider && (Physics.OverlapBoxNonAlloc(tempPos, particleSpacing * Vector3.one, 
                                results, referencePoint.transform.rotation, layersHit) > 0);
                            break;

                        case MeshCheckType.DoubleRaycast:

                            radiusCollider = (Physics.OverlapSphereNonAlloc(tempPos, doubleRadiusSmallRadius, results, layersHit) > 0);

                            bool tempBackfaces = Physics.queriesHitBackfaces;
                            Physics.queriesHitBackfaces = true;
                            int raycastHitCount = Physics.RaycastAll(tempPos + Vector3.up * doubleRaycastDistance, Vector3.down, doubleRaycastDistance * 2).Count();
                            raycastHitCount += Physics.RaycastAll(tempPos + Vector3.right * doubleRaycastDistance, Vector3.left, doubleRaycastDistance * 2).Count();
                            raycastHitCount += Physics.RaycastAll(tempPos + Vector3.forward * doubleRaycastDistance, Vector3.back, doubleRaycastDistance * 2).Count();
                            Physics.queriesHitBackfaces = tempBackfaces;
                            validPoint = (radiusCollider && ((raycastHitCount) == 0 || (raycastHitCount) % 2 == 1));
                            break;

                        case MeshCheckType.None:
                        default:
                            validPoint = (Physics.OverlapSphereNonAlloc(tempPos, particleSize, results, layersHit) > 0);
                            break;
                    }

                    if (validPoint)
                    {
                        Vector3Int vector3Int = new Vector3Int(x, y, z);
                        gridValues.Add(vector3Int, 0);
                        particleCount++;

                        if (particleCount > MAX_PARTICLE_PER_HEATMAP)
                        {
                            Debug.LogError("ERROR, MAX PARTICLE SIZE REACHED");
                            yield break;
                        }
                        else
                        {
                            //Debug.Log(string.Format("Valid point at ({0},{1},{2})", x, y, z));
                        }

                        //GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        //temp.transform.localScale = Vector3.one * particleSize;
                        //temp.transform.position = tempPos;
                        //temp.transform.rotation = referencePoint.transform.rotation;
                    }

                    if (watch.ElapsedTicks > tickBudget)
                    {
                        yield return null;
                        watch.Restart();
                    }
                }
            }
            Debug.Log(string.Format("{0}/{1} points checked.", x * gridDimensions.y * gridDimensions.z, gridDimensions.x * gridDimensions.y * gridDimensions.z));
        }
        Debug.Log(particleCount + " valid points found.");
        Debug.Log(debugRadiusPoints + " debug points found.");
        Debug.Log("Heatmap grid defined.");
    }

    private ParticleSystem[] CreateParticleSystems(int partCount, Collider collider)
    {

        int particleSystemsCount = Mathf.CeilToInt((float)partCount / Constants.maxParticleCount);

        ParticleSystem[] finalOutput = new ParticleSystem[particleSystemsCount];

        for (int i = 0; i < particleSystemsCount; i++)
        {

            //if (referencePoint.TryGetComponent<ParticleSystem>(out ParticleSystem output))
            //{
            //    if (createNewSystemAlways)
            //    {
            //        Destroy(output);
            //        finalOutput[i] = referencePoint.AddComponent<ParticleSystem>();
            //    }
            //    else
            //    {
            //        finalOutput[i] = output;
            //    }
            //}
            //else
            //{
            //    finalOutput[i] = referencePoint.AddComponent<ParticleSystem>();
            //}

            finalOutput[i] = referencePoint.AddComponent<ParticleSystem>();

            EmissionModule emission = finalOutput[i].emission;
            emission.enabled = true;
            emission.rateOverDistance = 0;
            emission.rateOverTime = 0;
            Burst[] bursts = new Burst[1];
            bursts[0].count = partCount;
            bursts[0].time = 0;
            emission.SetBursts(bursts);


            ShapeModule shape = finalOutput[i].shape;
            shape.enabled = false;
            //shape.shapeType = ParticleSystemShapeType.Sphere;
            //shape.radius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y, collider.bounds.extents.z);

            ParticleSystemRenderer renderer = referencePoint.GetComponent<ParticleSystemRenderer>();
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.allowRoll = false;
            renderer.alignment = ParticleSystemRenderSpace.Facing;

            MainModule main = finalOutput[i].main;
            main.loop = false;
            //main.duration = 1;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            //main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSize = particleSize;
            main.startLifetime = 9999f;
            //main.maxParticles = partCount;
            main.maxParticles = Constants.maxParticleCount;
            main.playOnAwake = true;
            main.startSpeed = 0;


            renderer.material = particleMaterial;
        }

        return finalOutput;
    }

    public void DeleteParticleSystem()
    {
        Destroy(particleMaterial);
    }

    public Dictionary<Vector3Int, long> GetPointDictionary()
    {
        return gridValues;
    }

    public void SetPointDictionary(Dictionary<Vector3Int, long> from)
    {
        gridValues.Clear();

        foreach (KeyValuePair<Vector3Int , long> pair in from)
        {
            Debug.Log(string.Format("Setting {0} to {1}", pair.Key, pair.Value));
            gridValues.Add(pair.Key, pair.Value);
        }
    }

    public void SetMaxHeat(long to)
    {
        maxHeat = to;
        Debug.Log("Max Heat is now " + maxHeat);
    }

    public void AddPointsToDictionary(Dictionary<Vector3Int, long> from)
    {
        foreach (KeyValuePair<Vector3Int, long> pair in from)
        {
            if (gridValues.ContainsKey(pair.Key))
            {
                gridValues[pair.Key] += pair.Value;
            }
            else
            {
                gridValues.Add(pair.Key, pair.Value);
            }
        }
    }
}
