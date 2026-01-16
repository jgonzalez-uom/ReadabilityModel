using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public void OpenLocalURL(string url)
    {
        Application.OpenURL(System.Environment.CurrentDirectory + "/" + url);
    }

    public void OpenNetworkURL(string url)
    {
        Application.OpenURL(url);
    }
}
