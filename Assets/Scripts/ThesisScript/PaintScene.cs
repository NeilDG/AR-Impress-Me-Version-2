using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.XphotoModule;

public class PaintScene : MonoBehaviour
{
    [SerializeField] private GameObject arcamera;
    [SerializeField] private Camera camera;
    [SerializeField] private ObjectManipulation objectMan;
    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Image image;
    [SerializeField] private Canvas myCanvas;

    [SerializeField] private int numColorsToMix = 2;
    [SerializeField] private int lever = 1;
    [SerializeField] private String imageName;

    public GameObject loadingScreen;
    private Sprite screenshot;
    private string path = "";
    private string path1 = "";
    List<Color32> color_palette = new List<Color32>();
    private int checkYear = 0;
    private int threshold = 100;
    private int maxBrushLength = 16;
    private int minBrushLength = 4;
    private float curveFilter = 1f;
    private int lengthS = 0;
    private int color_palette_index = 0;

    private ClientSocketScript clientSocket;


    //opencv
    private WebCamTextureToMatHelper webCamTextureToMatHelper;

    

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void runPaint()
    {
        StartCoroutine(Paint());
    }

    private IEnumerator Paint()
    {
        /*
        //OPENCV
        webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

        #if UNITY_ANDROID && !UNITY_EDITOR
        // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
        webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
        #endif
        webCamTextureToMatHelper.Initialize();
        //OPENCV
        */

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

    private void screenShot()
    {
        Debug.Log(Application.persistentDataPath);
        path = Application.persistentDataPath + "/UnalteredScene.jpg";
        //ScreenCapture.CaptureScreenshot("UnalteredScene.png");
        //ScreenCapture.CaptureScreenshot(path);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        camera.targetTexture = rt;
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        texture.ReadPixels(new UnityEngine.Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        byte[] shot = texture.EncodeToJPG();

        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        File.WriteAllBytes(path, shot);
        Debug.Log(path);

        string base64Image = Convert.ToBase64String(shot);
        clientSocket = new ClientSocketScript(base64Image, "GetColorPalette");
        clientSocket.Start();
        while (clientSocket.getResponse() == null)
        {
            clientSocket.Update();
        }

        color_palette_index = Int32.Parse(clientSocket.getResponse());
        clientSocket.Stop();
    }

    private void showScreenShot()
    {
        DisplayImage(path);
        image.enabled = true;
        paintButton.SetActive(false);
        mainMenu.SetActive(false);
        backButton.SetActive(true);
        myCanvas.enabled = true;
    }

    private void DisplayImage(string path)
    {
        if (System.IO.File.Exists(path))
        {
            path = Application.persistentDataPath + "/" + imageName + ".jpg";
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Texture2D orgTexture = new Texture2D(1, 1);
            orgTexture.LoadImage(bytes);

            if (Screen.orientation == ScreenOrientation.Portrait)
            {
                int height = texture.height / (texture.width / 480);
                texture = ScaleTexture(texture, 634, 480);
                orgTexture = ScaleTexture(orgTexture, 634, 480);
            }
            else
            {
                int width = texture.width / (texture.height / 480);
                texture = ScaleTexture(texture, 480, 634);
                orgTexture = ScaleTexture(orgTexture, 480, 634);
            }
            //OPENCV Color Picker
            Mat OrgTextureMat = new Mat(orgTexture.height, orgTexture.width, CvType.CV_8UC4);
            Utils.texture2DToMat(orgTexture, OrgTextureMat);


            Mat samples = OrgTextureMat.reshape(1, OrgTextureMat.cols() * OrgTextureMat.rows());
            Mat samples32f = new Mat();
            samples.convertTo(samples32f, CvType.CV_32F, 1.0 / 255.0);

            Mat labels = new Mat();
            TermCriteria criteria = new TermCriteria(TermCriteria.COUNT, 100, 1);
            Mat centers = new Mat();
            Core.kmeans(samples32f, 10, labels, criteria, 1, Core.KMEANS_PP_CENTERS, centers);

            //centers.convertTo(centers, CvType.CV_8UC1, 255.0);
            //centers.reshape(3);

            Debug.Log(centers);
            Debug.Log(centers.rows());
            Debug.Log(centers.cols());

            texture = changeColor(texture, centers);

            //OPENCV
            Mat TextureMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);
            Utils.texture2DToMat(texture, TextureMat);
            Mat refImg = new Mat(texture.height, texture.width, CvType.CV_8UC4);
            Mat Canvas = new Mat(texture.height, texture.width, CvType.CV_8UC4);
            int[] radius = new int[] { 8, 4, 2 };


            // ZMQ BRUSH STROKE ALGORITHM
            byte[] colorChangedPic = texture.EncodeToJPG();
            string image64 = Convert.ToBase64String(colorChangedPic);

            // Copy this structure to add in change color
            clientSocket = new ClientSocketScript(image64, "GetBrushStrokes");
            clientSocket.Start();
            while (clientSocket.getResponse() == null)
            {
                clientSocket.Update();
            }


            bytes = Convert.FromBase64String(clientSocket.getResponse());
            clientSocket.Stop();
            texture.LoadImage(bytes);

            /*Mat grayMat = new Mat(TextureMat.rows(), TextureMat.cols(), CvType.CV_8UC1);
            Mat gradientx = new Mat(TextureMat.rows(), TextureMat.cols(), CvType.CV_8UC1);
            Mat gradienty = new Mat(TextureMat.rows(), TextureMat.cols(), CvType.CV_8UC1);

            Mat rgbaMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);
            Utils.texture2DToMat(texture, rgbaMat);

            Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);
            Imgproc.Scharr(grayMat, gradientx, rgbaMat.depth(), 1, 0, 1 / 15.36);
            Imgproc.Scharr(grayMat, gradienty, rgbaMat.depth(), 0, 1, 1 / 15.36);

            //int gradient_smoothing_radius = Math.Round(Math.Max(rgbaMat.dims) / 50);
            //chosen stroke scale: 2
            //chosen gradient smoothing radius: 16
            Imgproc.GaussianBlur(gradientx, gradientx, new Size(2 * 16 + 1, 2 * 16 + 1), 0);
            Imgproc.GaussianBlur(gradienty, gradienty, new Size(2 * 16 + 1, 2 * 16 + 1), 0);

            Imgproc.medianBlur(rgbaMat, rgbaMat, 11);

            List<int> gridx = new List<int>();
            List<int> gridy = new List<int>();
            int index = 0;
            System.Random rnd = new System.Random();

            //new grid
            for (int i = 0; i < texture.width; i += 3)
            {
                for (int j = 0; j < texture.height; j += 3)
                {
                    int x = rnd.Next(-1, 2) + i;
                    int y = rnd.Next(-1, 2) + j;

                    gridy.Add(y % texture.height);
                    gridx.Add(x % texture.width);
                    index++;
                }
            }
            //shuffle grid
            int n = gridy.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                int temp = gridy[k];
                gridy[k] = gridy[n];
                gridy[n] = temp;

                temp = gridx[k];
                gridx[k] = gridx[n];
                gridx[n] = temp;
            }

            int batch_size = 10000;
            Debug.Log(gridx.Count + " " + gridy.Count);
            List<Color32> pixels = new List<Color32>();
            List<Color32> orgPixels = new List<Color32>();
            double shortest = 50, longest = 0, angleShort = 0;
            for (int h = 0; h < index - 1; h += batch_size)
            {
                pixels = new List<Color32>();
                orgPixels = new List<Color32>();
                int endpoint = h + batch_size;
                if (endpoint > index - 1)
                    endpoint = index - 1;
                //get the color from the texture
                for (int px = h; px < endpoint; px++)
                {
                    Color32 cpixel = texture.GetPixel(gridx[px], (gridy[px] - (texture.height - 1)) * (-1));
                    pixels.Add(cpixel);
                    cpixel = orgTexture.GetPixel(gridx[px], (gridy[px] - (orgTexture.height - 1)) * (-1));
                    orgPixels.Add(cpixel);
                }
                int cindex = 0;
                for (int px = h; px < endpoint; px++)
                {
                    int x = gridx[px],
                        y = gridy[px];

                    //get color
                    Color32 cpixel;
                    //use color of pixel
                    int cprob = rnd.Next(1, 11);

                    //if(cprob <= 10) {
                    Color a, b;
                    a = orgPixels[cindex];
                    b = pixels[cindex];
                    //here lol
                    cpixel = (a + b) / 2;
                    //cpixel = a;
                    //} else {
                    //if (cprob > 5) {
                    List<Color32> c_palette = color_palette;
                    c_palette.Remove(pixels[cindex]);
                    cprob = 0;
                    cprob = rnd.Next(0, c_palette.Count - 1);
                    a = c_palette[cprob];
                    b = cpixel;
                    //cpixel = (a  + b )/2;
                    //cpixel = b*1.1f;


                    //cpixel = a;

                    //}
                    cindex++;
                    //get angle
                    double length = Math.Round(2 + 2 * Math.Sqrt(Math.Sqrt(gradienty.get(y, x)[0] * gradienty.get(y, x)[0] + gradientx.get(y, x)[0] * gradientx.get(y, x)[0])));
                    double angle = (180 / Math.PI) * (Math.Atan2(gradienty.get(y, x)[0], gradientx.get(y, x)[0])) + 90;
                    double lengthb = 1;
                    if (length < shortest)
                    {
                        shortest = length;
                        angleShort = angle;
                    }
                    if (length > longest)
                        longest = length;
                    if (length > 2 && angle != 90)
                    {
                        length /= 3;
                    }
                    else
                    {
                        angle += 80;
                        length = 10;
                        lengthb = 2;
                        float H, S, V;
                        Color.RGBToHSV(b, out H, out S, out V);
                        float sat = cprob * 0.01f;
                        sat += 1;
                        S *= sat;
                        cpixel = Color.HSVToRGB(H, S, V);
                    }
                    Imgproc.ellipse(rgbaMat, new Point(x, y), new Size(length, lengthb), angle, 0, 360, new Scalar(cpixel.r, cpixel.g, cpixel.b), -1, Imgproc.LINE_AA);
                }
                //Imgcodecs.imwrite("D:\\Thesis\\Outputs\\PredecessorBrushStroke" + h + ".jpg", rgbaMat);
            }
            Debug.Log("Longest : " + longest);
            Debug.Log("Shortest : " + shortest);
            Debug.Log("Angle : " + angleShort);
            Utils.matToTexture2D(rgbaMat, texture);*/


            //Imgproc.cvtColor(Canvas, Canvas, Imgproc.COLOR_RGB2BGRA);
            //Utils.matToTexture2D(Canvas, texture);

            //texture = changeColor(texture, centers);

            Imgproc.cvtColor(Canvas, Canvas, Imgproc.COLOR_RGB2BGRA);
            Utils.texture2DToMat(texture, Canvas);
            Imgproc.cvtColor(Canvas, Canvas, Imgproc.COLOR_RGB2BGRA);

            Imgcodecs.imwrite("D:\\Thesis\\Outputs\\OldBrushStroke_" + imageName + "_Final.jpg", Canvas);
            //Imgcodecs.imwrite("D:\\Thesis\\Outputs\\OldBrushStroke_" + imageName +"_ColorChanged.jpg", Canvas);

            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
            image.sprite = sprite;
            screenshot = sprite;
            arcamera.SetActive(false);
        }
    }
    private float diff(double[] a, double[] b)
    {
        double d = Math.Sqrt(Math.Pow(a[0] - b[0], 2)
                        + Math.Pow(a[1] - b[1], 2)
                        + Math.Pow(a[2] - b[2], 2));

        return (float)d;
    }

    public void back()
    {
        if (!path.Equals(""))
            File.Delete(path);
        path = "";
        image.enabled = false;
        backButton.SetActive(false);
        paintButton.SetActive(true);
        mainMenu.SetActive(true);
        objectMan.reset();
        arcamera.SetActive(true);
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
        float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
                              incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

    private Texture2D changeColor(Texture2D source, Mat centers)
    {
        int n = 0;

        if (color_palette_index == 0)
            n = 10;
        else
            n = 9;

        Color[] palette = getPalette(centers);

        Color[] rpixels = source.GetPixels(0);
        Color mixed = new Color();


        for (int px = 0; px < rpixels.Length; px++)
        {
            double lowestValue = 0;
            int colorIndex = -1;

            //Numer of times to mix
            //int numColorsToMix = 3;

            //First Color Detection
            for (int x = 0; x < n; x++)
            {
                if (lowestValue > HSVDistanceCompare(palette[x], rpixels[px], lever) || colorIndex == -1)
                {
                    lowestValue = HSVDistanceCompare(palette[x], rpixels[px], lever);
                    colorIndex = x;
                }
            }

            mixed = palette[colorIndex];

            //Color Mixing

            for (int m = 0; m < numColorsToMix - 1; m++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (colorIndex != x)
                    {
                        Color a = mixed,
                              b = palette[x];
                        if (lowestValue > HSVDistanceCompare((a + b) / 2, rpixels[px], lever))
                        {
                            lowestValue = HSVDistanceCompare((a + b) / 2, rpixels[px], lever);
                            colorIndex = x;
                        }
                    }
                }
                mixed = (mixed + palette[colorIndex]) / 2;
            }

            Color32 cpixel;
            rpixels[px] = mixed;
            cpixel = mixed;

            if (!color_palette.Contains(cpixel))
            {
                color_palette.Add(cpixel);
            }
            //rpixels[px] *= 1.5f;
        }
        source.SetPixels(rpixels, 0);
        source.Apply();
        return source;
    }

    public Color[] getPalette(Mat centers)
    {
        Color[] orgpic = new Color[10];

        double r = 0, g = 0, b = 0;
        int[] arr = new int[2];// 1st is up to 9 then 2nd is up to 3
        for (int x = 0; x <= 9; x++)
        {
            for (int y = 0; y <= 3; y++)
            {
                arr[0] = x;
                arr[1] = y;
                if (y == 0)
                    r = centers.get(arr)[0];
                else if (y == 1)
                    g = centers.get(arr)[0];
                else if (y == 2)
                    b = centers.get(arr)[0];
            }
            orgpic[x] = new Color((float)r, (float)g, (float)b, 1);
            Debug.Log(orgpic[x]);
        }

        //Color[][] palettes = getAllPalettes();

        //return palettes[0];

        if (color_palette_index == 1)
        {
            Color[] palette = new Color[9];
            palette[0] = new Color(0.8627f, 0.8588f, 0.8392f); //LeadWhite;
            palette[1] = new Color(1.0f, 0.965f, 0.0f); //CadiumYellow;
            palette[2] = new Color(0.968f, 0.917f, 0.0f); //Chrome Yellow;
            palette[3] = new Color(0.251f, 0.51f, 0.427f); //ViridianGreen;
            palette[4] = new Color(0.314f, 0.784f, 0.471f); //EmeraldGreen;
            palette[5] = new Color(0.071f, 0.039f, 0.561f); //FrenchUltramarine;
            palette[6] = new Color(0.0f, 0.278f, 0.671f); //CobaltBlue;
            palette[7] = new Color(0.89f, 0.149f, 0.212f); //AlizarinCrimson;
            palette[8] = new Color(0.89f, 0.259f, 0.204f); //VermilionRed;
            return palette;
        }

        else if (color_palette_index == 2)
        {
            Color[] palette = new Color[9];
            palette[0] = new Color(0.75f, 0.75f, 0.75f); //Silver;
            palette[1] = new Color(1.0f, 1.0f, 1.0f); //White;
            palette[2] = new Color(0.57f, 0.40f, 0.67f); //Cobalt Violet Light;
            palette[3] = new Color(0.314f, 0.784f, 0.471f); //EmeraldGreen;
            palette[4] = new Color(0.071f, 0.039f, 0.561f); //FrenchUltramarine;
            palette[5] = new Color(1.0f, 0.965f, 0.0f); //CadmiumYellow;
            palette[6] = new Color(0.93f, 0.90f, 0.0f); //CadmiumYellowDark;
            palette[7] = new Color(1.0f, 0.97f, 0.0f); //AlizarinCrimson;
            palette[8] = new Color(0.89f, 0.259f, 0.204f); //VermilionRed;
            return palette;
        }

        else
        {
            Color[] palette = new Color[10];
            palette[0] = new Color(0.8627f, 0.8588f, 0.8392f); //LeadWhite;
            palette[1] = new Color(1.0f, 0.965f, 0.0f); //CadiumYellow;
            palette[2] = new Color(0.968f, 0.917f, 0.0f); //Chrome Yellow;
            palette[3] = new Color(0.251f, 0.51f, 0.427f); //ViridianGreen;
            palette[4] = new Color(0.314f, 0.784f, 0.471f); //EmeraldGreen;
            palette[5] = new Color(0.071f, 0.039f, 0.561f); //FrenchUltramarine;
            palette[6] = new Color(0.0f, 0.278f, 0.671f); //CobaltBlue;
            palette[7] = new Color(0.89f, 0.149f, 0.212f); //AlizarinCrimson;
            palette[8] = new Color(0.89f, 0.259f, 0.204f); //VermilionRed;
            palette[9] = new Color(0.16f, 0.14f, 0.13f); //IvoryBlack;
            return palette;
        }

    }
   
    public static double HSVDistanceCompare(Color e1, Color e2, int lever)
    {
        if (lever == 1)
        {
            float H1, S1, V1, H2, S2, V2;
            Color.RGBToHSV(e1, out H1, out S1, out V1);
            Color.RGBToHSV(e2, out H2, out S2, out V2);
            Vector3 x = new Vector3(H1, S1, V1);
            Vector3 y = new Vector3(H2, S2, V2);
            return Vector3.Distance(x, y);
        }
        else
        {
            LABColor lab1 = LABColor.FromColor(e1);
            LABColor lab2 = LABColor.FromColor(e2);

            return LABColor.Distance(lab1, lab2);
        }
    }

    public static double ColourDistanceGreyScale(Color e1, Color e2)
    {
        return Math.Sqrt(Math.Pow(e2.r * 0.299f - e1.r * .3, 2) + Math.Pow(e2.g * 0.587f - e1.g * 0.587f, 2) + Math.Pow(e2.b * 0.114f - e1.b * 0.114f, 2));
    }

    public static double ColourDistance(Color e1, Color e2)
    {
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
