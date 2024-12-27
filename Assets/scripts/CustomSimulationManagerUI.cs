using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.IO;
using UnityEngine.SceneManagement;

public class CustomSimulationManagerUI : MonoBehaviour
{
    public TMP_InputField vehicleID;
    public TMP_InputField fillerID;
    public TMP_InputField setupIDs;
    public TMP_InputField cameraPoints;
    public TMP_InputField vehiclePositions;
    public TMP_InputField fillerPositions;

    public TMP_InputField minRayDistance;
    public TMP_InputField maxRayDistance;

    public TMP_InputField vehicleAmountInput;

    public TMP_InputField fileNameInput;

    public UnityEvent OnSimulationStart;

    [System.Serializable]
    public class BooleanUnityEvent : UnityEvent<bool> { };
    public BooleanUnityEvent OnFileValidityChecked;
    

    public void UIUpdated()
    {
        setupIDs.text = setupIDs.text.Trim();
        vehicleID.text = vehicleID.text.Trim();
        vehiclePositions.text = vehiclePositions.text.Trim();
        vehicleAmountInput.text = vehicleAmountInput.text.Trim();
        fillerID.text = fillerID.text.Trim();
        fillerPositions.text = fillerPositions.text.Trim();
        cameraPoints.text = cameraPoints.text.Trim();
        minRayDistance.text = minRayDistance.text.Trim();
        maxRayDistance.text = maxRayDistance.text.Trim();
        vehicleAmountInput.text = vehicleAmountInput.text.Trim();

        string fileName = string.Format("{0}_{1}_{2}_{3}x{4}_{5}_{6}_({7}to{8})", 
            setupIDs.text, 
            vehicleID.text, 
            vehiclePositions.text, 
            vehicleAmountInput.text, 
            fillerID.text, 
            fillerPositions.text, 
            cameraPoints.text, 
            minRayDistance.text, 
            maxRayDistance.text);

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '-');
        }

        fileNameInput.text = fileName;

        OnFileValidityChecked.Invoke(CheckIfPathIsValid(fileName));
    }

    public void StartSimulation()
    {
        //UIUpdated();

        if (!int.TryParse(vehicleAmountInput.text, out var vehicleAmount))
        {
            Debug.LogError("No amount of records added!");
            return;
        }

        if (!float.TryParse(minRayDistance.text, out var minDis))
        {
            Debug.LogError("No minimum distance set. Set to default value (0).");
            minDis = 0;
        }
        
        if (!float.TryParse(maxRayDistance.text, out var maxDis))
        {
            Debug.LogError("No maximum distance set. Set to default value (Infinite).");
            maxDis = Mathf.Infinity;
        }

        CustomSimulationManager.Instance.SetSenderMinMax(minDis, maxDis);
        CustomSimulationManager.Instance.numberOfVehicles = vehicleAmount;
        CustomSimulationManager.Instance.savingFileName = fileNameInput.text;
        CustomSimulationManager.Instance.StartSimulation(setupIDs.text, vehicleID.text, vehiclePositions.text, fillerID.text, fillerPositions.text, cameraPoints.text);
        OnSimulationStart.Invoke();
    }

    public void LoadScene(string withName)
    {
        SceneManager.LoadScene(withName);        
    }

    public bool CheckIfPathIsValid(string fileName)
    {
        System.IO.FileInfo fi = null;
        try
        {
            fi = new System.IO.FileInfo(fileName);
        }
        catch (System.ArgumentException) { }
        catch (System.IO.PathTooLongException) { }
        catch (System.NotSupportedException) { }
        if (ReferenceEquals(fi, null))
        {
            // file name is not valid
            return false;
        }
        else
        {
            // file name is valid... May check for existence by calling fi.Exists.
            return true;
        }
    }
}
