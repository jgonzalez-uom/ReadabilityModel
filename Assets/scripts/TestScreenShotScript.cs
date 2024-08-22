using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestScreenShotScript : MonoBehaviour
{
    [SerializeField]
    public RenderTexture renderTextureBuffer;
    public Camera cam;

    private void Start()
    {
        SavePicture(Application.persistentDataPath + "/Test.png", RTImage(cam, renderTextureBuffer));
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
