using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
    public int heightCycles;
    public float minHeight;
    public float cameraSteps;

    [Header("Valid Directions")]
    public Transform[] directions;
    //public bool front = true;
    //public bool right = true;
    //public bool back = true;
    //public bool left = true;

    [HideInInspector]
    public bool processing = false;

    System.Diagnostics.Stopwatch watch;
    private float maxFrameLength = 0.01666f;
    private long tickBudget;

    public bool displayEachFrame = true;


    public IEnumerator CycleThroughCameras(Camera camera)
    {
        yield return StartCoroutine(CycleThroughCameras(camera, false, this.transform));
    }

    public IEnumerator CycleThroughCameras(Camera camera, bool cullWhenTargetNotInView, Transform target)
    {
        Quaternion prevRotation = camera.transform.rotation;
        Vector3 prevPosition = camera.transform.position;


        watch = new System.Diagnostics.Stopwatch();

        tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));

        for (int i = 0; i < heightCycles; i++)
        {
            Vector3 newPos = this.transform.position;

            newPos.y = transform.position.y + minHeight + cameraSteps * i;

            camera.transform.position = newPos;

            foreach (var dir in directions)
            {
                camera.transform.rotation = dir.rotation;

                if (cullWhenTargetNotInView)
                { 
                    if (Vector3.Dot(camera.transform.forward, (target.position - camera.transform.position).normalized) < 0)
                    {
                        continue;
                    }
                }

                SimulationManager.Instance.Execute();

                if (displayEachFrame)
                    yield return null;

                //camera.transform.rotation = Quaternion.LookRotation(this.transform.InverseTransformVector(dir), this.transform.up);
            }


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
