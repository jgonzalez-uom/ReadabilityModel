using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(Camera))]
public class QuickScreenshot : MonoBehaviour
{
    public KeyCode key;
    [SerializeField]
    public RenderTexture renderTextureBuffer;
    private Camera m_Camera;
    public string folderName;
    public enum method { ScreenshotFunction, TargetTexture};
    public method ScreenshotMethod;

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            string fileName = System.DateTime.UtcNow.ToString("HHmmss_ddMMyyyy") + transform.name;
            TakeScreenshot(fileName);
        }
    }

    public void TakeScreenshot(string fileName)
    {
        

        if (ScreenshotMethod == method.TargetTexture)
            StartCoroutine(TakePhotosCoroutine(Application.persistentDataPath + "/", fileName, ".png"));
        else
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/" + fileName + ".png");

        Debug.Log("Screenshot taken: " + fileName);
    }

    public IEnumerator TakePhotosCoroutine(string path, string fileName, string extension)
    {
        int cameraInd = 0;

        yield return new WaitForEndOfFrame();

        string fullPath = string.Format("{0}{1}{2}", path, fileName, extension);

        SavePicture(fullPath, RTImage(m_Camera, renderTextureBuffer));

        cameraInd++;

        yield return null;
    }


    public void SavePicture(string filePath, Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
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
}
