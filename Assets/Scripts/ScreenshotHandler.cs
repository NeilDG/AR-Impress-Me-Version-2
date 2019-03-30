using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{
    private static ScreenshotHandler instance;

    private Camera myCamera;
    private bool takeScreenshotNextFrame;
    private static byte[] screen_bytes;

    private void Awake()
    {
        Debug.Log("AWAKE");
        instance = this;
        //myCamera = gameObject.GetComponent<Camera>();
        myCamera = Camera.main;
    }

    private void OnPostRender()
    {
        if (takeScreenshotNextFrame)
        {
            takeScreenshotNextFrame = false;
            RenderTexture renderTexture = myCamera.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/CameraScreenshot.png", byteArray);
            Debug.Log("Saved ScreenShot!!");

            screen_bytes = byteArray;   

            RenderTexture.ReleaseTemporary(renderTexture);
            myCamera.targetTexture = null;

        }
    }

    private void TakeScreenShot(int width, int height)
    {
        myCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeScreenshotNextFrame = true;
        OnPostRender();
    }

    public static byte[] TakeScreenshot_Static(int width, int height)
    {
        
        instance.TakeScreenShot(width, height);
        return screen_bytes;
    }

}
