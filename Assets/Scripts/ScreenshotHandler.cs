using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{

    [SerializeField] private GameObject SwordButton;
    [SerializeField] private GameObject AK47Button;
    [SerializeField] private GameObject DragonButton;

    [SerializeField] private GameObject ScreenShotButton;
    
    AWSFileHandler fileHandler;
    public void Start()
    {
        fileHandler = new AWSFileHandler();
    }
    public void Update()
    {
        
    }

    public void TakeShot()
    {
        Debug.Log("Crush");
        TakeScreenShot();
    }

    public void TakeScreenShot()
    {

        Texture2D tex = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);

        removeButtons();
    

        byte[] image_bytes = null;
        Texture2D print_screen = ScreenCapture.CaptureScreenshotAsTexture();

        image_bytes = print_screen.EncodeToPNG();

        if (image_bytes == null)
        {
            Debug.Log("FUCK");
        } else
        {
            Debug.Log("SHOT TAKEN");
        }

        tex.LoadRawTextureData(image_bytes);
        tex.Apply();

        SaveTextureAsPNG(tex);

        returnButtons();

    }

    public void SaveTextureAsPNG(Texture2D _texture)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + Application.persistentDataPath);
    }

    private bool m_screenShotLock = false;

    public void screenShot()
    {
        
         m_screenShotLock = true;
        //StartCoroutine(TakeScreenShotCo());
        removeButtons();

        var directory = new DirectoryInfo(Application.dataPath);
        Debug.Log("Taking screenshot to " + "dahyun.png");
        Debug.Log(Application.dataPath);
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + "dahyun.png");
        m_screenShotLock = false;

        //fileHandler.PostObject("dahyun.png");
        
        returnButtons();
    }

    private IEnumerator TakeScreenShotCo()
    {
        removeButtons();
        yield return new WaitForEndOfFrame();

        var directory = new DirectoryInfo(Application.dataPath);
        Debug.Log("Taking screenshot to " + "dahyun.png");
        Debug.Log(Application.dataPath);
        ScreenCapture.CaptureScreenshot(Application.dataPath + "/" + "dahyun.png");
        m_screenShotLock = false;

        //fileHandler.PostObject2("dahyun.png");

        yield return new WaitForEndOfFrame();
        returnButtons();
        returnButtons();
    }

    public void SendBucket()
    {
        fileHandler.PostObject2("dahyun.png");
    }

    private void removeButtons()
    {
        SwordButton.SetActive(false);
        AK47Button.SetActive(false);
        DragonButton.SetActive(false);
        ScreenShotButton.SetActive(false);
    }

    private void returnButtons()
    {
        SwordButton.SetActive(true);
        AK47Button.SetActive(true);
        DragonButton.SetActive(true);
        ScreenShotButton.SetActive(true);
    }



}
