using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

public class VehicleDataDisplayManager : MonoBehaviour
{

    public GridPointRecorderScript gridPointRecorderScript;
    public PhotographyManager photographyManager;
    private HeatmapManager ActiveTarget;
    public Transform vehiclePrefab;
    public Transform vehicleTransformProperties;
    public string directoryName;
    public string fileName = string.Empty;
    public bool hideMeshesInPhotography;

    public UnityEvent OnDisplayStart;
    public UnityEvent OnDisplayEnd;

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
        if (!vehicleTransformProperties)
            vehicleTransformProperties = transform;

        if ((ActiveTarget = Instantiate(vehiclePrefab, vehicleTransformProperties.position, vehicleTransformProperties.rotation).GetComponentInChildren<HeatmapManager>()) == null)
        {
            Debug.LogError("Vehicle Prefab does not contain a HeatmapManager!");
            return;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = ActiveTarget.name;
        }
    }

    public void SetDataIntoVehicle()
    {
        StartCoroutine(SetVehicleGridPoints());
    }

    IEnumerator SetVehicleGridPoints()
    { 
        //gridPointRecorderScript.LoadFile(directoryName, fileName);

        OnDisplayStart.Invoke();

        yield return StartCoroutine(ActiveTarget.HeatmapSetup());

        ActiveTarget.HeatmapDisplay.AddPointsToDictionary(gridPointRecorderScript.GetDataPoints());

        ActiveTarget.HeatmapDisplay.SetMaxHeat(gridPointRecorderScript.GetMaxValueInSaveFile());

        yield return StartCoroutine(ActiveTarget.DisplayHeatmap());

        OnDisplayEnd.Invoke();

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

        Debug.Log("Saving photo to " + Application.persistentDataPath + "/" + directoryName + "/" + fileName + ".json");

        yield return StartCoroutine(photographyManager.TakePhotosCoroutine(Application.persistentDataPath + "/" + directoryName + "/", fileName, ".json"));
    }
}
