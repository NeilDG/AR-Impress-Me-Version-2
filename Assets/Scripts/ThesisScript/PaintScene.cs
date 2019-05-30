using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PaintScene : MonoBehaviour {
    [SerializeField] private GameObject arcamera;
    [SerializeField] private Camera camera;
    [SerializeField] private ObjectManipulation objectMan;
    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private Image image;
    [SerializeField] private Canvas myCanvas;

    public GameObject loadingScreen;
    private Sprite screenshot;
    private string path = "";

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
            
        
    }

    public void runPaint() {
        StartCoroutine(Paint());
    }

    private IEnumerator Paint() {
        objectMan.reset();
        myCanvas.enabled = false;
        screenShot();
        yield return new WaitUntil(() => File.Exists(path) == true);
        showScreenShot();
    }

    private void LoadingScreen()
    {
        // Set it to the inverse of what the current active state is
        loadingScreen.SetActive(!loadingScreen.activeSelf);
    }

    private void screenShot() {
        Debug.Log(Application.persistentDataPath);
        path = Application.persistentDataPath + "/UnalteredScene.png";
        //ScreenCapture.CaptureScreenshot("UnalteredScene.png");
        //ScreenCapture.CaptureScreenshot(path);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        byte[] shot = texture.EncodeToPNG();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        File.WriteAllBytes(path, shot);
        Debug.Log(path);
    }

    private void showScreenShot() {
        DisplayImage(path);
        image.enabled = true;
        paintButton.SetActive(false);
        backButton.SetActive(true);
        myCanvas.enabled = true;
    }

    private void DisplayImage(string path) {
        if (System.IO.File.Exists(path)) {
            //path = Application.persistentDataPath + "/abcd6.jpg"; 
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            int height = texture.height / (texture.width / 480);
            texture = ScaleTexture(texture, 480, height);
            Debug.Log(height);
            //texture = ScaleTexture(texture, texture.width/2, texture.height/2);
            texture = changeColor(texture);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
            image.sprite = sprite;
            screenshot = sprite;
            arcamera.SetActive(false);
        }
    }

    public void back() {
        if(!path.Equals(""))
            File.Delete(path);
        path = "";
        image.enabled = false;
        backButton.SetActive(false);
        paintButton.SetActive(true);
        objectMan.reset();
        arcamera.SetActive(true);
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight) {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
        float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
        for (int px = 0; px < rpixels.Length; px++) {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
                              incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    Color LeadWhite = new Color(0.8627f, 0.8588f, 0.8392f),
          CadiumYellow = new Color(1.0f, 0.965f, 0.0f),
          ViridianGreen = new Color(0.251f, 0.51f, 0.427f),
          EmeraldGreen = new Color(0.314f, 0.784f, 0.471f),
          FrenchUltramarine = new Color(0.071f, 0.039f, 0.561f),
          CobaltBlue = new Color(0.0f, 0.278f, 0.671f),
          AlizarinCrimson = new Color(0.89f, 0.149f, 0.212f),
          VermilionRed = new Color(0.89f, 0.259f, 0.204f),
          IvoryBlack = new Color(0.16f, 0.14f, 0.13f);

    private Texture2D changeColor(Texture2D source) {
        Color[] palette = new Color[9];
        palette[0] = LeadWhite;
        palette[1] = CadiumYellow;
        palette[2] = ViridianGreen;
        palette[3] = EmeraldGreen;
        palette[4] = FrenchUltramarine;
        palette[5] = CobaltBlue;
        palette[6] = AlizarinCrimson;
        palette[7] = VermilionRed;
        palette[8] = IvoryBlack;

        Color[] rpixels = source.GetPixels(0);
        float width = 0f/* source.width*/,
             height = 0f/*source.height*/,
             gradientx = 4f,
             gradienty = 2.5f;
            
        Color mixed = new Color();

        for (int px = 0; px < rpixels.Length; px++) {
            double lowestValue = 0;
            int colorIndex = -1, bwIndex = -1;
            Boolean bdeeper = false, wdeeper = false, mix = false;

            //rpixels[px] *= 1.1f;

            //First Color Detection
            for (int x = 0; x < 9; x++) {
                if(lowestValue > ColourDistance(palette[x], rpixels[px]) || colorIndex == -1) {
                    lowestValue = ColourDistance(palette[x], rpixels[px]);
                    colorIndex = x;
                }
            }
            
            float H, S, V;
            Color.RGBToHSV(rpixels[px], out H, out S, out V);
            double B = (Math.Pow(rpixels[px].r * 0.299f, 2) + Math.Pow(rpixels[px].g * 0.587f,2) + Math.Pow(rpixels[px].b * 0.114f, 2)) / 255;
           if (colorIndex == 0 || colorIndex == 8) {
                mixed = palette[colorIndex];
                mix = true;
                //GreyScale Balck/White detection
                double bwlowestValue = 0;
                for (int x = 0; x < 9; x++) {
                    if (bwlowestValue > ColourDistanceGreyScale(palette[x], rpixels[px]) || bwIndex == -1) {
                        bwlowestValue = ColourDistanceGreyScale(palette[x], rpixels[px]);
                        bwIndex = x;
                    }
                }
                if (bwIndex == 0)
                    wdeeper = true;
                else if(bwIndex == 8)
                    bdeeper = true;
                /*
                //Color Mixing
                int prev = colorIndex;
                for (int x = 0; x < 9; x++) {
                    if (colorIndex != x) {
                        Color a = palette[colorIndex],
                              b = palette[x];
                        if (lowestValue > ColourDistance((a + b) / 2, rpixels[px])) {
                            lowestValue = ColourDistance((a + b) / 2, rpixels[px]);
                            mixed = (a + b) / 2;
                            prev = 9;
                        }
                    }
                }
                colorIndex = prev;*/
            }
            //else {
                //First Color Detection
                float deg = H * 360;
                double c1, c2;
                if ((deg >= 0 && deg < 30) || (deg >= 270 && deg < 330)) {
                    c1 = ColourDistance(AlizarinCrimson, rpixels[px]);
                    c2 = ColourDistance(VermilionRed, rpixels[px]);
                    if(c1 < c2) {
                        lowestValue = ColourDistance(AlizarinCrimson, rpixels[px]);
                        colorIndex = 6;
                    } else {
                        lowestValue = ColourDistance(VermilionRed, rpixels[px]);
                        colorIndex = 7;
                    }
                }
                else if (deg >= 30 && deg < 90) {
                    lowestValue = ColourDistance(CadiumYellow, rpixels[px]);
                    colorIndex = 1;
                }
                else if (deg >= 90 && deg < 150) {
                    c1 = ColourDistance(EmeraldGreen, rpixels[px]);
                    c2 = ColourDistance(ViridianGreen, rpixels[px]);
                    if (c1 < c2) {
                        lowestValue = ColourDistance(EmeraldGreen, rpixels[px]);
                        colorIndex = 3;
                    }
                    else {
                        lowestValue = ColourDistance(ViridianGreen, rpixels[px]);
                        colorIndex = 2;
                    }
                }
                else if (deg >= 150 && deg < 270) {
                    c1 = ColourDistance(CobaltBlue, rpixels[px]);
                    c2 = ColourDistance(FrenchUltramarine, rpixels[px]);
                    if (c1 < c2) {
                        lowestValue = ColourDistance(CobaltBlue, rpixels[px]);
                        colorIndex = 5;
                    }
                    else {
                        lowestValue = ColourDistance(FrenchUltramarine, rpixels[px]);
                        colorIndex = 4;
                    }
                }
                else {
                    c1 = ColourDistance(AlizarinCrimson, rpixels[px]);
                    c2 = ColourDistance(VermilionRed, rpixels[px]);
                    if (c1 < c2) {
                        lowestValue = ColourDistance(AlizarinCrimson, rpixels[px]);
                        colorIndex = 6;
                    }
                    else {
                        lowestValue = ColourDistance(VermilionRed, rpixels[px]);
                        colorIndex = 7;
                    }
                }
            //}
            
            
            if(colorIndex != 9) {
                //if (mix) {
                    //mixed = (mixed + palette[colorIndex]) / 2;
                //}
                //else 
                    mixed = palette[colorIndex];

                //Color Mixing
                for (int x = 0; x < 9; x++) {
                    if (colorIndex != x) {
                        Color a = mixed,
                              b = palette[x];
                        if (lowestValue > ColourDistance((a + b) / 2, rpixels[px])) {
                            lowestValue = ColourDistance((a + b) / 2, rpixels[px]);
                            colorIndex = x;
                        }
                    }
                }
                mixed = (mixed + palette[colorIndex]) / 2;
            }
                
            //GrayScale Deepener
            if (bdeeper) {
                mixed = (mixed + mixed + IvoryBlack) / 3;
            }
            else if (wdeeper) {
                mixed = (mixed + mixed + LeadWhite) / 3;
            }



            //Black and White Darkener

            if (S < 0.1 && B > 0.9) {
                /*while (lowestValue > ColourDistance((mixed + LeadWhite)/2, rpixels[px])) {
                    mixed = (mixed + LeadWhite)/2;
                    lowestValue = ColourDistance(mixed, rpixels[px]);
                }*/
                
            }
            else if (B < 0.1) {
               /* while (lowestValue > ColourDistance((mixed + IvoryBlack)/2, rpixels[px])) {
                    mixed = (mixed + IvoryBlack)/2;
                    lowestValue = ColourDistance(mixed, rpixels[px]);
                }*/
            }

            
            rpixels[px] = mixed;

            //Perlin Noise Filter
            float seed = (mixed.r + mixed.g + mixed.b)/3, 
                  noisex, noisey, nx = 0f, ny = 0f;
            //nx = 0.01f;
            //if(seed > 0.5)
                //ny = 0.01f; 
            //else ny = 0.1f;

            nx = 0.1f;
            ny = 0.5f;
            noisex = width * nx; 
            noisey = height * ny;

            if (Mathf.PerlinNoise(noisex, noisey) < 0.3f)
                //rpixels[px] = (rpixels[px] * 15 + LeadWhite) / 16;
            
            /*if (seed > 0.5) {
                nx = 0.01f;
                ny = 0.01f;
                noisex = width * nx + (seed * 10);
                noisey = height * ny + (seed * 10);
                //noisey /= (height * ny); // gets shorter as it goes down
                //noisey -= (height * ny); //straight lines
                
                noisex += height; //straight down
                noisex += height * (width/source.width);

                if (Mathf.PerlinNoise(noisex, noisey) < 0.4f)
                    rpixels[px] = (rpixels[px] + rpixels[px] + IvoryBlack)/3;

            }
            else {*/
                nx = 0.05f;
                ny = 0.1f;
                noisex = width * nx / gradientx;
                noisey = height * ny * gradienty;
                //noisex = width * nx / 4f;
                //noisey = height * ny * 2.5f;
                gradientx -= 0.00000005f;
                gradienty += 0.000005f;
                
                
                //noisex += (height * 2); //sideways
                //noisex += height; //straight down
                //noisex += width * (height / source.height);

                noisex += (height * nx);
                noisey += (width * ny /  8);
                

                if (Mathf.PerlinNoise(noisex, noisey) < 0.5f)
                    //rpixels[px] = (rpixels[px] * 15 + IvoryBlack) / 16;
            //}

            rpixels[px] *= 1.5f;
            /*
            height -= 1f;
            if(height == (source.height - source.width)) {
                width -= 1;
                height = source.height;
            }*/


            width += 1f;
            if (width % source.width == 0) {
                width = 0;
                height += 1f;
            } 
            
 
        }
        Debug.Log(gradientx + " " + gradienty);
        source.SetPixels(rpixels, 0);
        source.Apply();
        return source;
    }

    public static double ColourDistanceGreyScale(Color e1, Color e2) {
        return Math.Sqrt(Math.Pow(e2.r * 0.299f - e1.r * .3, 2) + Math.Pow(e2.g * 0.587f - e1.g * 0.587f, 2) + Math.Pow(e2.b * 0.114f - e1.b * 0.114f, 2));
    }

    public static double ColourDistance(Color e1, Color e2) {
        //return Math.Sqrt( Math.Pow(e2.r * 0.299f - e1.r *.3, 2) + Math.Pow(e2.g * 0.587f - e1.g * 0.587f, 2)+ Math.Pow(e2.b * 0.114f - e1.b * 0.114f, 2));
        return Math.Sqrt(Math.Pow(e2.r - e1.r, 2) + Math.Pow(e2.g - e1.g, 2) + Math.Pow(e2.b - e1.b, 2));
        //return (Math.Abs(e2.r - e1.r) / 256 + Math.Abs(e2.g - e1.g) / 256 + Math.Abs(e2.b - e1.b) /256) / 3 * 100;
        /* 
        float H1, S1, V1, H2, S2, V2;
        Color.RGBToHSV(e1, out H1, out S1, out V1);
        Color.RGBToHSV(e2, out H2, out S2, out V2);

        float d = Math.Abs(H1 - H2);

        if(d > 180)
            d = 360 - d;

        float B1 = (e1.r * 0.299f + e1.g * 0.587f + e1.b * 0.114f) / 255,
              B2 = (e2.r * 0.299f + e2.g * 0.587f + e2.b * 0.114f) / 255;

        //return d;

        //return Math.Abs((S1 + B1) - (S2 + B2));
        
        return Math.Abs((S1 + B1) - (S2 + B2)) + d;
        
        double cR = e1.r - e2.r,
               cG = e1.g - e2.g,
               cB = e1.b - e2.b,
               uR = e1.r + e2.r;
        return cR * cR * (2 + uR / 256) + cG * cG * 4 + cB * cB * (2 + (255 - uR) / 256);
        
        Vector3 x = new Vector3(e1.r, e1.g, e1.b);
        Vector3 y = new Vector3(e2.r, e2.g, e2.b);

        return Vector3.Distance(x, y);
        */
        }
    }
