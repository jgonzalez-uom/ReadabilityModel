using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static string simulationsToRun;

    [System.Serializable]
    public class SimulationSettings : UnityEvent<string> { };
    public SimulationSettings OnSceneStart;

    private void Start()
    {
        OnSceneStart.Invoke(simulationsToRun);
    }

    public void SetSimulationsIndList(string to)
    {
        simulationsToRun = to;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Close()
    {
        Application.Quit();
    }
}
