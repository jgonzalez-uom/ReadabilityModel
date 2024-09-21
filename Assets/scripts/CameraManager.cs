using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public List<Camera> cameras = new List<Camera>();
    private Camera selectedCamera;

    public void ActivateOnlyOne(int index)
    {
        foreach (Camera c in cameras)
        {
            c.enabled = false;
        }

        cameras[index].enabled = true;
        selectedCamera = cameras[index];
    }

    public void ActivateOnlyOne(Camera cam)
    {
        foreach (Camera c in cameras)
        {
            c.enabled = false;
        }

        selectedCamera = cameras.Find(x => x == cam);
        selectedCamera.enabled = true;
    }

    public void ActivateOnlyOne()
    {
        foreach (Camera c in cameras)
        {
            c.enabled = false;
        }

        selectedCamera.enabled = true;
    }

    public void ActivateAll()
    {
        foreach (Camera c in cameras)
        {
            c.enabled = true;
        }
    }

}
