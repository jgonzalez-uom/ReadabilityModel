using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayObjectMultiplier : MonoBehaviour
{
    public GameObject[] objects;
    private GameObject[] spawnedObjects;

    [SerializeField]
    private Vector3 offset;
    //public Vector3 offset
    //{
    //    get
    //    {
    //        return _offset;
    //    }
    //    set
    //    {
    //        _offset = value;
    //        Recalculate();
    //    }
    //}

    public Transform firstPosition;


    // Start is called before the first frame update
    void Start()
    {
        spawnedObjects = new GameObject[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            Vector3 pos = firstPosition.TransformPoint(offset * i);
            spawnedObjects[i] = Instantiate(objects[i], pos, firstPosition.rotation, this.transform);
        }
    }

    public void Recalculate()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            Vector3 pos = firstPosition.TransformPoint(offset * i);
            spawnedObjects[i].transform.position = pos;
            spawnedObjects[i].transform.rotation = firstPosition.rotation;
        }
    }
}
