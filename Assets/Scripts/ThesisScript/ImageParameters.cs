using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageParameters : MonoBehaviour
{
    private float[] brushStrokeOpacities;
    private int colorPaletteSelected;
    private int brushStrokeIndex;

    void Start()
    {
        brushStrokeOpacities = new float[6];
        colorPaletteSelected = -1;
        brushStrokeIndex = -1;
    }

    void Update()
    {
        
    }

    public float[] getBrushStrokeOpacities()
    {
        return brushStrokeOpacities;
    }

    public void setBrushStrokeOpacities(float[] brushOpacities)
    {
        for(int i = 0; i < brushOpacities.Length; i++)
        {
            brushStrokeOpacities[i] = brushOpacities[i];
        }
    }

    public int getColorPaletteSelected()
    {
        return colorPaletteSelected;
    }

    public void setColorPaletteSelected(int colorPalette)
    {
        this.colorPaletteSelected = colorPalette;
    }

    public int getBrushStrokeIndex()
    {
        return brushStrokeIndex;
    }

    public void setBrushStrokeIndex(int index)
    {
        this.brushStrokeIndex = index;
    }
}
