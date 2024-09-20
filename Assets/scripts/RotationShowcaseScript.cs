using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationShowcaseScript : MonoBehaviour
{
    public float degreesPerSecond;
    public bool paused = false;
    Quaternion originalRotation;

    private void Start()
    {
        originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            transform.Rotate(transform.up, degreesPerSecond * Time.deltaTime);
        }
    }

    public void SetPause(bool to)
    {
        paused = to;
    }

    public void Reset()
    {
        transform.rotation = originalRotation;
    }
}
