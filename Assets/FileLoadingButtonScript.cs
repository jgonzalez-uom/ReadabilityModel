using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FileLoadingButtonScript : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI text;
    public TMP_InputField weightInput;
    [HideInInspector]
    public string fileName;
    public string fullName;

    public void AddEvent(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void ClearEvent()
    {
        button.onClick.RemoveAllListeners();
    }
}
