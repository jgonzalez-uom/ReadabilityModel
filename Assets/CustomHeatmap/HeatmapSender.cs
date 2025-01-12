using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSender : MonoBehaviour
{
    public enum heatmapProjectionType { Radial, Rectangular, Point};


    public heatmapProjectionType projectionType;
    public bool DebugHitRays = true;
    public bool DebugMissedRays = true;
    public float rayDuration = 10.0f;
    private float maxFrameLength = 0.016f;

    [Header("Rectangular Projection Settings")]
    [Range(1, 320)]
    public int resolutionX;
    [Range(1, 320)]
    public int resolutionY;
    public Camera camera;
    
    [Header("Radial Projection Settings")]
    public int angleCount = 10;
    [Range(1, 20)]
    public int rowCount = 2;

    [Header("Additional Data Settings")]
    public float minDistance = 0f;
    [Tooltip("-1 for infinity")]
    public float maxDistance = -1;

    public LayerMask layersHit;

    public delegate void MyDelegate();
    public static MyDelegate OnCapture;

    private void Start()
    {
        if (camera == null) 
            camera = GetComponent<Camera>();
    }

    public void CameraViewHeat()
    {
        StartCoroutine(CameraViewHeatCoroutine());
        OnCapture.Invoke();
    }

    IEnumerator CameraViewHeatCoroutine()
    {
        var watch = new System.Diagnostics.Stopwatch();

        long tickBudget = (long)(System.Diagnostics.Stopwatch.Frequency
                                 * ((maxFrameLength)));
        watch.Restart();

        if (projectionType == heatmapProjectionType.Radial)
        {
            float radius = Mathf.Min(Screen.width - 1, Screen.height - 1);

            for (int r = 0; r < rowCount; r++)
            {
                float polarR = (radius / rowCount) * (r + 1);
                float polarA = 0;

                for (int i = 0; i < angleCount; i++)
                {
                    polarA = (360 / angleCount) * i;

                    Vector2 rayDir = new Vector2(
                        Mathf.Cos(polarA) * polarR + radius,
                        Mathf.Sin(polarA) * polarR + radius);

                    CastRay(rayDir);

                    if (watch.ElapsedTicks > tickBudget)
                    {
                        yield return null;
                        watch.Restart();
                    }
                }
            }

            CastRay(new Vector2(radius, radius));
        }
        else if (projectionType == heatmapProjectionType.Rectangular)
        {
            Vector2Int steps = new Vector2Int(camera.pixelWidth / resolutionX, camera.pixelHeight / resolutionY);

            for (int x = 0; x < camera.pixelWidth; x += steps.x)
            {
                for (int y = 0; y < camera.pixelHeight; y += steps.y)
                {
                    CastRay(new Vector2(x, y));

                    if (watch.ElapsedTicks > tickBudget)
                    {
                        yield return null;
                        watch.Restart();
                    }
                }
            }
        }
        else
        {
            CastRay(new Vector2((Screen.width - 1) / 2, (Screen.height - 1) / 2));
        }
    }

    void CastRay(Vector2 pos)
    {
        RaycastHit hit;

        Ray ray = camera.ScreenPointToRay(pos);

        Vector3 origin = camera.transform.position + (ray.direction * minDistance);

        if (Physics.Raycast(origin, ray.direction, out hit, ((maxDistance < 0) ? Mathf.Infinity : (maxDistance - minDistance)), layersHit))
        {
            if (hit.transform.TryGetComponent<HeatmapReceiver>(out HeatmapReceiver heatmapReceiver))
            {
                heatmapReceiver.AddPoint(hit.point);

                if (DebugHitRays)
                    Debug.DrawRay(origin, ray.direction * (hit.point - ray.origin).magnitude, Color.red, rayDuration);

                return;
            }


            if (DebugMissedRays)
                Debug.DrawRay(origin, ray.direction * (maxDistance - minDistance), Color.blue, rayDuration);

            return;
        }

        if (DebugMissedRays)
            Debug.DrawRay(origin, ray.direction * (maxDistance - minDistance), Color.blue, rayDuration);
    }
}
