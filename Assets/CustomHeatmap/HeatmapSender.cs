using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapSender : MonoBehaviour
{
    //public bool inputBased;

    public int testIterations = 1;

    public int angleCount = 10;
    [Range(1, 20)]
    public int rowCount = 2;

    public void CameraViewHeat()
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
            }
        }

        CastRay(new Vector2(radius, radius));
    }

    void InputHeat()
    {    
        if (Input.GetButton("Fire1"))
        {
            CastRay(Input.mousePosition);
        }
    }

    void CastRay(Vector2 pos)
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(pos);


        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.TryGetComponent<HeatmapReceiver>(out HeatmapReceiver heatmapReceiver))
            {
                heatmapReceiver.AddPoint(hit.point);

                Debug.DrawRay(ray.origin, ray.direction * (hit.point - ray.origin).magnitude, Color.red, 5000);
                //Debug.Log(hit.point);

                return;
            }


            Debug.DrawRay(ray.origin, ray.direction * (hit.point - ray.origin).magnitude, Color.blue, 5000);

            return;
        }

        Debug.DrawRay(ray.origin, ray.direction * 10, Color.blue, 5000);
    }
}
