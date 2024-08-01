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
    public Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        points = new Vector3[maxPoints];

        foreach (HeatmapReceiver r in GetComponentsInChildren<HeatmapReceiver>())
        {
            r.SetLogger(this);
        }
    }

    public void LogPoint(Vector3 pt)
    {
        if (currentInd >= points.Length)
            return;

        points[currentInd++] = pt;
        //print(points[currentInd]);
        //currentInd++;
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


        string[] points = File.ReadAllText(filePath).Split(';');
        Vector3[] newPoints = new Vector3[maxPoints];
        currentInd = 0;

        foreach (string s in points)
        {
            newPoints[currentInd++] = ParseVector3(s);
        }
    }



    Vector3 ParseVector3(string str)
    {
        string[] values = str.Trim().Trim(new char[] {'(',')' }).Split(',');

        if (values.Length < 3 )
        {
            Debug.LogError("ERROR PARSING TO VECTOR: " + str);
            return Vector3.zero;
        }

        return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }
}

