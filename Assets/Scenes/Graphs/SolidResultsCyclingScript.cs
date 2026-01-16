using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SolidResultsCyclingScript : MonoBehaviour
{
    public QuickScreenshot screenshooter;
    public List<Image> images;
    public List<SolidSprites> sprites;
    public KeyCode startKey = KeyCode.Return;

    public TextMeshProUGUI graphTitleDisplay;
    public TextMeshProUGUI[] valueDisplays;

    [System.Serializable]
    public class SolidSprites
    {
        public string name;
        public string displayName;
        public List<Sprite> sprites;
        public Vector2 dimensions = new Vector2(550, 550);
        public int minValue = 0;
        public int maxValue = 10000;
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

            int steps = Mathf.FloorToInt((float)s.maxValue / valueDisplays.Length);

            for (int n = 0; n < valueDisplays.Length; n++)
            {
                valueDisplays[n].text = string.Format("{0}-{1}", steps * n, Mathf.Clamp((steps * (n + 1)) - 1, steps * n, s.maxValue)); 
            }

            screenshooter.TakeScreenshot(s.name);
            yield return null;
        }
    }
}
