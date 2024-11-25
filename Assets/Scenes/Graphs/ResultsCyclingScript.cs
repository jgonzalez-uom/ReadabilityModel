using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultsCyclingScript : MonoBehaviour
{
    public QuickScreenshot screenshooter;
    public List<Image> images;
    public List<Sprites> sprites;
    public KeyCode startKey = KeyCode.Return;

    public TextMeshProUGUI graphTitleDisplay;
    public TextMeshProUGUI minValueDisplay;
    public TextMeshProUGUI maxValueDisplay;

    [System.Serializable]
    public class Sprites
    {
        public string name;
        public string displayName;
        public List<Sprite> sprites;
        public Vector2 dimensions = new Vector2(550, 550);
        public string minValue = "0";
        public string maxValue = "100000+";
    }

    private void Update()
    {
        if (!Input.GetKeyDown(startKey))
            return;

        StartCoroutine(CycleCoroutine());
    }

    IEnumerator CycleCoroutine()
    {
        foreach (var s in sprites)
        {
            for (int i = 0; i < Mathf.Min(s.sprites.Count, images.Count); i++)
            {
                images[i].sprite = s.sprites[i];
                images[i].rectTransform.sizeDelta = s.dimensions;
            }

            graphTitleDisplay.text = s.displayName;
            minValueDisplay.text = s.minValue;
            maxValueDisplay.text = s.maxValue;

            screenshooter.TakeScreenshot(s.name);
            yield return null;
        }
    }
}
