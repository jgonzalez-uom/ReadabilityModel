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
    private Vector3[] points;

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
        points[currentInd++] = pt;
        //print(points[currentInd]);
        //currentInd++;
    }

    public void SaveFile(string subDirectory = "")
    {

        if (!Directory.Exists(Application.dataPath + "/" + subDirectory + fileName + ".txt"))
        {

            Directory.CreateDirectory(Application.dataPath + "/" + subDirectory);
        }

        string content = "";

        for (int i = 0; i < currentInd; i++)
        {
            content += (points[i]).ToString() + ",";
        }

        File.WriteAllText(Application.dataPath + "/" + subDirectory + fileName + ".txt", content);
    }
}

