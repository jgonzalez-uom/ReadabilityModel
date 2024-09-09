using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GradientScript : MonoBehaviour
{
    public TextMeshProUGUI minText;
    public TextMeshProUGUI maxText;

    public void SetGradient(HeatmapManager heatmapManager)
    {
        minText.text = heatmapManager.HeatmapDisplay.minVisibleHeat.ToString();
        maxText.text = heatmapManager.HeatmapDisplay.maxVisibleHeat.ToString();
    }
}
