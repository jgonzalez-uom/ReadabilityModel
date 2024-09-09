using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VehicleDataDisplayManager : MonoBehaviour
{

    public GridPointRecorderScript gridPointRecorderScript;
    public PhotographyManager photographyManager;
    private HeatmapManager ActiveTarget;
    public Transform vehiclePrefab; 
    public string directoryName;
    public string fileName = string.Empty;
    public bool hideMeshesInPhotography;

    public void SetVehiclePrefab(Transform to)
    {
        vehiclePrefab = to;
    }

    public void SetFileName(string to)
    {
        fileName = to; 
    }

    public void SpawnVehiclePrefab()
    {
        if ((ActiveTarget = Instantiate(vehiclePrefab).GetComponentInChildren<HeatmapManager>()) == null)
        {
            Debug.LogError("Vehicle Prefab does not contain a HeatmapManager!");
            return;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = ActiveTarget.name;
        }
    }

    public void LoadDataIntoVehicle()
    {
        StartCoroutine(LoadVehicleGridPoints());
    }

    IEnumerator LoadVehicleGridPoints()
    {
        gridPointRecorderScript.LoadFile(directoryName, fileName);

        ActiveTarget.HeatmapDisplay.SetPointDictionary(gridPointRecorderScript.GetDataPoints());

        yield return StartCoroutine(ActiveTarget.HeatmapSetup());

        yield return StartCoroutine(ActiveTarget.DisplayHeatmap());

    }

    public void TakePhoto()
    {
        StartCoroutine(TakePhotoCoroutine());
    }

    private IEnumerator TakePhotoCoroutine()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = ActiveTarget.name;
        }

        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName + "/");
        }

        if (hideMeshesInPhotography)
            photographyManager.HideMeshes(ActiveTarget.HeatmapLogger.parentObject.transform);

        Debug.Log("Saving photo to " + Application.persistentDataPath + "/" + directoryName + "/" + fileName + ".png");

        yield return StartCoroutine(photographyManager.TakePhotosCoroutine(Application.persistentDataPath + "/" + directoryName + "/", fileName, ".png"));
    }
}
