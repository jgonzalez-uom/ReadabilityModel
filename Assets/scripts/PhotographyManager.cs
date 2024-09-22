using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

public class PhotographyManager : MonoBehaviour
{
    public Camera[] cameras;
    [SerializeField]
    public RenderTexture renderTextureBuffer;
    MeshRenderer[] renderers;

    public void HideMeshes(Transform target)
    {
        renderers = target.GetComponentsInChildren<MeshRenderer>();
        foreach (var c in renderers)
        {
            c.enabled = false;
        }
    }

    public void ShowMeshes(Transform target)
    {
        foreach (var c in renderers)
        {
            c.enabled = true;
        }
    }
    //public void TakePhotos(string path, string fileName, string extension)
    //{
    //    StartCoroutine(TakePhotosCoroutine(path, fileName, extension));
    //}

    public IEnumerator TakePhotosCoroutine(string path, string fileName, string extension)
    {
        int cameraInd = 0;
        foreach (var c in cameras)
        {
            yield return new WaitForEndOfFrame();

            string fullPath = string.Format("{0}{1}{2}{3}", path, fileName, cameraInd, extension);

            SavePicture(fullPath, RTImage(c, renderTextureBuffer));

            cameraInd++;

            yield return null;
        }
    }

    private Texture2D RTImage(Camera mCamera, RenderTexture renderTexture)
    {
        Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);

        mCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        mCamera.Render();
        
        Texture2D screenShot = new Texture2D(renderTexture.width, renderTexture.height);

        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        mCamera.targetTexture = null;
        RenderTexture.active = null;

        return screenShot;
    }

    public void SavePicture(string filePath, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }
}
