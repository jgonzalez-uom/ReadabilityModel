using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public static SimulationManager Instance;


    private System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    [System.Serializable]
    public class Tests
    {
        public string testName;
        public HeatmapManager prefab;
        public Transform[] positions;
        public CameraPoint[] cameraPoints;
        public HeatmapSender heatmapSender;

        public bool saveFileWithLogs;
    }

    [Header("Test Settings")]
    public string directoryName;
    public Tests[] tests;
    public Transform banishmentPoint;
    [Tooltip("Separated by commas")]
    public string testIndices;

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
            if (int.TryParse(s, out int i) && i >= 0 && i < tests.Length)
            {
                ActiveTarget = Instantiate(tests[i].prefab, banishmentPoint.position, banishmentPoint.rotation);
                ActiveTestInd = i;

                yield return StartCoroutine(ActiveTarget.HeatmapSetup());

                int cameraIndex = 0;
                foreach (var t in tests[i].positions)
                {
                    string fileName;

                    ActiveTarget.transform.position = t.position;
                    ActiveTarget.transform.rotation = t.rotation;

                    foreach (var item in tests[i].cameraPoints)
                    {
                        ActiveTarget.HeatmapLogger.ClearPointCache();

                        yield return StartCoroutine(item.CycleThroughCameras(Camera.main));

                        if (watch.ElapsedTicks > tickBudget)
                        {
                            yield return null;
                            watch.Restart();
                        }

                        if (tests[i].saveFileWithLogs)
                        {
                            fileName = string.Format("{0}_{1},{2}_{3}", tests[i].testName, t.position.ToSafeString(), t.eulerAngles.ToSafeString(), cameraIndex);

                            ActiveTarget.HeatmapLogger.fileName = fileName;
                            ActiveTarget.HeatmapLogger.SaveFile(directoryName + "/" + tests[i].testName + "/");

                            cameraIndex++;
                        }

                        yield return StartCoroutine(ActiveTarget.LoadCurrentPointsIntoMatrix());
                    }
                }

                ActiveTarget.transform.position = banishmentPoint.position;
                ActiveTarget.transform.rotation = banishmentPoint.rotation;


                yield return StartCoroutine(ActiveTarget.DisplayHeatmap());
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
