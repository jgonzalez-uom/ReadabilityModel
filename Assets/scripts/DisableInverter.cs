using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInverter : MonoBehaviour
{
    public void SetNOTActive(bool to)
    {
        gameObject.SetActive(!to);
    }
}
