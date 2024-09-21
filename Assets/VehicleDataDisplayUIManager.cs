using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using UnityEditor.Rendering;
using static MenuScript;
using TMPro;

public class VehicleDataDisplayUIManager : MonoBehaviour
{
    [Header("References")]
    public VehicleDataDisplayManager mainManager;
    public FileLoadingButtonScript buttonPrefab;
    public RectTransform foundFiles;
    public RectTransform loadedFiles;

    [System.Serializable]
    public class ItemSwappingEvent : UnityEvent<FileLoadingButtonScript> { };
    [Header("Events to move items")]
    public ItemSwappingEvent OnItemMovedToFound;
    public ItemSwappingEvent OnItemMovedToLoaded;
    List<FileLoadingButtonScript> foundFilesList = new List<FileLoadingButtonScript>();
    List<FileLoadingButtonScript> loadedFilesList = new List<FileLoadingButtonScript>();

    [Header("Selectable Object")]
    public List<SimulationIndexList> simulationIndexList = new List<SimulationIndexList>();
    public UnityEvent OnObjectValueChanged;
    public TMP_Dropdown objectSelectionDropdown;

    [Header("Camera Selection")]
    public SimulationIndexList[] photographyManagers;
    public TMP_Dropdown cameraSelectionDropdown;

    // Start is called before the first frame update
    void Start()
    {
        LoadFilesFromPath(Application.persistentDataPath + "/" + mainManager.directoryName + "/");
        PopulateObjectDropdown();
        PopulateCameraDropdown();
    }

    public void PopulateObjectDropdown()
    {
        objectSelectionDropdown.ClearOptions();

        List<string> list = new List<string>();

        foreach (var s in simulationIndexList)
            list.Add(s.displayName);

        objectSelectionDropdown.AddOptions(list);

        SetVehicle();
    }

    public void PopulateCameraDropdown()
    {
        cameraSelectionDropdown.ClearOptions();

        List<string> list = new List<string>();

        foreach (var s in photographyManagers)
            list.Add(s.displayName);

        cameraSelectionDropdown.AddOptions(list);

        SetCamera(cameraSelectionDropdown.value);
    }

    public void SetCamera(int ind)
    {
        //foreach (var m in photographyManagers)
        //{
        //    m.gameObject.SetActive(false);
        //}

        if (ind >= 0 && ind < photographyManagers.Length)
        {
            //mainManager.photographyManager = photographyManagers[ind];
            //mainManager.photographyManager.gameObject.SetActive(true);
            photographyManagers[ind].OnSelected.Invoke();
        }
        else
        {
            Debug.Log("Photography manager not found: " + ind);
        }
    }

    public void SetVehicle()
    {
        if (simulationIndexList.Count <= objectSelectionDropdown.value)
            return;

        OnObjectValueChanged.Invoke();
        simulationIndexList[objectSelectionDropdown.value].OnSelected.Invoke();
    }

    public void LoadSelection()
    {
        string[] fileNames = new string[loadedFilesList.Count];
        
        for (int n = 0; n < fileNames.Length; n++)
        {
            fileNames[n] = loadedFilesList[n].fullName;
        }

        mainManager.gridPointRecorderScript.LoadFiles(mainManager.directoryName, fileNames);
    }

    void LoadFilesFromPath(string path)
    {
        DirectoryInfo info;
        FileInfo[] fileInfo;
        try
        {
            info = new DirectoryInfo(path);
            fileInfo = info.GetFiles();
        }
        catch 
        {
            Debug.LogError("Directory couldn't be accessed. Does it exist?");
            return;
        }

        foreach (FileInfo file in fileInfo)
        {
            if (file.Extension == ".json")
            {
                var temp = Instantiate(buttonPrefab, foundFiles);

                temp.name = file.Name;

                temp.text.text = file.Name;

                temp.fileName = file.Name;

                temp.fullName = file.FullName;

                //temp.button.onClick.AddListener(() => AddItemToLoaded(temp));
                temp.AddEvent(() => AddItemToLoaded(temp));

                foundFilesList.Add(temp);
            }
        }
    }

    public void AddItemToLoaded(FileLoadingButtonScript item)
    {
        item.transform.SetParent(loadedFiles);
        loadedFilesList.Add(item);
        foundFilesList.Remove(item);

        item.ClearEvent();
        item.AddEvent(() => AddItemToFound(item));

        OnItemMovedToLoaded.Invoke(item);
    }

    public void AddItemToFound(FileLoadingButtonScript item)
    {
        item.transform.SetParent(foundFiles);
        loadedFilesList.Remove(item);
        foundFilesList.Add(item);

        item.ClearEvent();
        item.AddEvent(() => AddItemToLoaded(item));

        OnItemMovedToFound.Invoke(item);
    }
}
