using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HeatmapLogger : MonoBehaviour
{
    public GameObject parentObject;

    [Header("File Info")]
    public string fileName;

    [Header("Values")]
    public int maxPoints = 5000;
    private int currentInd = 0;

    //MOVE THIS TO THE MANAGER
    public List<Vector3> points = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        if (parentObject == null)
            parentObject = this.transform.parent.gameObject;

        foreach (HeatmapReceiver r in parentObject.GetComponentsInChildren<HeatmapReceiver>())
        {
            r.SetLogger(this);
        }
    }

    public void LogPoint(Vector3 pt)
    {
        if (points.Count >= maxPoints)
            return;

        points.Add(pt);
    }

    public void ClearPointCache()
    {
        points.Clear();
    }

    public void SaveFile(string subDirectory = "")
    {
        string filepath = Application.dataPath + "/" + subDirectory + fileName + ".txt";

        if (!Directory.Exists(filepath))
        {

            Directory.CreateDirectory(Application.dataPath + "/" + subDirectory);
        }

        string content = "";

        for (int i = 0; i < currentInd; i++)
        {
            content += (points[i]).ToString() + ";";
        }

        Debug.Log("Saved at: " + filepath);

        File.WriteAllText(Application.dataPath + "/" + subDirectory + fileName + ".txt", content);
    }

    public void LoadFile(string subDirectory = "")
    {
        string filePath = Application.dataPath + "/" + subDirectory + fileName + ".txt";

        if (!Directory.Exists(Application.dataPath + "/" + subDirectory))
        {
            Debug.LogError("PATH DOESN'T EXIST: " + Application.dataPath + "/" + subDirectory);
            return;
        }

        LoadFileName(filePath);
    }

    public void LoadFileName(string filePath)
    {

        string readFile = File.ReadAllText(filePath);

        if (!string.IsNullOrEmpty(readFile))
        {
            Debug.Log("Successfully loaded " + filePath);
        }
        else
        {
            Debug.Log("Failed to load " + filePath + " or file is empty.");
        }

        string[] textPoints = readFile.Split(';');
        Vector3[] newPoints = new Vector3[maxPoints];
        currentInd = 0;

        foreach (string s in textPoints)
        {
            if (currentInd >= newPoints.Length)
                break;

            newPoints[currentInd++] = ParseVector3(s);
        }

        for (int i = 0; i < currentInd; i++)
            points.Add(newPoints[i]);
    }



    Vector3 ParseVector3(string str)
    {
        string[] values = str.Trim().Trim(new char[] {'(',')' }).Split(',');

        if (values.Length < 3 )
        {
            Debug.LogError("ERROR PARSING TO VECTOR: " + str);
            return Vector3.zero;
        }

        Vector3 result = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        return result;
    }
}

