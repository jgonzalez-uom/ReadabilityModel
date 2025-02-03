using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;

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
        public string FileName;
        public long maxValue = 0;
        public List<DataPoint> points = new List<DataPoint>();

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
                //Debug.Log("Loading point " + point.point.ToString() + ": " + point.value.ToString());  
                output.Add(point.point, point.value);
            }
            return output;
        }
    }

    private DataSavefile _saveFile;
    private System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    private void Start()
    {

        watch = new System.Diagnostics.Stopwatch();
        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
    }

    public IEnumerator SetDataPoints(Dictionary<Vector3Int, long> input)
    {
        _saveFile = new DataSavefile();
        //string finalOutput = "";

        watch.Restart();

        foreach (KeyValuePair<Vector3Int, long> pair in input)
        {
            //finalOutput += (string.Format("{0}:{1}", pair.Key, pair.Value.ToString())) + "\n";
            _saveFile.AddPoint(new DataPoint(pair.Key, pair.Value));

            if (pair.Value > _saveFile.maxValue)
            {
                _saveFile.maxValue = pair.Value;
            }

            if (watch.ElapsedTicks > tickBudget)
            {
                yield return null;
                watch.Restart();
            }
        }
        //Debug.Log(finalOutput);
    }

    public Dictionary<Vector3Int, long> GetDataPoints()
    {
        if (_saveFile != null)
            return _saveFile.GetDataPointsAsDictionary();

        Debug.LogError("NO Data File found. Did you use LoadFile first?");
        return null;
    }

    public long GetMaxValueInSaveFile()
    {
        return _saveFile.maxValue;
    }

    public void SaveFile(string directoryName, string savingFileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + directoryName + "/");
        }
        _saveFile.FileName = savingFileName;
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

    public void LoadFiles(string directoryName, string[] fileNames, float[] weights = null)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/" + directoryName + "/"))
        {
            Debug.LogError("Directory does not exist: " + Application.persistentDataPath + "/" + directoryName + "/");
        }

        if (weights == null)
        {
            weights = new float[fileNames.Length];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = 1;
        }

        _saveFile = new DataSavefile();
        Dictionary<Vector3Int, long> keyValuePairs = new Dictionary<Vector3Int, long>();

        int weightIndex = 0;

        foreach (string savingFileName in fileNames)
        {
            if (!System.IO.File.Exists(savingFileName))
            {
                Debug.LogError("File does not exist: " + savingFileName);
                continue;
            }

            string fileContent = System.IO.File.ReadAllText(savingFileName);

            var temp = JsonUtility.FromJson<DataSavefile>(fileContent);

            foreach (KeyValuePair<Vector3Int, long> pair in temp.GetDataPointsAsDictionary())
            {
                if (keyValuePairs.ContainsKey(pair.Key))
                {
                    keyValuePairs[pair.Key] += (long)(pair.Value * weights[weightIndex]);
                }
                else
                {
                    keyValuePairs.Add(pair.Key, (long)(pair.Value * weights[weightIndex]));
                }

                if (keyValuePairs[pair.Key] > _saveFile.maxValue)
                {
                    _saveFile.maxValue = keyValuePairs[pair.Key];
                }
            }

            weightIndex++;
        }

        Debug.Log("Loading loaded file into object.");
        
        foreach (KeyValuePair<Vector3Int, long> pair in keyValuePairs)
        {
            _saveFile.AddPoint(new DataPoint(pair.Key, pair.Value));

        }
    }
}
