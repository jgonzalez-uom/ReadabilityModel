using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;


    private System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    [System.Serializable]
    public class Tests
    {
        [System.Serializable]
        public class VehiclePosition
        {
            public Transform objectPosition;
            public bool useDriverViewPoint = false;
            public CameraPoint optionalDriverViewPoint;
            public bool fillerVehicle;
        }

        public string testName;
        public Transform prefab;
        public Transform fillerPrefab;
        public VehiclePosition[] vehiclePositions;
        public CameraPoint[] pedestrianViewPoints;
        public Transform[] roadSetups;
        public Camera camera;
        public HeatmapSender heatmapSender;

        [HideInInspector]
        public bool saveFileWithLogs;

        //public enum fillerCullingTypes {None, CullBehindPlayer,  };
        //public bool cullUselessFillerScenarios = true;

        [Header("Scenario Culling")]
        public bool cullFillerBehindCamera = true;
        public bool cullFillerAfterTarget = true;
        public bool cullCameraNegativeDot = true;

        [System.Serializable]
        public class MyIntEvent : UnityEvent<HeatmapManager> { };

        [Header("Others")]
        public MyIntEvent OnStart;
        public MyIntEvent OnStop;
    }

    [System.Serializable]
    public class ProgressEvent : UnityEvent<float> { };

    [Header("Test Settings")]
    public string directoryName;
    public Tests[] tests;
    public Transform defaultTargetVehiclePrefab;
    public Transform defaultFillerVehiclePrefab;
    public Transform banishmentPoint;
    public bool overrideCullFillerBehindCamera = false;
    public bool cullFillerBehindCamera = false;
    public bool overrideCullFillerAfterTarget = false;
    public bool cullFillerAfterTarget = false;
    public bool overrideCullCameraNegativeDot = false;
    public bool cullCameraNegativeDot = false;
    [Tooltip("Separated by commas")]
    public string testIndices;
    public string roadSetupsList;
    [Header("Progress Bar")]
    public ProgressEvent OnProgress;
    public UnityEvent OnProgressCompleted;
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

    [Header("Photography")]
    public bool takePhotosAfterSimulation;
    public bool hideMeshesInPhotography;
    public PhotographyManager photographyManager;

    [Header("Data Point Saving")]
    public bool savePointsAfterSimulation;
    public GridPointRecorderScript gridPointRecorderScript;
    public UnityEvent OnDatapointSavingStart;
    public UnityEvent OnDatapointSavingFinished;
    //public UnityEvent OnDatapointLoadingStart;
    //public UnityEvent OnDatapointLoadingFinished;

    private HeatmapManager ActiveTarget;
    private int ActiveTestInd = -1;

    private bool SimulationRunning = false;
    private string savingFileName = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        watch = new System.Diagnostics.Stopwatch();
        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));

        //foreach (var t in Permutations.GetPermutationsWithRept(new List<int> { 0, 1 }, tests[0].positions.Length))
        //{
        //    string tempt = "";
        //    foreach (var s in t)
        //    {
        //        tempt += s.ToString() + ",";
        //    }
        //    print(tempt);
        //}
    }

    public void HideMeshDuringPhotos(bool to)
    {
        hideMeshesInPhotography = to;
    }
    public void SetDefaultTargetVehicle(Transform to)
    {
        defaultTargetVehiclePrefab = to;
    }
    public void SetDefaultFillerVehicle(Transform to)
    {
        defaultFillerVehiclePrefab = to;
    }
    public void SetFolderName(string to)
    {
        directoryName = to;
    }
    public void SetFileName(string to)
    {
        savingFileName = to;
    }

    public void SetSimulationsToRun(string simulationsToRun)
    {
        testIndices = simulationsToRun;
    }

    public void SetPhotographyManager(PhotographyManager to)
    {
        photographyManager = to;
    }

    public void SetCullFillerBehindCamera(bool to)
    {
        cullFillerBehindCamera = to;
    }

    public void SetCullFillerAfterTarget(bool to)
    {
        cullFillerAfterTarget = to;
    }

    public void SetCullCameraNegativeDot(bool to)
    {
        cullCameraNegativeDot = to;
    }

    public void StartSimulation()
    {
        if (SimulationRunning)
        {
            Debug.LogError("SIMULATION IS ALREADY RUNNING!");
            return;
        }

        StartCoroutine(RunSimulation());
    }

    IEnumerator RunSimulation()
    {
        SimulationRunning = true;
        foreach (var s in testIndices.Split(','))
        {
            Debug.Log("Running test " + s);  

            if (int.TryParse(s, out int i) && i >= 0 && i < tests.Length)
            {
                if (tests[i].prefab == null)
                    tests[i].prefab = defaultTargetVehiclePrefab;

                if (tests[i].fillerPrefab == null)
                    tests[i].fillerPrefab = defaultFillerVehiclePrefab;

                Transform tempTransform = Instantiate(tests[i].prefab, banishmentPoint.position, banishmentPoint.rotation);

                if ((ActiveTarget = tempTransform.GetComponentInChildren<HeatmapManager>()) == null)
                {
                    Debug.LogError("Scenario " + s + ": " + tests[i].testName + " couldn't be run. No HeatmapManager was found.");
                    continue;
                } 

                ActiveTestInd = i;

                tests[i].OnStart.Invoke(ActiveTarget);

                yield return StartCoroutine(ActiveTarget.HeatmapSetup());

                int currentRoadSetup = 0;

                foreach (var currentSetup in tests[i].roadSetups)
                {
                    currentSetup.gameObject.SetActive(true);
                    progress = currentRoadSetup / tests[i].roadSetups.Length;

                    List<Transform> fillerCars = new List<Transform>();
                    GameObject fillerParent = new GameObject();
                    Transform tempFV = tests[i].fillerPrefab;

                    if (tempFV == null)
                        tempFV = defaultFillerVehiclePrefab;

                    foreach (var p in tests[i].vehiclePositions)
                    {
                        if (p.fillerVehicle)
                        {
                            Transform newfiller = Instantiate(tempFV, p.objectPosition.position, p.objectPosition.rotation, fillerParent.transform);
                            newfiller.gameObject.SetActive(false);
                            fillerCars.Add(newfiller);
                            //Debug.Log(newfiller.name);
                        }
                    }
                    IEnumerable<IEnumerable<int>> indexes = new List<List<int>>();


                    if (fillerCars.Count > 0)
                    {
                        indexes = Permutations.GetPermutationsWithRept(new List<int> { 0, 1 }, fillerCars.Count);
                    }
                    else
                    {
                        List<List<int>> temp = new List<List<int>>
                        {
                            new List<int>()
                        };
                        temp[0].Add(1);
                        indexes = temp;
                    }

                    string debug = "";
                    foreach (var x in indexes)
                    {
                        debug += "{";
                        foreach (var y in x)
                        {
                            debug += y + ",";
                        }
                        debug += "},";
                    }    
                    Debug.Log(debug);
                    int indexesLength = indexes.Count();

                    //CameraPoint[] cameraPositions = new CameraPoint[tests[i].pedestrianViewPoints.Length + tests[i].driverViewPoints.Length];
                    //tests[i].pedestrianViewPoints.CopyTo(cameraPositions, 0);
                    //tests[i].driverViewPoints.CopyTo(cameraPositions, tests[i].pedestrianViewPoints.Length);


                    int cameraIndex = 0;
                    foreach (var index in indexes)
                    {
                        Debug.Log("New layout");
                        bool[] occupiedPositions = new bool[tests[i].vehiclePositions.Length];

                        progress = (float)(currentRoadSetup * indexesLength + cameraIndex) / (float)(indexesLength * tests[i].roadSetups.Length);

                        for (int tp = 0; tp < tests[i].vehiclePositions.Length; tp++)
                        {
                            Debug.Log("New vehicle position");
                            progress = (float)(
                                (currentRoadSetup * indexesLength * tests[i].vehiclePositions.Length)
                                + (cameraIndex * tests[i].vehiclePositions.Length)
                                + tp)
                                / 
                                (float)(
                                indexesLength 
                                * tests[i].roadSetups.Length 
                                * tests[i].vehiclePositions.Length);

                            var t = tests[i].vehiclePositions[tp];

                            int binInd = 0;
                            for (int ind = 0; ind < tests[i].vehiclePositions.Length; ind++)
                            {
                                if (!tests[i].vehiclePositions[ind].fillerVehicle)
                                {
                                    continue;
                                }

                                if (ind == tp)
                                {
                                    occupiedPositions[ind] = true;
                                    binInd++;
                                    continue;
                                }

                                
                                occupiedPositions[ind] = (index.ElementAt(binInd) == 1);
                                fillerCars[binInd].gameObject.SetActive(occupiedPositions[ind]);
                                binInd++;
                            }

                            ActiveTarget.HeatmapLogger.parentObject.transform.position = t.objectPosition.position;
                            ActiveTarget.HeatmapLogger.parentObject.transform.rotation = t.objectPosition.rotation;

                            //string fileName;

                            //foreach (var item in tests[i].pedestrianViewPoints)
                            for (int vi = 0; vi < (tests[i].pedestrianViewPoints.Length + tests[i].vehiclePositions.Length); vi++)
                            {
                                //Debug.Log(string.Format("{0}/{1}", (float)(
                                //    (currentRoadSetup * tests[i].vehiclePositions.Length * indexesLength * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + (tp * indexesLength * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + (cameraIndex * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + vi), (float)(tests[i].roadSetups.Length
                                //    * tests[i].vehiclePositions.Length
                                //    * indexesLength
                                //    * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length)
                                //    )));
                                //progress = (float)(
                                //    (currentRoadSetup * tests[i].vehiclePositions.Length * indexesLength * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + (tp * indexesLength * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + (cameraIndex * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length))
                                //    + vi)
                                //    /
                                //    (float)(tests[i].roadSetups.Length
                                //    * tests[i].vehiclePositions.Length
                                //    * indexesLength
                                //    * (tests[i].vehiclePositions.Length + tests[i].pedestrianViewPoints.Length)
                                //    );
                                CameraPoint item = null;

                                if (vi >= tests[i].pedestrianViewPoints.Length)
                                {
                                    int tempInd = vi - tests[i].pedestrianViewPoints.Length;

                                    if (!tests[i].vehiclePositions[tempInd].useDriverViewPoint
                                        || tp == tempInd
                                        || tests[i].vehiclePositions[tempInd].optionalDriverViewPoint == null 
                                        || occupiedPositions[tempInd])
                                    {
                                        Debug.Log("Skipping camera at " + tests[i].vehiclePositions[tempInd].objectPosition.name);
                                        continue;
                                    }
                                    else
                                    {
                                        item = tests[i].vehiclePositions[tempInd].optionalDriverViewPoint;
                                    }
                                }
                                else
                                {
                                    item = tests[i].pedestrianViewPoints[vi];
                                }


                                ActiveTarget.HeatmapLogger.ClearPointCache();

                                bool tempCFBC = (tests[i].cullFillerBehindCamera && !overrideCullFillerBehindCamera) || (this.cullFillerBehindCamera && overrideCullFillerBehindCamera);
                                bool tempCFAT = (tests[i].cullFillerAfterTarget && !overrideCullFillerAfterTarget) || (this.cullFillerAfterTarget && overrideCullFillerAfterTarget);
                                bool tempCCND = (tests[i].cullCameraNegativeDot && !overrideCullCameraNegativeDot) || (this.cullCameraNegativeDot && overrideCullCameraNegativeDot);

                                if (tempCFBC || tempCFAT || tempCCND)
                                {
                                    Vector3 cameraToTarget = (ActiveTarget.HeatmapLogger.parentObject.transform.position - item.transform.position).normalized;
                                    Vector3 targetToFiller;
                                    Vector3 cameraToFiller;
                                    bool allFillerVisible = true;

                                    foreach (var f in fillerCars)
                                    {
                                        if (f.gameObject.activeSelf)
                                        {
                                            targetToFiller = (f.transform.position - ActiveTarget.HeatmapDisplay.heatmapBoundBox.bounds.center).normalized;
                                            //targetToFiller = (f.transform.position - ActiveTarget.HeatmapLogger.parentObject.transform.position).normalized;
                                            cameraToFiller = (f.transform.position - item.transform.position).normalized;

                                            if ((tempCFBC && Vector3.Dot(cameraToTarget, cameraToFiller) < 0) 
                                                || (tempCFAT && Vector3.Dot(cameraToTarget, targetToFiller) >= 0))
                                            {
                                                allFillerVisible = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (!allFillerVisible)
                                    {
                                        continue;
                                    }
                                }

                                yield return StartCoroutine(item.CycleThroughCameras(tests[i].camera, tempCCND, ActiveTarget.HeatmapLogger.parentObject.transform));

                                if (watch.ElapsedTicks > tickBudget)
                                {
                                    yield return null;
                                    watch.Restart();
                                }

                                //if (tests[i].saveFileWithLogs)
                                //{
                                //    fileName = string.Format("{0}_{1},{2}_{3}", tests[i].testName, t.objectPosition.position.ToSafeString(), t.objectPosition.eulerAngles.ToSafeString(), cameraIndex);

                                //    ActiveTarget.HeatmapLogger.fileName = fileName;
                                //    ActiveTarget.HeatmapLogger.SaveFile(directoryName + "/" + tests[i].testName + "/");

                                //    cameraIndex++;
                                //}

                                yield return StartCoroutine(ActiveTarget.LoadCurrentPointsIntoMatrix());
                            }

                        }

                        cameraIndex++;
                    }

                    OnProgressCompleted.Invoke();

                    currentSetup.gameObject.SetActive(false);
                    Destroy(fillerParent);

                    currentRoadSetup++;
                }

                

                ActiveTarget.HeatmapLogger.parentObject.transform.position = banishmentPoint.position;
                ActiveTarget.HeatmapLogger.parentObject.transform.rotation = banishmentPoint.rotation;


                yield return StartCoroutine(ActiveTarget.DisplayHeatmap());

                if (savePointsAfterSimulation)
                {
                    OnDatapointSavingStart.Invoke();
                    yield return StartCoroutine(gridPointRecorderScript.SetDataPoints(ActiveTarget.HeatmapDisplay.GetPointDictionary()));
                    gridPointRecorderScript.SaveFile(directoryName, savingFileName);
                    OnDatapointSavingFinished.Invoke();
                }

                if (takePhotosAfterSimulation)
                {
                    yield return StartCoroutine(TakePhoto(tests[i]));
                }

                tests[i].OnStop.Invoke(ActiveTarget);

                OnProgress.Invoke(1);

                //Destroy(ActiveTarget.HeatmapLogger.parentObject);

                OnProgressCompleted.Invoke();
            }
        }

        SimulationRunning = false;
    }

    private IEnumerator TakePhoto(Tests test)
    {
        if (string.IsNullOrEmpty(savingFileName))
        {
            savingFileName = test.testName;
        }

        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName + "/");
        }

        if (hideMeshesInPhotography)
            photographyManager.HideMeshes(ActiveTarget.HeatmapLogger.parentObject.transform);

        Debug.Log("Saving photo to " + Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".png");

        yield return StartCoroutine(photographyManager.TakePhotosCoroutine(Application.persistentDataPath + "/" + directoryName + "/", savingFileName, ".png"));
    }

    private void SaveCurrentPoints(HeatmapDisplay displayManager)
    {
        gridPointRecorderScript.SetDataPoints(displayManager.GetPointDictionary());
        gridPointRecorderScript.SaveFile(this.directoryName, this.savingFileName);
    }

    public void Execute()
    {
        if (ActiveTestInd >= 0)
            tests[ActiveTestInd].heatmapSender.CameraViewHeat();
    }
}
