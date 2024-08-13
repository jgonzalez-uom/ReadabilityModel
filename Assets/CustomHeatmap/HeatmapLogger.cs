using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HeatmapLogger : MonoBehaviour
{
    [Header("File Info")]
    public string fileName;

    [Header("Values")]
    public int maxPoints = 5000;
    private int currentInd = 0;

    //MOVE THIS TO THE MANAGER
    //public Vector3[] points;
    public List<Vector3> points = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        //points = new Vector3[maxPoints];

        foreach (HeatmapReceiver r in GetComponentsInChildren<HeatmapReceiver>())
        {
            r.SetLogger(this);
        }
    }

    public void LogPoint(Vector3 pt)
    {
        //if (currentInd >= points.Length)
        //    return;
        if (points.Count >= maxPoints)
            return;
        Debug.Log("Added point " + pt.ToString()); 
        points.Add(pt);
        //points[currentInd++] = pt;
        //print(points[currentInd]);
        //currentInd++;
    }

    public void ClearPointCache()
    {
        //points = new Vector3[maxPoints];
        //currentInd = 0;
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

        //points = new Vector3[currentInd];
        //for (int i = 0; i < currentInd; i++)
        //    points[i] = newPoints[i];

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
        //Debug.Log("Parsed: " + result.ToString());
        return result;
    }
}

