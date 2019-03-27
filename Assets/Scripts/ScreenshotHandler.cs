using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{

    [SerializeField] private GameObject SwordButton;
    [SerializeField] private GameObject AK47Button;
    [SerializeField] private GameObject DragonButton;

    [SerializeField] private GameObject ScreenShotButton;

    public void Start()
    {

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

        returnButtons();
    

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



        returnButtons();

    }

    private void removeButtons()
    {
        SwordButton.SetActive(false);
        AK47Button.SetActive(false);
        DragonButton.SetActive(false);
    }

    private void returnButtons()
    {
        SwordButton.SetActive(true);
        AK47Button.SetActive(true);
        DragonButton.SetActive(true);
    }



}
