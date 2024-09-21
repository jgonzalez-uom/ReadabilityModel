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
    public Transform vehicleTransformParent;
    public string directoryName;
    public string fileName = string.Empty;
    public bool hideMeshesInPhotography;
    public bool centerMesh;

    public UnityEvent OnDisplayStart;
    public UnityEvent OnDisplayEnd;

    public UnityEvent OnPhotoStart;
    public UnityEvent OnPhotoEnd;

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

        vehicleTransformParent.position = ActiveTarget.HeatmapDisplay.heatmapBoundBox.bounds.center;

        ActiveTarget.HeatmapDisplay.referencePoint.transform.SetParent(vehicleTransformParent, true);
        ActiveTarget.HeatmapDisplay.referencePoint.transform.localScale = Vector3.one;

        if (centerMesh)
        {
            //ActiveTarget.HeatmapDisplay.referencePoint.transform.Translate(
            //    vehicleTransformProperties.position - vehicleTransformParent.position
            //    );

            vehicleTransformParent.position = new Vector3(vehicleTransformProperties.position.x, vehicleTransformParent.position.y, vehicleTransformProperties.position.z);
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

    public void SetPhotoManager(PhotographyManager to)
    {
        photographyManager = to;
    }

    public void TakePhoto()
    {
        StartCoroutine(TakePhotoCoroutine());
    }

    private IEnumerator TakePhotoCoroutine()
    {
        OnPhotoStart.Invoke();

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

        OnPhotoEnd.Invoke();
    }
}
