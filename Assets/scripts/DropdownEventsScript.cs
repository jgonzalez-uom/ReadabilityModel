using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DropdownEventsScript : MonoBehaviour
{
    [System.Serializable]
    public class OnSelectedElement
    {
        public string name;
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;
    }

    public OnSelectedElement[] items;
    private int currentlySelectedElement = -1;

    public void SelectItem(int index)
    {
        if (currentlySelectedElement == index || index >= items.Length || index < 0)
        {
            return;
        }

        items[index].OnSelected.Invoke();

        if (currentlySelectedElement < 0)
        {
            items[index].OnDeselected.Invoke();
        }

        currentlySelectedElement = index;
    }

    public void ActivateSelectedElement()
    {
        if (currentlySelectedElement < 0 || currentlySelectedElement >= items.Length)
        {
            return;
        }

        items[currentlySelectedElement].OnSelected.Invoke();
    }

    public void Reset()
    {
        currentlySelectedElement = -1;
        
    }
}
