using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.XphotoModule;

public class PaintScene : MonoBehaviour {
    [SerializeField] private GameObject arcamera;
    [SerializeField] private Camera camera;
    [SerializeField] private ObjectManipulation objectMan;
    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private Image image;
    [SerializeField] private Canvas myCanvas;

    [SerializeField] private int numColorsToMix =2;
    [SerializeField] private int lever = 1;
    [SerializeField] private String imageName;

    public GameObject loadingScreen;
    private Sprite screenshot;
    private string path = "";
    List<Color32> color_palette = new List<Color32>();

    //opencv
    private WebCamTextureToMatHelper webCamTextureToMatHelper;

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

    private void screenShot() {
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
            path = Application.persistentDataPath + "/"+ imageName + ".jpg"; 
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Texture2D orgTexture = new Texture2D(1, 1); 
            orgTexture.LoadImage(bytes);

            if (Screen.orientation == ScreenOrientation.Portrait) {
                int height = texture.height / (texture.width / 480);
                texture = ScaleTexture(texture, 480, height);
                orgTexture = ScaleTexture(orgTexture, 480, height);
            }
            else {
                int width = texture.width / (texture.height / 480);
                texture = ScaleTexture(texture, width, 480);
                orgTexture = ScaleTexture(orgTexture, width, 480);
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
            
            //Get KMEANS
            //Mat label = new Mat(texture.height, texture.width, CvType.CV_8UC1);
            //TermCriteria tc = new TermCriteria(TermCriteria.EPS, 10, 1);
            //Core.kmeans(TextureMat, 20, label, tc, 10, Core.KMEANS_PP_CENTERS);

            Mat grayMat = new Mat(TextureMat.rows(), TextureMat.cols(), CvType.CV_8UC1);
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
            for (int i = 0; i < texture.width; i+=3) {
                for (int j = 0; j < texture.height; j+=3) {
                    int x = rnd.Next(-1, 2) + i;
                    int y = rnd.Next(-1, 2) + j;

                    gridy.Add(y % texture.height);
                    gridx.Add(x % texture.width);
                    index++;
                }
            }
            //shuffle grid
            int n = gridy.Count;
            while (n > 1) {
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
            for (int h = 0; h < index - 1; h += batch_size) {
                pixels = new List<Color32>();
                orgPixels = new List<Color32>();
                int endpoint = h + batch_size;
                if (endpoint > index - 1)
                    endpoint = index - 1;
                //get the color from the texture
                for (int px = h; px < endpoint; px++) {
                    Color32 cpixel = texture.GetPixel(gridx[px], (gridy[px] - (texture.height - 1)) * (-1));
                    pixels.Add(cpixel);
                    cpixel = orgTexture.GetPixel(gridx[px], (gridy[px] - (orgTexture.height - 1)) * (-1));
                    orgPixels.Add(cpixel);
                }
                int cindex = 0;
                for (int px = h; px < endpoint; px++) {
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
                    cpixel = (a+b)/2;
                    //cpixel = a;
                    //} else {
                    //if (cprob > 5) {
                    List<Color32> c_palette = color_palette;
                    c_palette.Remove(pixels[cindex]);
                    cprob = 0;
                    cprob = rnd.Next(0, c_palette.Count-1);
                    a = c_palette[cprob];
                    b = cpixel;
                    //cpixel = (a  + b )/2;
                    //cpixel = b*1.1f;
                        
                    
                    //cpixel = a;

                    //}
                    cindex++;
                    //get angle
                    double length = Math.Round(2+2 * Math.Sqrt(Math.Sqrt(gradienty.get(y, x)[0] * gradienty.get(y, x)[0] + gradientx.get(y, x)[0] * gradientx.get(y, x)[0])));
                    double angle = (180 / Math.PI) * (Math.Atan2(gradienty.get(y, x)[0], gradientx.get(y, x)[0])) + 90;
                    double lengthb = 1;
                    if (length < shortest) {
                        shortest = length;
                        angleShort = angle;
                    }
                    if(length > longest)
                        longest = length;
                    if (length > 2 && angle != 90) {
                        length /= 3;
                    } else {
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
            }
            Debug.Log("Longest : " + longest);
            Debug.Log("Shortest : " + shortest);
            Debug.Log("Angle : " + angleShort);
            Utils.matToTexture2D(rgbaMat, texture);
            
            //OPENCV
            
            Sprite sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
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
    
    private Texture2D changeColor(Texture2D source, Mat centers) {
        Color[] palette = getPalette(centers);

        Color[] rpixels = source.GetPixels(0);
        Color mixed = new Color();
    

        for (int px = 0; px < rpixels.Length; px++) {
            double lowestValue = 0;
            int colorIndex = -1;

            //Numer of times to mix
            //int numColorsToMix = 3;
            
            //First Color Detection
            for (int x = 0; x < 10; x++) {
                if(lowestValue > HSVDistanceCompare(palette[x], rpixels[px], lever) || colorIndex == -1) {
                    lowestValue = HSVDistanceCompare(palette[x], rpixels[px], lever);
                    colorIndex = x;
                }
            }
            
            mixed = palette[colorIndex];

            //Color Mixing
            for (int m = 0; m < numColorsToMix - 1; m++)
            {
                for (int x = 0; x < 10; x++)
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
            
            if (!color_palette.Contains(cpixel)) {
                color_palette.Add(cpixel);
            }
            //rpixels[px] *= 1.5f;
        }
        source.SetPixels(rpixels, 0);
        source.Apply();
        return source;
    }

    public Color[] getPalette(Mat centers) {
        Color[] orgpic = new Color[10];

        double r = 0, g = 0, b = 0;
        int[] arr = new int[2];// 1st is up to 9 then 2nd is up to 3
        for(int x = 0; x <= 9; x++) {
            for(int y = 0; y<=3; y++) {
                arr[0] = x;
                arr[1] = y;
                if (y == 0)
                    r = centers.get(arr)[0];
                else if(y == 1)
                    g = centers.get(arr)[0];
                else if (y == 2)
                    b = centers.get(arr)[0];
            }
            orgpic[x] = new Color((float) r, (float)g, (float)b, 1);
            Debug.Log(orgpic[x]);
        }

        Color[][] palettes = getAllPalettes();

        //return palettes[0];

        int num = 0;
        if (num == 1) {
            Color[] palette = new Color[10];
            return palette;
        } else {
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

    public Color[][] getAllPalettes() {
        Color[][] palettes = new Color[20][];
        Color[] palette1 = new Color[10];
        palette1[0] = new Color(0.4543589f, 0.4225364f, 0.3726439f);
        palette1[1] = new Color(0.75667f, 0.7822415f, 0.7842853f);
        palette1[2] = new Color(0.5529608f, 0.6018273f, 0.6538826f);
        palette1[3] = new Color(0.4973674f, 0.5290362f, 0.5574539f);
        palette1[4] = new Color(0.515651f, 0.5008222f, 0.4673921f);
        palette1[5] = new Color(0.7137745f, 0.7365609f, 0.7384267f);
        palette1[6] = new Color(0.3465873f, 0.3520891f, 0.3642873f);
        palette1[7] = new Color(0.6398488f, 0.662753f, 0.6789462f);
        palette1[8] = new Color(0.4210952f, 0.445389f, 0.4658112f);
        palette1[9] = new Color(0.5973237f, 0.589197f, 0.5661615f);
        palettes[0] = palette1;
        Color[] palette2 = new Color[10];
        palette2[0] = new Color(0.6134503f, 0.6145477f, 0.5654253f);
        palette2[1] = new Color(0.3805538f, 0.4042504f, 0.4008004f);
        palette2[2] = new Color(0.6434704f, 0.6800621f, 0.6521712f);
        palette2[3] = new Color(0.5132888f, 0.5096062f, 0.4731368f);
        palette2[4] = new Color(0.2491482f, 0.2937473f, 0.3078351f);
        palette2[5] = new Color(0.7185919f, 0.7363993f, 0.6916769f);
        palette2[6] = new Color(0.4311045f, 0.4857974f, 0.507238f);
        palette2[7] = new Color(0.5629441f, 0.625541f, 0.6271745f);
        palette2[8] = new Color(0.5190136f, 0.5618676f, 0.5546733f);
        palette2[9] = new Color(0.7375074f, 0.7092663f, 0.5893683f);
        palettes[1] = palette2;
        Color[] palette3 = new Color[10];
        palette3[0] = new Color(0.408264f, 0.47285f, 0.4487566f);
        palette3[1] = new Color(0.6876056f, 0.7220944f, 0.7007343f);
        palette3[2] = new Color(0.6084615f, 0.6551684f, 0.6283157f);
        palette3[3] = new Color(0.3276137f, 0.393463f, 0.2975006f);
        palette3[4] = new Color(0.5126147f, 0.5469798f, 0.4411395f);
        palette3[5] = new Color(0.5467349f, 0.5931788f, 0.5547032f);
        palette3[6] = new Color(0.3325813f, 0.4131669f, 0.397377f);
        palette3[7] = new Color(0.7577199f, 0.7978467f, 0.7869734f);
        palette3[8] = new Color(0.4536597f, 0.522623f, 0.5325086f);
        palette3[9] = new Color(0.4432921f, 0.4726807f, 0.3156845f);
        palettes[2] = palette3;
        Color[] palette4 = new Color[10];
        palette4[0] = new Color(0.3144264f, 0.3414707f, 0.4009414f);
        palette4[1] = new Color(0.4274375f, 0.3975669f, 0.4044792f);
        palette4[2] = new Color(0.2472565f, 0.2903652f, 0.360913f);
        palette4[3] = new Color(0.4139406f, 0.3741189f, 0.374352f);
        palette4[4] = new Color(0.4866226f, 0.4578542f, 0.4643114f);
        palette4[5] = new Color(0.334172f, 0.3216837f, 0.3492293f);
        palette4[6] = new Color(0.2732587f, 0.322151f, 0.3965252f);
        palette4[7] = new Color(0.3867649f, 0.349288f, 0.3522764f);
        palette4[8] = new Color(0.3748112f, 0.3693995f, 0.3937642f);
        palette4[9] = new Color(0.460322f, 0.4230004f, 0.4277186f);
        palettes[3] = palette4;
        Color[] palette5 = new Color[10];
        palette5[0] = new Color(0.2598629f, 0.2780161f, 0.2405372f);
        palette5[1] = new Color(0.6562274f, 0.6389561f, 0.5877509f);
        palette5[2] = new Color(0.5365145f, 0.5390936f, 0.5256548f);
        palette5[3] = new Color(0.802872f, 0.7797624f, 0.7368523f);
        palette5[4] = new Color(0.3918145f, 0.408544f, 0.376489f);
        palette5[5] = new Color(0.7192235f, 0.6931329f, 0.6427115f);
        palette5[6] = new Color(0.5981792f, 0.5883644f, 0.5425531f);
        palette5[7] = new Color(0.4798993f, 0.4876221f, 0.4649566f);
        palette5[8] = new Color(0.3668854f, 0.3890251f, 0.2702099f);
        palette5[9] = new Color(0.3129945f, 0.3388627f, 0.3069572f);
        palettes[4] = palette5;
        Color[] palette6 = new Color[10];
        palette6[0] = new Color(0.6521623f, 0.6609101f, 0.6550167f);
        palette6[1] = new Color(0.5604674f, 0.651346f, 0.6693364f);
        palette6[2] = new Color(0.7349713f, 0.7508888f, 0.7530447f);
        palette6[3] = new Color(0.4995867f, 0.6044791f, 0.6339514f);
        palette6[4] = new Color(0.6955321f, 0.7102917f, 0.7065498f);
        palette6[5] = new Color(0.632376f, 0.6943685f, 0.7116122f);
        palette6[6] = new Color(0.5863347f, 0.5880346f, 0.5968837f);
        palette6[7] = new Color(0.3390104f, 0.3269792f, 0.3282813f);
        palette6[8] = new Color(0.4501963f, 0.5468696f, 0.5935512f);
        palette6[9] = new Color(0.5340801f, 0.5202764f, 0.5433552f);
        palettes[5] = palette6;
        Color[] palette7 = new Color[10];
        palette7[0] = new Color(0.6118627f, 0.6053909f, 0.6120263f);
        palette7[1] = new Color(0.6589772f, 0.6518069f, 0.6575629f);
        palette7[2] = new Color(0.6371369f, 0.6282865f, 0.6356912f);
        palette7[3] = new Color(0.5771857f, 0.5775052f, 0.5880852f);
        palette7[4] = new Color(0.6523438f, 0.569551f, 0.5597007f);
        palette7[5] = new Color(0.7541663f, 0.6574739f, 0.6547405f);
        palette7[6] = new Color(0.5149871f, 0.5502816f, 0.6019019f);
        palette7[7] = new Color(0.5648355f, 0.5911764f, 0.6490465f);
        palette7[8] = new Color(0.6878411f, 0.67897f, 0.6824499f);
        palette7[9] = new Color(0.6812977f, 0.6301944f, 0.6138095f);
        palettes[6] = palette7;
        Color[] palette8 = new Color[10];
        palette8[0] = new Color(0.3657527f, 0.4794477f, 0.468688f);
        palette8[1] = new Color(0.5459028f, 0.6352768f, 0.5961739f);
        palette8[2] = new Color(0.5857077f, 0.4856436f, 0.5450687f);
        palette8[3] = new Color(0.5236708f, 0.5821266f, 0.4641211f);
        palette8[4] = new Color(0.6805158f, 0.7193611f, 0.7042409f);
        palette8[5] = new Color(0.4467334f, 0.5424123f, 0.5487682f);
        palette8[6] = new Color(0.310215f, 0.3874296f, 0.3771847f);
        palette8[7] = new Color(0.6467286f, 0.663257f, 0.5239775f);
        palette8[8] = new Color(0.4678032f, 0.4378277f, 0.4352321f);
        palette8[9] = new Color(0.6088566f, 0.5638956f, 0.7073793f);
        palettes[7] = palette8;
        Color[] palette9 = new Color[10];
        palette9[0] = new Color(0.590872f, 0.5855726f, 0.4844806f);
        palette9[1] = new Color(0.5135081f, 0.4672143f, 0.3186707f);
        palette9[2] = new Color(0.5716878f, 0.6460692f, 0.6615548f);
        palette9[3] = new Color(0.6893355f, 0.698902f, 0.6371268f);
        palette9[4] = new Color(0.4150873f, 0.4601619f, 0.4154892f);
        palette9[5] = new Color(0.3669859f, 0.3617405f, 0.2723023f);
        palette9[6] = new Color(0.5103198f, 0.5609663f, 0.5386027f);
        palette9[7] = new Color(0.5157797f, 0.3967938f, 0.1920527f);
        palette9[8] = new Color(0.6285143f, 0.637586f, 0.5622508f);
        palette9[9] = new Color(0.5354495f, 0.5258468f, 0.4182283f);
        palettes[8] = palette9;
        Color[] palette10 = new Color[10];
        palette10[0] = new Color(0.6089487f, 0.6030036f, 0.6096816f);
        palette10[1] = new Color(0.6556751f, 0.6482291f, 0.6537438f);
        palette10[2] = new Color(0.5746147f, 0.575574f, 0.5869582f);
        palette10[3] = new Color(0.6839369f, 0.6756541f, 0.6800469f);
        palette10[4] = new Color(0.7535216f, 0.6579584f, 0.6548214f);
        palette10[5] = new Color(0.5132971f, 0.5498528f, 0.6029718f);
        palette10[6] = new Color(0.6817232f, 0.6293254f, 0.6128765f);
        palette10[7] = new Color(0.5647115f, 0.5912126f, 0.6493547f);
        palette10[8] = new Color(0.6340922f, 0.6249594f, 0.6323966f);
        palette10[9] = new Color(0.6520306f, 0.5679941f, 0.5584745f);
        palettes[9] = palette10;
        Color[] palette11 = new Color[10];
        palette11[0] = new Color(0.4629771f, 0.4552791f, 0.3417548f);
        palette11[1] = new Color(0.6336826f, 0.5981795f, 0.4557266f);
        palette11[2] = new Color(0.2718239f, 0.2881885f, 0.2954597f);
        palette11[3] = new Color(0.3421977f, 0.3767919f, 0.3853249f);
        palette11[4] = new Color(0.5524194f, 0.5329711f, 0.4034726f);
        palette11[5] = new Color(0.7140666f, 0.690607f, 0.6163217f);
        palette11[6] = new Color(0.4288729f, 0.4626266f, 0.4619843f);
        palette11[7] = new Color(0.7140325f, 0.6594003f, 0.4888808f);
        palette11[8] = new Color(0.5359526f, 0.5522348f, 0.5231222f);
        palette11[9] = new Color(0.6483538f, 0.6271173f, 0.5508857f);
        palettes[10] = palette11;
        Color[] palette12 = new Color[10];
        palette12[0] = new Color(0.5245705f, 0.4820365f, 0.5994645f);
        palette12[1] = new Color(0.6969981f, 0.6191672f, 0.7046881f);
        palette12[2] = new Color(0.5828916f, 0.3238567f, 0.2778691f);
        palette12[3] = new Color(0.8451224f, 0.7568411f, 0.7482923f);
        palette12[4] = new Color(0.6383707f, 0.4307342f, 0.4469995f);
        palette12[5] = new Color(0.6266858f, 0.55437f, 0.6558481f);
        palette12[6] = new Color(0.7958069f, 0.667645f, 0.6565015f);
        palette12[7] = new Color(0.6768785f, 0.4989862f, 0.5597881f);
        palette12[8] = new Color(0.7176549f, 0.5950106f, 0.6106505f);
        palette12[9] = new Color(0.7524661f, 0.6855834f, 0.7628824f);
        palettes[11] = palette12;
        Color[] palette13 = new Color[10];
        palette13[0] = new Color(0.2273794f, 0.2427913f, 0.2883285f);
        palette13[1] = new Color(0.610408f, 0.5874407f, 0.5785545f);
        palette13[2] = new Color(0.4245002f, 0.4247586f, 0.3918278f);
        palette13[3] = new Color(0.3572688f, 0.3540255f, 0.336172f);
        palette13[4] = new Color(0.1597757f, 0.1756987f, 0.2442379f);
        palette13[5] = new Color(0.490889f, 0.4715789f, 0.4900071f);
        palette13[6] = new Color(0.2788016f, 0.3045101f, 0.3284447f);
        palette13[7] = new Color(0.327625f, 0.3775193f, 0.4080277f);
        palette13[8] = new Color(0.5603603f, 0.3573524f, 0.3228299f);
        palette13[9] = new Color(0.5683798f, 0.5577986f, 0.3837993f);
        palettes[12] = palette13;
        Color[] palette14 = new Color[10];
        palette14[0] = new Color(0.4846703f, 0.5460722f, 0.6451986f);
        palette14[1] = new Color(0.6451805f, 0.6837912f, 0.6783476f);
        palette14[2] = new Color(0.5692032f, 0.617708f, 0.6408731f);
        palette14[3] = new Color(0.7646286f, 0.8078007f, 0.7866507f);
        palette14[4] = new Color(0.2937244f, 0.3537274f, 0.5749525f);
        palette14[5] = new Color(0.6420595f, 0.6558859f, 0.5251854f);
        palette14[6] = new Color(0.5189811f, 0.5578461f, 0.5375835f);
        palette14[7] = new Color(0.7036352f, 0.7438723f, 0.7374129f);
        palette14[8] = new Color(0.4074493f, 0.4665321f, 0.5918831f);
        palette14[9] = new Color(0.7627646f, 0.7617106f, 0.6043631f);
        palettes[13] = palette14;
        Color[] palette15 = new Color[10];
        palette15[0] = new Color(0.7412313f, 0.7372103f, 0.782928f);
        palette15[1] = new Color(0.6251884f, 0.6385493f, 0.3686172f);
        palette15[2] = new Color(0.8427879f, 0.8243126f, 0.8393552f);
        palette15[3] = new Color(0.2615357f, 0.3390472f, 0.3205809f);
        palette15[4] = new Color(0.4701635f, 0.5644752f, 0.5637866f);
        palette15[5] = new Color(0.3902204f, 0.5007029f, 0.4684881f);
        palette15[6] = new Color(0.4765769f, 0.5445233f, 0.3989534f);
        palette15[7] = new Color(0.3640949f, 0.452697f, 0.360565f);
        palette15[8] = new Color(0.6013945f, 0.632754f, 0.5455289f);
        palette15[9] = new Color(0.6019385f, 0.655223f, 0.7518787f);
        palettes[14] = palette15;
        Color[] palette16 = new Color[10];
        palette16[0] = new Color(0.2895469f, 0.4903502f, 0.6156157f);
        palette16[1] = new Color(0.4257213f, 0.5681931f, 0.6439831f);
        palette16[2] = new Color(0.5370468f, 0.6958252f, 0.8633523f);
        palette16[3] = new Color(0.7497359f, 0.7578226f, 0.7925764f);
        palette16[4] = new Color(0.375226f, 0.5021557f, 0.4944202f);
        palette16[5] = new Color(0.3490734f, 0.4401586f, 0.327141f);
        palette16[6] = new Color(0.5511098f, 0.6425373f, 0.7128925f);
        palette16[7] = new Color(0.444354f, 0.6133121f, 0.7774845f);
        palette16[8] = new Color(0.3199076f, 0.5455486f, 0.7417374f);
        palette16[9] = new Color(0.564272f, 0.5955351f, 0.4958472f);
        palettes[15] = palette16;
        Color[] palette17 = new Color[10];
        palette17[0] = new Color(0.5481471f, 0.5908293f, 0.5492339f);
        palette17[1] = new Color(0.3111174f, 0.4150814f, 0.3834285f);
        palette17[2] = new Color(0.4606915f, 0.5323535f, 0.4603915f);
        palette17[3] = new Color(0.2074241f, 0.2756317f, 0.2912653f);
        palette17[4] = new Color(0.5801881f, 0.5932811f, 0.4174521f);
        palette17[5] = new Color(0.3851551f, 0.471812f, 0.4110289f);
        palette17[6] = new Color(0.3671989f, 0.3888596f, 0.3096673f);
        palette17[7] = new Color(0.7313943f, 0.6987219f, 0.5895171f);
        palette17[8] = new Color(0.2624857f, 0.3494913f, 0.326438f);
        palette17[9] = new Color(0.4666624f, 0.4906431f, 0.3416154f);
        palettes[16] = palette17;
        Color[] palette18 = new Color[10];
        palette18[0] = new Color(0.520506f, 0.4506093f, 0.5330921f);
        palette18[1] = new Color(0.8550762f, 0.8348041f, 0.8165054f);
        palette18[2] = new Color(0.8110067f, 0.685843f, 0.5790952f);
        palette18[3] = new Color(0.8310422f, 0.7564283f, 0.699112f);
        palette18[4] = new Color(0.5785297f, 0.6493468f, 0.8333429f);
        palette18[5] = new Color(0.4832955f, 0.3813649f, 0.4228162f);
        palette18[6] = new Color(0.6160423f, 0.4135357f, 0.3495739f);
        palette18[7] = new Color(0.7352482f, 0.5800542f, 0.4836715f);
        palette18[8] = new Color(0.6986428f, 0.6426013f, 0.6619148f);
        palette18[9] = new Color(0.5380958f, 0.5267984f, 0.6623191f);
        palettes[17] = palette18;
        Color[] palette19 = new Color[10];
        palette19[0] = new Color(0.7366556f, 0.7473565f, 0.7572416f);
        palette19[1] = new Color(0.4909341f, 0.473709f, 0.4459949f);
        palette19[2] = new Color(0.6309434f, 0.6301894f, 0.6261618f);
        palette19[3] = new Color(0.2849642f, 0.2589993f, 0.2369484f);
        palette19[4] = new Color(0.6719077f, 0.673538f, 0.6731344f);
        palette19[5] = new Color(0.3755919f, 0.3431463f, 0.3034778f);
        palette19[6] = new Color(0.5897197f, 0.5860568f, 0.5757028f);
        palette19[7] = new Color(0.7035682f, 0.7101171f, 0.7156034f);
        palette19[8] = new Color(0.5356267f, 0.5323374f, 0.5208732f);
        palette19[9] = new Color(0.4428402f, 0.4139266f, 0.3749736f);
        palettes[18] = palette19;
        Color[] palette20 = new Color[10];
        palette20[0] = new Color(0.570663f, 0.5230171f, 0.3517743f);
        palette20[1] = new Color(0.2303497f, 0.2585128f, 0.2553082f);
        palette20[2] = new Color(0.6026779f, 0.7004287f, 0.7573442f);
        palette20[3] = new Color(0.4867529f, 0.502694f, 0.4611297f);
        palette20[4] = new Color(0.3163122f, 0.3386309f, 0.3200745f);
        palette20[5] = new Color(0.6382511f, 0.5816284f, 0.4695515f);
        palette20[6] = new Color(0.7476048f, 0.7130359f, 0.7004979f);
        palette20[7] = new Color(0.3817648f, 0.4160028f, 0.4191318f);
        palette20[8] = new Color(0.4529647f, 0.4341676f, 0.3124166f);
        palette20[9] = new Color(0.5313613f, 0.5821003f, 0.5975538f);
        palettes[19] = palette20;
        return palettes;
    }

    public static double HSVDistanceCompare(Color e1, Color e2, int lever) {
        if(lever == 1) {
            float H1, S1, V1, H2, S2, V2;
            Color.RGBToHSV(e1, out H1, out S1, out V1);
            Color.RGBToHSV(e2, out H2, out S2, out V2);
            Vector3 x = new Vector3(H1, S1, V1);
            Vector3 y = new Vector3(H2, S2, V2);
            return Vector3.Distance(x, y);
        }  else {
            LABColor lab1 = LABColor.FromColor(e1);
            LABColor lab2 = LABColor.FromColor(e2);

            return LABColor.Distance(lab1, lab2);
        }
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
