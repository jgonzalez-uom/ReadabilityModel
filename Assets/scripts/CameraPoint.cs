using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
    public int heightCycles;
    public float minHeight;
    public float cameraSteps;

    [Header("Valid Directions")]
    public bool front = true;
    public bool right = true;
    public bool back = true;
    public bool left = true;

    [HideInInspector]
    public bool processing = false;

    System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    public bool displayEachFrame = true;

    private void Start()
    {

        watch = new System.Diagnostics.Stopwatch();

        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
    }

    public IEnumerator CycleThroughCameras(Camera camera)
    {
        Quaternion prevRotation = camera.transform.rotation;
        Vector3 prevPosition = camera.transform.position;

        for (int i = 0; i < heightCycles; i++)
        {
            Vector3 newPos = this.transform.position;

            newPos.y = transform.position.y + minHeight + cameraSteps * i;

            camera.transform.position = newPos;

            if (front)
            {
                camera.transform.forward = this.transform.forward;
                SimulationManager.Instance.Execute();
            }

            if (displayEachFrame)
                yield return null; 

            if (right)
            {
                camera.transform.right = this.transform.right;
                SimulationManager.Instance.Execute();
            }

            if (displayEachFrame)
                yield return null;

            if (back)
            {
                camera.transform.forward = -this.transform.forward;
                SimulationManager.Instance.Execute();
            }

            if (displayEachFrame)
                yield return null;

            if (left)
            {
                camera.transform.right = -this.transform.right;
                SimulationManager.Instance.Execute();
            }

            if (displayEachFrame)
                yield return null;


            if (watch.ElapsedTicks > tickBudget)
            {
                yield return null;
                watch.Restart();
            }
        }

        camera.transform.position = prevPosition;
        camera.transform.rotation = prevRotation;
    }
}
