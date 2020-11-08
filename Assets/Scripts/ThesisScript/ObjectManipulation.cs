﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class ObjectManipulation : MonoBehaviour
{
    [SerializeField] Camera arcamera;
    [SerializeField] GameObject imagetarget;

    [SerializeField] private GameObject[] models;
    [SerializeField] private List<GameObject> buttons = new List<GameObject>();

    [SerializeField] private GameObject templateB;
    [SerializeField] private Text templateT;

    [SerializeField] private GameObject paintButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject sliderObj;
    [SerializeField] private GameObject ModelMenu;
    [SerializeField] private UnityEngine.UI.Slider scaleSlider;

    [SerializeField] private GameObject scrollView;
    [SerializeField] private GameObject contents;

    [SerializeField] private GameObject xArrow;
    [SerializeField] private GameObject yArrow;
    [SerializeField] private GameObject zArrow;
    
    private string current = "";
    private Transform cGameTransform;
    private GameObject cGameObject;

    [SerializeField] GameObject buttonPrefab;
    [SerializeField] GameObject brushStrokeButton;
    [SerializeField] GameObject brushStrokeScrollView;
    [SerializeField] GameObject extraOptionsButton;
    [SerializeField] GameObject extraOptionsScrollView;

    // Start is called before the first frame update
    void Start()
    {
        /*
        for(int x = 0; x < models.Length; x++) {
            templateB.name = models[x].name;
            templateT.text = models[x].name;
            
            GameObject newButton = Instantiate(templateB);
            newButton.transform.parent = contents.transform;
            newButton.transform.localPosition = templateB.transform.localPosition;
            newButton.transform.localScale = templateB.transform.localScale;

            newButton.SetActive(true);
        }
        */

        for (int x = 0; x < models.Length; x++)
        {
            GameObject newButton = (GameObject)Instantiate(buttonPrefab);
            newButton.name = models[x].name;
            newButton.GetComponentInChildren<Text>().text = models[x].name;

            string path = Application.persistentDataPath + "/Art/Models-pics/" + models[x].name + ".png";
            newButton.GetComponentInChildren<RawImage>().texture = LoadPNG(path);
           
            newButton.transform.parent = contents.transform;
            newButton.transform.localPosition = buttonPrefab.transform.localPosition;
            newButton.transform.localScale = buttonPrefab.transform.localScale;

            newButton.SetActive(true);
        }


        Screen.autorotateToPortraitUpsideDown = false;
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            var ray = arcamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                if (!hit.transform.name.Equals(current) && !scrollView.active && !extraOptionsScrollView.active && !brushStrokeScrollView.active) {
                    reset();
                    cGameTransform = hit.transform;
                    current = hit.transform.name;

                    backButton.SetActive(true);
                    sliderObj.SetActive(true);
                    xArrow.SetActive(true);
                    yArrow.SetActive(true);
                    zArrow.SetActive(true);
                    
                    cGameTransform.parent = arcamera.transform;
                    scaleSlider.value = cGameTransform.localScale.x;
                    xArrow.transform.localPosition = cGameTransform.localPosition;
                    yArrow.transform.localPosition = cGameTransform.localPosition;
                    zArrow.transform.localPosition = cGameTransform.localPosition;
                }
            } else {
                if(!scrollView.active && !backButton.active && !extraOptionsScrollView.active && !brushStrokeScrollView.active) 
                    reset();
            }
        }
        if (scaleSlider.isActiveAndEnabled) {
            float scale = scaleSlider.value;
            cGameTransform.localScale = new Vector3(scale, scale, scale);
        }
    }
    

    public void activate() {
        string name = EventSystem.current.currentSelectedGameObject.name;
        for(int x = 0; x < models.Length; x++) {
            if (name.Contains(models[x].name)) {
                models[x].SetActive(true);
                models[x].transform.localPosition = new Vector3(0, 0, 0);
                models[x].transform.localRotation = new Quaternion();
            }
        }
        backButton.SetActive(false);
        paintButton.SetActive(true);
        reset();
    }

    public void AddModel() {
        Debug.Log("Adding...");
        paintButton.SetActive(false);
        backButton.SetActive(true);
        scrollView.SetActive(true);
        brushStrokeButton.SetActive(false);
        extraOptionsButton.SetActive(false);
        ModelMenu.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void RemoveModel() {
        Debug.Log("in");
        for (int x = 0; x < models.Length; x++) {
            if (cGameTransform.name.Contains(models[x].name)) {
                models[x].SetActive(false);
            }
        }
        reset();
    }

    public void reset() {
        current = "";
        scrollView.SetActive(false);
        ModelMenu.GetComponent<CanvasGroup>().alpha = 1;
        brushStrokeButton.SetActive(true);
        extraOptionsButton.SetActive(true);

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);
        sliderObj.SetActive(false);

        if (cGameTransform != null)
            cGameTransform.parent = imagetarget.transform;
    }

    public static Texture2D LoadPNG(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }

}
