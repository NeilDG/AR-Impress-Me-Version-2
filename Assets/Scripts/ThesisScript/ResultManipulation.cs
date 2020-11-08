using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.XphotoModule;


public class ResultManipulation : MonoBehaviour
{
    [SerializeField] private RawImage[] brushStrokes;

    [SerializeField] private GameObject scrollView;
    [SerializeField] private GameObject contents;

    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject applyButton;
    [SerializeField] private GameObject applyButton2;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject brushStrokeButton;
    [SerializeField] private GameObject modelMenu;

    [SerializeField] private GameObject optionButton;
    [SerializeField] private GameObject extraOptionButton;
    [SerializeField] private GameObject extraOptionsScrollView;
    [SerializeField] private GameObject firstCancelButton;
    [SerializeField] private GameObject secondCancelButton;

    [SerializeField] private GameObject dynamicSelectButton;
    [SerializeField] private GameObject pre1886SelectButton;
    [SerializeField] private GameObject post1886SelectButton;
    [SerializeField] private GameObject finalYearsSelectButton;


    [SerializeField] private Slider brush128Slider;
    [SerializeField] private Slider brush64Slider;
    [SerializeField] private Slider brush32Slider;
    [SerializeField] private Slider brush8Slider;
    [SerializeField] private Slider brush4Slider;
    [SerializeField] private Slider brush2Slider;

    [SerializeField] private Text brush128Text;
    [SerializeField] private Text brush64Text;
    [SerializeField] private Text brush32Text;
    [SerializeField] private Text brush8Text;
    [SerializeField] private Text brush4Text;
    [SerializeField] private Text brush2Text;

    [SerializeField] private GameObject pre1886SelectedText;
    [SerializeField] private GameObject post1886SelectedText;
    [SerializeField] private GameObject finalYearsSelectedText;

    private int brushStrokeIndex = 0;
    private bool isRendered = false;
    private float brush128SliderVal = 1.0f;
    private float brush64SliderVal = 1.0f;
    private float brush32SliderVal = 1.0f;
    private float brush8SliderVal = 0.5f;
    private float brush4SliderVal = 0.2f;
    private float brush2SliderVal = 0.1f;

    private int dynamicallySelectedColorPalette = -1;
    private int selectedPalette = -1;
    private int select = -1;

    private PaintScene paintScene;

    private string currentDisplayedImageName;
    // Start is called before the first frame update
    void Start()
    {
        paintScene = gameObject.GetComponent<PaintScene>();
        for (int x = 0; x < brushStrokes.Length; x++)
        {
            GameObject newButton = (GameObject)Instantiate(buttonPrefab);
            newButton.name = brushStrokes[x].name;
            newButton.GetComponentInChildren<Text>().text = brushStrokes[x].name;

            newButton.GetComponentInChildren<RawImage>().texture = brushStrokes[x].texture;

            newButton.transform.parent = contents.transform;
            newButton.transform.localPosition = buttonPrefab.transform.localPosition;
            newButton.transform.localScale = buttonPrefab.transform.localScale;

            newButton.SetActive(true);
        }
        Screen.autorotateToPortraitUpsideDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        brush128Text.text = "Brush Radius 128\nOpacity: " + brush128Slider.value.ToString();
        brush64Text.text = "Brush Radius 64\nOpacity: " + brush64Slider.value.ToString();
        brush32Text.text = "Brush Radius 32\nOpacity: " + brush32Slider.value.ToString();
        brush8Text.text = "Brush Radius 8\nOpacity: " + brush8Slider.value.ToString();
        brush4Text.text = "Brush Radius 4\nOpacity: " + brush4Slider.value.ToString();
        brush2Text.text = "Brush Radius 2\nOpacity: " + brush2Slider.value.ToString();

        if (isRendered)
        {
            if (dynamicallySelectedColorPalette == 0)
                pre1886SelectedText.SetActive(true);
            else if (dynamicallySelectedColorPalette == 1)
                post1886SelectedText.SetActive(true);
            else if (dynamicallySelectedColorPalette == 2)
                finalYearsSelectedText.SetActive(true);
                
        }
    }

    public void selectBrushStroke()
    {
        scrollView.SetActive(true);
        firstCancelButton.SetActive(true);
        brushStrokeButton.SetActive(false);
        extraOptionButton.SetActive(false);
        modelMenu.GetComponent<CanvasGroup>().alpha = 0;
        paintButton.SetActive(false);
        if (isRendered)
        {
            secondCancelButton.SetActive(false);
        }
        applyButton2.SetActive(false);
    }

    public void showExtraOptions()
    {
        optionButton.SetActive(false);
        applyButton2.SetActive(true);
        extraOptionButton.SetActive(true);
        brushStrokeButton.SetActive(true);
        secondCancelButton.SetActive(true);
    }

    public void showExtraOptionsScrollView()
    {
        extraOptionButton.SetActive(false);
        extraOptionsScrollView.SetActive(true);
        firstCancelButton.SetActive(true);
        brushStrokeButton.SetActive(false);
        modelMenu.GetComponent<CanvasGroup>().alpha = 0;
        paintButton.SetActive(false);
        applyButton.SetActive(true);
        applyButton2.SetActive(false);

        if (isRendered)
        {
            secondCancelButton.SetActive(false);
        }
    }

    public void reset()
    {
        scrollView.SetActive(false);
        extraOptionsScrollView.SetActive(false);
        modelMenu.GetComponent<CanvasGroup>().alpha = 1;
        brushStrokeButton.SetActive(true);
        extraOptionButton.SetActive(true);
        firstCancelButton.SetActive(false);
        applyButton.SetActive(false);

        if (!isRendered)
        {
            paintButton.SetActive(true);
        }
        else
        {
            secondCancelButton.SetActive(true);
            applyButton2.SetActive(true);
        }
    }

    public void renderedReset(float[] brushStrokeOpacities, int brushStrokeIndex, int colorPalette)
    {
        optionButton.SetActive(true);
        extraOptionButton.SetActive(false);
        brushStrokeButton.SetActive(false);
        secondCancelButton.SetActive(false);
        applyButton2.SetActive(false);
        backButton.SetActive(true);

        brush128Slider.value = brushStrokeOpacities[0];
        brush64Slider.value = brushStrokeOpacities[1];
        brush32Slider.value = brushStrokeOpacities[2];
        brush8Slider.value = brushStrokeOpacities[3];
        brush4Slider.value = brushStrokeOpacities[4];
        brush2Slider.value = brushStrokeOpacities[5];
        brush128SliderVal = brushStrokeOpacities[0];
        brush64SliderVal = brushStrokeOpacities[1];
        brush32SliderVal = brushStrokeOpacities[2];
        brush8SliderVal = brushStrokeOpacities[3];
        brush4SliderVal = brushStrokeOpacities[4];
        brush2SliderVal = brushStrokeOpacities[5];

        this.brushStrokeIndex = brushStrokeIndex;
        brushStrokeButton.GetComponentInChildren<RawImage>().texture = brushStrokes[brushStrokeIndex].texture;
        this.select = colorPalette;
        this.selectedPalette = colorPalette;
        resetColorPaletteButtons();
    }

    public void renderedApply()
    {
        optionButton.SetActive(true);
        extraOptionButton.SetActive(false);
        brushStrokeButton.SetActive(false);
        secondCancelButton.SetActive(false);
        applyButton2.SetActive(false);
        backButton.SetActive(true);
    }

    public void resetColorPaletteButtons()
    {
        if (selectedPalette == 0)
            selectPre1886();
        else if (selectedPalette == 1)
            selectPost1886();
        else if (selectedPalette == 2)
            selectFinalYears();
    }

    public void activate()
    {
        string name = EventSystem.current.currentSelectedGameObject.name;

        for (int x = 0; x < brushStrokes.Length; x++)
        {
            if (name.Contains(brushStrokes[x].name))
            {
                Debug.Log(brushStrokes[x].name);
                setBrushStrokeIndex(x);
                brushStrokeButton.GetComponentInChildren<RawImage>().texture = brushStrokes[x].texture;
            }
        }

        backButton.SetActive(false);
        paintButton.SetActive(true);
        reset();
    }

    public int getBrushStrokeIndex()
    {
        return this.brushStrokeIndex;
    }

    public void setBrushStrokeIndex(int index)
    {
        this.brushStrokeIndex = index;
    }

    public void setIsRendered(bool isRendered)
    {
        this.isRendered = isRendered;
    }

    public void setChosenDynamicPalette(int chosenDynamicPalette)
    {
        this.dynamicallySelectedColorPalette = chosenDynamicPalette;
    }

    public void setSelectedPalette(int selected)
    {
        this.selectedPalette = selected;
    }

    public int getSelectedPalette()
    {
        return this.selectedPalette;
    }

    public void setCurrentDisplayedImageName(string name)
    {
        this.currentDisplayedImageName = name;
    }

    public string getCurrentDisplayedImageName()
    {
        return this.currentDisplayedImageName;
    }

    public void applyExtraOptions()
    {
        brush128SliderVal = brush128Slider.value;
        brush64SliderVal = brush64Slider.value;
        brush32SliderVal = brush32Slider.value;
        brush8SliderVal = brush8Slider.value;
        brush4SliderVal = brush4Slider.value;
        brush2SliderVal = brush2Slider.value;
        if (select == -1)
            selectedPalette = -1;
        else if (select == 0)
            selectedPalette = 0;
        else if (select == 1)
            selectedPalette = 1;
        else if (select == 2)
            selectedPalette = 2;
        reset();


    }

    public void cancelFirst()
    {
        brush128Slider.value = brush128SliderVal;
        brush64Slider.value = brush64SliderVal;
        brush32Slider.value = brush32SliderVal;
        brush8Slider.value = brush8SliderVal;
        brush4Slider.value = brush4SliderVal;
        brush2Slider.value = brush2SliderVal;
        if (selectedPalette == -1)
            selectDynamicPalette();
        else if (selectedPalette == 0)
            selectPre1886();
        else if (selectedPalette == 1)
            selectPost1886();
        else if (selectedPalette == 2)
            selectFinalYears();
        reset();
    }

    public void setBrushValues(float brush128Val, float brush64Val, float brush32Val, float brush8Val, float brush4Val, float brush2Val)
    {
        this.brush128SliderVal = brush128Val;
        this.brush64SliderVal = brush64Val;
        this.brush32SliderVal = brush32Val;
        this.brush8SliderVal = brush8Val;
        this.brush4SliderVal = brush4Val;
        this.brush2SliderVal = brush2Val;
    }

    public float[] getBrushValues()
    {
        float[] brushValues = new float[6];

        brushValues[0] = brush128SliderVal;
        brushValues[1] = brush64SliderVal;
        brushValues[2] = brush32SliderVal;
        brushValues[3] = brush8SliderVal;
        brushValues[4] = brush4SliderVal;
        brushValues[5] = brush2SliderVal;

        return brushValues;
    }

    public void selectDynamicPalette()
    {
        dynamicSelectButton.SetActive(false);
        pre1886SelectButton.SetActive(true);
        post1886SelectButton.SetActive(true);
        finalYearsSelectButton.SetActive(true);
        select = -1;
    }

    public void selectPre1886()
    {
        if (!isRendered)
            dynamicSelectButton.SetActive(true);
        pre1886SelectButton.SetActive(false);
        post1886SelectButton.SetActive(true);
        finalYearsSelectButton.SetActive(true);
        select = 0;
    }

    public void selectPost1886()
    {
        if (!isRendered)
            dynamicSelectButton.SetActive(true);
        pre1886SelectButton.SetActive(true);
        post1886SelectButton.SetActive(false);
        finalYearsSelectButton.SetActive(true);
        select = 1;
    }

    public void selectFinalYears()
    {
        if (!isRendered)
            dynamicSelectButton.SetActive(true);
        pre1886SelectButton.SetActive(true);
        post1886SelectButton.SetActive(true);
        finalYearsSelectButton.SetActive(false);
        select = 2;
    }
}
