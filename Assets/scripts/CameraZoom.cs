using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Transform farthestPoint;
    public Transform nearthestPoint;


    public void SetCameraPosition(float to)
    {
        transform.position = Vector3.Lerp(farthestPoint.position, nearthestPoint.position, to);
    }
}
