using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public string simulationsToRun;

    [System.Serializable]
    public class SimulationSettings : UnityEvent<string> { };
    public SimulationSettings OnSceneStart;

    [System.Serializable]
    public class SimulationIndexList
    {
        public string displayName;
        public string indexList;
        public UnityEvent OnSelected;
    }

    public List<SimulationIndexList> simulationIndexList = new List<SimulationIndexList>();
    public SimulationSettings OnValueChanged;

    public TMP_Dropdown selectionDropdown;

    private void Start()
    {
        OnSceneStart.Invoke(simulationsToRun);
    }

    public void PopulateDropdown()
    {
        selectionDropdown.ClearOptions();

        List<string> list = new List<string>();

        foreach (var s in simulationIndexList)
            list.Add(s.displayName);

        selectionDropdown.AddOptions(list);

        SetSimulationsIndList();
    }

    public void SetSimulationsIndList()
    {
        if (simulationIndexList.Count <= selectionDropdown.value)
            return;

        simulationsToRun = simulationIndexList[selectionDropdown.value].indexList;

        OnValueChanged.Invoke(simulationsToRun);
        simulationIndexList[selectionDropdown.value].OnSelected.Invoke();
    }

    public void SetSimulationsIndList(int to)
    {
        if (simulationIndexList.Count <= to)
            return;

        Debug.Log("Setting selection to " + simulationIndexList[to].displayName);
        
        simulationsToRun
            = simulationIndexList[to].indexList;

        OnValueChanged.Invoke(simulationsToRun);
        simulationIndexList[to].OnSelected.Invoke();
    }

    public void SetSimulationsIndList(string to)
    {
        simulationsToRun = to;

        OnValueChanged.Invoke(simulationsToRun);
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
