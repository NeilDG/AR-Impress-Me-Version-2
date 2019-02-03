using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintScene : MonoBehaviour {
    [SerializeField] private GameObject backButton;
    [SerializeField] private Image image;
    [SerializeField] private Canvas myCanvas;
    private Sprite screenshot;
    private int screenCount = 4;
    private string path;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
            
        
    }

    public void Paint() {
        myCanvas.enabled = false;
        Invoke("screenShot", 1f);
        Invoke("showScreenShot", 2f);
    }

    private void screenShot() {
        path = Application.persistentDataPath + "/UnalteredScene.png";
        ScreenCapture.CaptureScreenshot("UnalteredScene.png");
    }

    private void showScreenShot() {
        DisplayImage(path);
        DisplayImage(path);
        myCanvas.enabled = true;
    }

    private void DisplayImage(string path) {
        if (System.IO.File.Exists(path)) {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
            image.sprite = sprite;
            image.enabled = true;
            screenshot = sprite;
            backButton.SetActive(true);
        }
    }

    public void back() {
        image.enabled = false;
        backButton.SetActive(false);
    }
}
