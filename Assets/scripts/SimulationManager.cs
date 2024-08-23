using System.Collections;
using System.Collections.Generic;
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
            public bool fillerVehicle;
        }

        public string testName;
        public Transform prefab;
        public Transform fillerPrefab;
        public VehiclePosition[] positions;
        public CameraPoint[] cameraPoints;
        public Transform[] roadSetups;
        public Camera camera;
        public HeatmapSender heatmapSender;

        public bool saveFileWithLogs;

        [System.Serializable]
        public class MyIntEvent : UnityEvent<HeatmapManager> { };

        public MyIntEvent OnStart;
        public MyIntEvent OnStop;
    }

    [Header("Test Settings")]
    public string directoryName;
    public Tests[] tests;
    public Transform banishmentPoint;
    [Tooltip("Separated by commas")]
    public string testIndices;

    [Header("Photography")]
    public bool takePhotosAfterSimulation;
    public bool hideMeshesInPhotography;
    public PhotographyManager photographyManager;

    private HeatmapManager ActiveTarget;
    private int ActiveTestInd = -1;

    private bool SimulationRunning = false;

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
                Transform tempTransform = Instantiate(tests[i].prefab, banishmentPoint.position, banishmentPoint.rotation);

                if ((ActiveTarget = tempTransform.GetComponentInChildren<HeatmapManager>()) == null)
                {
                    Debug.LogError("Scenario " + s + ": " + tests[i].testName + " couldn't be run. No HeatmapManager was found.");
                    continue;
                } 
                    

                ActiveTestInd = i;

                tests[i].OnStart.Invoke(ActiveTarget);

                yield return StartCoroutine(ActiveTarget.HeatmapSetup());

                foreach (var currentSetup in tests[i].roadSetups)
                {
                    currentSetup.gameObject.SetActive(true);

                    List<Transform> fillerCars = new List<Transform>();
                    GameObject fillerParent = new GameObject();

                    foreach (var p in tests[i].positions)
                    {
                        if (p.fillerVehicle)
                        {
                            Transform newfiller = Instantiate(tests[i].fillerPrefab, p.objectPosition.position, p.objectPosition.rotation, fillerParent.transform);
                            newfiller.gameObject.SetActive(false);
                            fillerCars.Add(newfiller);
                            Debug.Log(newfiller.name);
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

                    foreach (var index in indexes)
                    {
                        int cameraIndex = 0;
                        for (int tp = 0; tp < tests[i].positions.Length; tp++)
                        {
                            int binInd = 0;
                            for (int ind = 0; ind < tests[i].positions.Length; ind++)
                            {
                                if (!tests[i].positions[ind].fillerVehicle)
                                {
                                    continue;
                                }

                                if (ind == tp)
                                {
                                    binInd++;
                                    continue;
                                }

                                fillerCars[binInd].gameObject.SetActive(index.ElementAt(binInd) == 1);
                                binInd++;
                            }

                            var t = tests[i].positions[tp];
                            string fileName;

                            ActiveTarget.transform.position = t.objectPosition.position;
                            ActiveTarget.transform.rotation = t.objectPosition.rotation;


                            foreach (var item in tests[i].cameraPoints)
                            {
                                ActiveTarget.HeatmapLogger.ClearPointCache();

                                yield return StartCoroutine(item.CycleThroughCameras(tests[i].camera));

                                if (watch.ElapsedTicks > tickBudget)
                                {
                                    yield return null;
                                    watch.Restart();
                                }

                                if (tests[i].saveFileWithLogs)
                                {
                                    fileName = string.Format("{0}_{1},{2}_{3}", tests[i].testName, t.objectPosition.position.ToSafeString(), t.objectPosition.eulerAngles.ToSafeString(), cameraIndex);

                                    ActiveTarget.HeatmapLogger.fileName = fileName;
                                    ActiveTarget.HeatmapLogger.SaveFile(directoryName + "/" + tests[i].testName + "/");

                                    cameraIndex++;
                                }

                                yield return StartCoroutine(ActiveTarget.LoadCurrentPointsIntoMatrix());
                            }

                        }
                    }

                    currentSetup.gameObject.SetActive(false);
                    Destroy(fillerParent);

                }

                

                ActiveTarget.transform.position = banishmentPoint.position;
                ActiveTarget.transform.rotation = banishmentPoint.rotation;


                yield return StartCoroutine(ActiveTarget.DisplayHeatmap());

                if (takePhotosAfterSimulation)
                {
                    if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName + "/");
                    }

                    if (hideMeshesInPhotography)
                        photographyManager.HideMeshes(ActiveTarget.transform);
                    
                    photographyManager.TakePhotos(Application.persistentDataPath + "/" + directoryName + "/", tests[i].testName, ".png");
                }

                tests[i].OnStop.Invoke(ActiveTarget);
            }
        }

        SimulationRunning = false;
    }

    public void Execute()
    {
        if (ActiveTestInd >= 0)
            tests[ActiveTestInd].heatmapSender.CameraViewHeat();
    }
}
