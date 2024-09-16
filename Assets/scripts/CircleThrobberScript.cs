using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleThrobberScript : MonoBehaviour
{
    public Image image;
    public float percentagePerSecond = 0.5f;
    private bool emptying = false;
    private float progress = 0;

    private void OnEnable()
    {
        progress = 0;
        emptying = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((progress >= 1 && !emptying))
        {
            progress = 1;
            emptying = true;
            image.fillClockwise = !image.fillClockwise;
        }
        else if ((emptying && progress <= 0))
        {
            progress = 0;
            emptying = false;
            image.fillClockwise = !image.fillClockwise;
        }
        else if (!emptying)
        {
            progress += percentagePerSecond * Time.deltaTime;
            image.fillAmount = progress;
        }
        else
        {
            progress -= percentagePerSecond * Time.deltaTime;
            image.fillAmount = progress;
        }
    }
}
