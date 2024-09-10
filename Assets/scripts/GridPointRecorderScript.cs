using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GridPointRecorderScript : MonoBehaviour
{
    [System.Serializable]
    public class DataPoint
    {
        public Vector3Int point;
        public long value;

        public DataPoint(Vector3Int point, long value)
        {
            this.point = point;
            this.value = value;
        }
    }

    [System.Serializable]
    public class DataSavefile
    {
        List<DataPoint> points = new List<DataPoint>();

        public void AddPoint(DataPoint point)
        {
            points.Add(point);
        }

        public DataPoint GetPoint(int index)
        {
            if (points.Count > index || index < 0)
            {
                Debug.LogError("Point couldn't be loaded from DataSaveFile: " + index);
                return null;
            }

            return points[index];
        }

        public Dictionary<Vector3Int, long> GetDataPointsAsDictionary()
        {
            Dictionary<Vector3Int, long> output = new Dictionary<Vector3Int, long>();

            foreach (DataPoint point in points)
            {
                output.Add(point.point, point.value);
            }

            return output;
        }
    }

    private DataSavefile _saveFile;

    public void SetDataPoints(Dictionary<Vector3Int, long> input)
    {
        _saveFile = new DataSavefile();

        foreach (KeyValuePair<Vector3Int, long> pair in input)
        {
            _saveFile.AddPoint(new DataPoint(pair.Key, pair.Value));
        }
    }

    public Dictionary<Vector3Int, long> GetDataPoints()
    {
        if (_saveFile == null)
            return _saveFile.GetDataPointsAsDictionary();

        Debug.LogError("NO Data File found. Did you use LoadFile first?");
        return null;
    }

    public void SaveFile(string directoryName, string savingFileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName + "/");
        }
        string dataFile = JsonUtility.ToJson(_saveFile);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json", dataFile);

        Debug.Log("Saved point file to " + Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json");
    }

    public void LoadFile(string directoryName, string savingFileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Debug.LogError("Directory does not exist: " + Application.persistentDataPath + "/" + directoryName + "/");
        }

        if (!System.IO.File.Exists(Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json"))
        {
            Debug.LogError("File does not exist: " + Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json");
        }

        string fileContent = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json");

        _saveFile = JsonUtility.FromJson<DataSavefile>(fileContent);
    }

    public void LoadFiles(string directoryName, string[] fileNames)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Debug.LogError("Directory does not exist: " + Application.persistentDataPath + "/" + directoryName + "/");
        }

        Dictionary<Vector3Int, long> keyValuePairs = new Dictionary<Vector3Int, long>();

        foreach (string savingFileName in fileNames)
        {
            if (!System.IO.File.Exists(Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json"))
            {
                Debug.LogError("File does not exist: " + Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json");
                continue;
            }

            string fileContent = System.IO.File.ReadAllText(Application.persistentDataPath + "/" + directoryName + "/" + savingFileName + ".json");

            var temp = JsonUtility.FromJson<DataSavefile>(fileContent);

            foreach (KeyValuePair<Vector3Int, long> pair in temp.GetDataPointsAsDictionary())
            {
                if (keyValuePairs.ContainsKey(pair.Key))
                {
                    keyValuePairs[pair.Key] += pair.Value;
                }
                else
                {
                    keyValuePairs.Add(pair.Key, pair.Value);
                }
            }
        }

        _saveFile = new DataSavefile();
        
        foreach (KeyValuePair<Vector3Int, long> pair in keyValuePairs)
        {
            _saveFile.AddPoint(new DataPoint(pair.Key, pair.Value));

        }
    }
}
