using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomSimulationManager : MonoBehaviour
{
    #region Subclasses
    [System.Serializable]
    public class CameraPointItem
    {
        public string id;
        public CameraPoint cameraPoint;
    }

    [System.Serializable]
    public class VehiclePoint
    {
        public string id;
        public Transform vehiclePoint;
    }

    [System.Serializable]
    public class Setup
    {
        public string id;
        public GameObject setupParent;
        [System.Serializable]
        public class MyIntEvent : UnityEvent<HeatmapManager> { };
        public MyIntEvent OnStart;
        public MyIntEvent OnStop;
    }

    [System.Serializable]
    public class VehiclePrefab
    {
        public string id;
        public Transform prefab;
    }
    #endregion

    public static CustomSimulationManager Instance;

    [Header("Positions")]
    public CameraPointItem[] cameraPoints;
    public VehiclePoint[] vehiclePoints;

    [Header("Contents")]
    public Setup[] setups;
    public VehiclePrefab[] vehiclePrefabs;
    public int numberOfVehicles = 1;
    public VehiclePrefab[] fillerVehiclePrefabs;
    [HideInInspector]
    public List<string> activeSetups = new List<string>();

    [Header("Data Saving")]
    public bool savePointsAfterSimulation;
    public GridPointRecorderScript gridPointRecorderScript;
    public UnityEvent OnDatapointSavingStart;
    public UnityEvent OnDatapointSavingFinished;
    public string savingFileName = string.Empty;
    public string directoryName;
    public string fileNamePrefix;

    [Header("Progress")]
    private float _progress = 0;
    private float progress
    {
        get { return _progress; }
        set
        {
            _progress = value;
            OnProgress.Invoke(_progress);
            Debug.Log("Current progress: " + _progress.ToString());
        }
    }
    [System.Serializable]
    public class ProgressEvent : UnityEvent<float> { };
    public ProgressEvent OnProgress;
    public UnityEvent OnProgressCompleted;

    [Header("Others")]
    public Camera shotCamera;
    public Transform banishmentPoint;
    public HeatmapSender heatmapSender;

    //Dictionaries
    private Dictionary<string, CameraPointItem> cameraPointsDictionary = new Dictionary<string, CameraPointItem>();
    private Dictionary<string, VehiclePoint> vehiclePointsDictionary = new Dictionary<string, VehiclePoint>();
    private Dictionary<string, Setup> setupsDictionary = new Dictionary<string, Setup>();
    private Dictionary<string, VehiclePrefab> vehiclePrefabsDictionary = new Dictionary<string, VehiclePrefab>();
    private Dictionary<string, VehiclePrefab> fillerVehiclePrefabsDictionary = new Dictionary<string, VehiclePrefab>();

    //Used to not freeze the program
    private System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    //variables during execution
    private HeatmapManager ActiveTarget;
    private bool SimulationRunning = false;
    private List<VehiclePoint> activeVehiclePoints = new List<VehiclePoint>();
    private List<VehiclePoint> activeFillerVehiclePoints = new List<VehiclePoint>();
    private List<CameraPointItem> activeCameraPoints = new List<CameraPointItem>();
    private Dictionary<string, Transform> spawnedFillerVehicles = new Dictionary<string, Transform>();
    private Dictionary<string, int[]> validDirectionsPerPoint = new Dictionary<string, int[]>();


    public void Awake()
    {
        Instance = this;

        foreach (var cp in cameraPoints)
        {
            if (cameraPointsDictionary.ContainsKey(cp.id))
            {
                Debug.Log("Duplicate Camera ID: " + cp.id);
                continue;
            }

            cameraPointsDictionary.Add(cp.id, cp);
        }

        foreach (var vp in vehiclePoints)
        {
            if (vehiclePointsDictionary.ContainsKey(vp.id))
            {
                Debug.Log("Duplicate Vehicle ID: " + vp.id);
                continue;
            }

            vehiclePointsDictionary.Add(vp.id, vp);
        }

        foreach (var vpr in vehiclePrefabs)
        {
            if (vehiclePrefabsDictionary.ContainsKey(vpr.id))
            {
                Debug.Log("Duplicate Vehicle ID: " + vpr.id);
                continue;
            }

            vehiclePrefabsDictionary.Add(vpr.id, vpr);
        }

        foreach (var fvpr in fillerVehiclePrefabs)
        {
            if (fillerVehiclePrefabsDictionary.ContainsKey(fvpr.id))
            {
                Debug.Log("Duplicate Vehicle ID: " + fvpr.id);
                continue;
            }

            fillerVehiclePrefabsDictionary.Add(fvpr.id, fvpr);
        }

        watch = new System.Diagnostics.Stopwatch();
        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
    }

    public void SetSenderMinMax(float min= 0, float max = Mathf.Infinity)
    {
        Debug.Log(string.Format("Setting Min to {0} and max to {1}", min, max));
        heatmapSender.minDistance = min;
        heatmapSender.maxDistance = max;
    }

    public void StartSimulation(string setupIDs, string activeVehiclePrefabID, string activeVehiclePositionsID, string fillerVehiclePrefabID, string fillerVehiclePositionsIDs, string activeCameraAngles)
    {
        activeVehiclePoints = new List<VehiclePoint>();
        activeFillerVehiclePoints = new List<VehiclePoint>();
        spawnedFillerVehicles = new Dictionary<string, Transform>();
        string[] selectedSetups = setupIDs.Split(',');

        foreach (var s in selectedSetups)
        {
            if (setupsDictionary.TryGetValue(s, out var val))
            {
                activeSetups.Add(s);

                val.setupParent.gameObject.SetActive(true);
            }
        }

        if (ActiveTarget != null)
        {
            Destroy(ActiveTarget.gameObject);
        }

        if (vehiclePrefabsDictionary.TryGetValue(activeVehiclePrefabID, out VehiclePrefab targetVehiclePrefab))
        {
            Transform tempTransform = Instantiate(targetVehiclePrefab.prefab, banishmentPoint.position, banishmentPoint.rotation);
            if ((ActiveTarget = tempTransform.GetComponentInChildren<HeatmapManager>()) == null)
            {
                Debug.LogError("Scenario couldn't be run. No HeatmapManager was found on " + activeVehiclePrefabID + ".");
                return;
            }
        }
        else
        {
            Debug.LogError("Scenario couldn't be run. No vehicle prefab was found with ID " + activeVehiclePrefabID + ".");
            return;
        }

        foreach (var s in activeVehiclePositionsID.Split(','))
        {
            if (vehiclePointsDictionary.TryGetValue(s, out VehiclePoint v))
            {
                activeVehiclePoints.Add(v);
            }
            else
            {
                Debug.LogError("Couldn't add " + s + " to the active vehicle points. No such ID found.");
            }
        }

        foreach (var f in fillerVehiclePositionsIDs.Split(','))
        {
            if (vehiclePointsDictionary.TryGetValue(f, out VehiclePoint v))
            {
                activeFillerVehiclePoints.Add(v);
            }
            else
            {
                Debug.LogError("Couldn't add \"" + f + "\" to the filler vehicle points. No such ID found.");
            }
        }


        GameObject fillerParent = new GameObject();

        if (fillerVehiclePrefabsDictionary.TryGetValue(fillerVehiclePrefabID, out VehiclePrefab fillerVehiclePrefab))
        {
            foreach (var fp in activeFillerVehiclePoints)
            {
                Transform temp = Instantiate(fillerVehiclePrefab.prefab, fp.vehiclePoint.position, fp.vehiclePoint.rotation, fillerParent.transform);
                spawnedFillerVehicles.Add(fp.id, temp);
                temp.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Scenario couldn't be run. No vehicle prefab was found with ID " + activeVehiclePrefabID + ".");
            return;
        }

        string[] tuples = activeCameraAngles.Split(';');

        foreach (string tuple in tuples)
        {
            string[] parts = tuple.Split('-');
            string cameraPointID = parts[0];

            if (cameraPointsDictionary.ContainsKey(cameraPointID))
            {
                string[] points = parts[1].Split(',');

                int[] validDirections = new int[points.Length];

                for (int i = 0; i < points.Length; i++)
                {
                    if (int.TryParse(points[i], out int result))
                    {
                        validDirections[i] = result;
                    }
                    else
                    {
                        validDirections[i] = -1;
                    }
                }

                activeCameraPoints.Add(cameraPointsDictionary[cameraPointID]);
                validDirectionsPerPoint.Add(cameraPointID, validDirections);
            }

        }




        StartCoroutine(RunSimulation());
    }

    IEnumerator RunSimulation()
    {
        SimulationRunning = true;

        foreach (var s in activeSetups)
        {
            setupsDictionary[s].OnStart.Invoke(ActiveTarget);
        }

        progress = 0;

        var indexes = CalculatePermutations(activeVehiclePoints.Count, activeFillerVehiclePoints.Count, numberOfVehicles-1);

        yield return StartCoroutine(ActiveTarget.HeatmapSetup());

        yield return null;

        int tvIndex = 0;
        float denominatorProgress = (indexes.Count * activeFillerVehiclePoints.Count) + 1;

        foreach (var index in indexes)
        {
            ActiveTarget.HeatmapLogger.parentObject.transform.position = activeVehiclePoints[index.vehiclePosition].vehiclePoint.position;
            ActiveTarget.HeatmapLogger.parentObject.transform.rotation = activeVehiclePoints[index.vehiclePosition].vehiclePoint.rotation;

            int fvIndex = 0;
            foreach (var fL in index.fillerPermutations)
            {
                string temp = "(";
                foreach (var b in fL)
                    temp += (b ? "1" : 0) + ",";
                temp += ")";

                progress = (((tvIndex) * activeFillerVehiclePoints.Count) + fvIndex) / denominatorProgress;

                for (int fp = 0; fp < fL.Length; fp++)
                {
                    spawnedFillerVehicles[activeFillerVehiclePoints[fp].id].gameObject.SetActive(fL[fp]);
                }

                foreach (var cp in activeCameraPoints)
                {
                    if (watch.ElapsedTicks > tickBudget)
                    {
                        yield return null;
                        watch.Restart();
                    }

                    ActiveTarget.HeatmapLogger.ClearPointCache();

                    yield return StartCoroutine(cp.cameraPoint.CycleThroughCameras(shotCamera, validDirectionsPerPoint[cp.id]));

                    yield return StartCoroutine(ActiveTarget.LoadCurrentPointsIntoMatrix());
                }

                fvIndex++;
            }

            tvIndex++;
        }

        OnProgressCompleted.Invoke();


        ActiveTarget.HeatmapLogger.parentObject.transform.position = banishmentPoint.position;
        ActiveTarget.HeatmapLogger.parentObject.transform.rotation = banishmentPoint.rotation;


        yield return StartCoroutine(ActiveTarget.DisplayHeatmap());

        if (savePointsAfterSimulation)
        {
            OnDatapointSavingStart.Invoke();
            yield return StartCoroutine(gridPointRecorderScript.SetDataPoints(ActiveTarget.HeatmapDisplay.GetPointDictionary()));
            gridPointRecorderScript.SaveFile(directoryName, fileNamePrefix + savingFileName + Application.version);
            OnDatapointSavingFinished.Invoke();
        }


        foreach (var s in activeSetups)
        {
            setupsDictionary[s].OnStop.Invoke(ActiveTarget);
        }


        OnProgress.Invoke(1);


        OnProgressCompleted.Invoke();

        SimulationRunning = false;
    }

    class VehiclePermutation
    {
        public VehiclePermutation(int vp, List<bool[]> fp)
        {
            vehiclePosition = vp;
            fillerPermutations = fp;
        }

        public int vehiclePosition;
        public List<bool[]> fillerPermutations = new List<bool[]>();
    }

    List<VehiclePermutation> CalculatePermutations(int vehiclePosLength, int fillerPosLength, int fillerAmount)
    {

        List<VehiclePermutation> result = new List<VehiclePermutation>();
        int vpi = 0;

        for (int n = 0; n < vehiclePosLength; n++)
        {
            List<bool[]> fillerPermResult = new List<bool[]>();

            CalculatePermutationsRecursive(n, -1, fillerPosLength, new bool[fillerPosLength], ref fillerPermResult, fillerAmount);

            result.Add(new VehiclePermutation(n, fillerPermResult));
        }

        return result;
    }

    void CalculatePermutationsRecursive(int vehiclePosInd, int fillerPosInd, int fillerPosLength, bool[] currentPerm, ref List<bool[]> result, int depthLeft)
    {
        if (depthLeft == 0)
        {
            bool[] thisPerm = new bool[fillerPosLength];

            for (int i = 0; i < fillerPosLength; i++)
            {
                thisPerm[i] = currentPerm[i];
            }

            result.Add(thisPerm);

            return;
        }

        for (int n = fillerPosInd + 1; n < fillerPosLength; n++)
        {
            if (activeFillerVehiclePoints[n].id == activeVehiclePoints[vehiclePosInd].id)
                continue;

            currentPerm[n] = true;
            CalculatePermutationsRecursive(vehiclePosInd, n, fillerPosLength, currentPerm, ref result, depthLeft - 1);
            currentPerm[n] = false;
        }
    }

    public void Execute()
    {
        heatmapSender.CameraViewHeat();
    }
}
