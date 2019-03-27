using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintScene : MonoBehaviour {
    [SerializeField] private Material paint;
    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private Image image;
    [SerializeField] private Canvas myCanvas;
    private Sprite screenshot;
    private int screenCount = 4;
    private string path;
    private byte[] sendBytes;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
            
        
    }

    public void Paint() {
        //myCanvas.enabled = false;
        //Invoke("screenShot", 1f);
        screenShot();
        //Invoke("showScreenShot", 2f);
    }

    private void screenShot() {
        Debug.Log(Application.persistentDataPath);
        //path = Application.persistentDataPath + "/UnalteredScene.png";
        path = "D:/School/GAME/AR-Impress-Me/UnalteredScene.png";
        ScreenCapture.CaptureScreenshot("UnalteredScene.png");

        Texture2D GG = ScreenCapture.CaptureScreenshotAsTexture();
    }

    private void showScreenShot() {
        DisplayImage(path);
        //DisplayImage(path);
        paint.SetTexture(1, screenshot.texture);
        image.material = paint;
        image.enabled = true;
        paintButton.SetActive(false);
        backButton.SetActive(true);
        myCanvas.enabled = true;
    }

    private void DisplayImage(string path) {
        if (System.IO.File.Exists(path)) {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            sendBytes = bytes;
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
            image.sprite = sprite;
            screenshot = sprite;
        }
    }

    public byte[] SendBytes()
    {
        return sendBytes;
    }

    public void back() {
        image.enabled = false;
        backButton.SetActive(false);
        paintButton.SetActive(true);
    }
}
