using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultScene : MonoBehaviour
{

    [SerializeField] private GameObject save_button;
    [SerializeField] private GameObject retry_button;
    [SerializeField] private Image result_image;
    private Sprite screenshot;

    // Start is called before the first frame update
    void Start()
    {
        DisplayImage(Application.persistentDataPath + "/UnalteredScene.png");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveImage()
    {

    }

    private void DisplayImage(string path)
    {
        if (System.IO.File.Exists(path))
        {
            Debug.Log("3");
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
            result_image.sprite = sprite;
            screenshot = sprite;
            Debug.Log("4");
        }
    }
}
