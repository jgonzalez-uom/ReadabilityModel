using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class AnalyticsScript : MonoBehaviour
{
    long captureTotal = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        HeatmapSender.OnCapture += CountCapture;
    }

    private void OnDisable()
    {
        HeatmapSender.OnCapture -= CountCapture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CountCapture()
    {
        captureTotal++;
    }

    public void PrintResults()
    {
        string fileName = System.DateTime.UtcNow.ToString("HHmmddMMMMyyyy") + transform.name;
        string path = Application.persistentDataPath + "/" + fileName + ".png";

        //Write some text to the test.txt file

        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine("Test");

        writer.Close();
    }
}
