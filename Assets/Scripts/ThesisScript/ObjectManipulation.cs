using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
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

    [SerializeField] private GameObject scrollView;
    [SerializeField] private GameObject contents;

    [SerializeField] private GameObject xArrow;
    [SerializeField] private GameObject yArrow;
    [SerializeField] private GameObject zArrow;
    
    private string current = "";
    private Transform cGameTransform;
    private GameObject cGameObject;

    [SerializeField] GameObject buttonPrefab;

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

            string path = Application.dataPath + "/Art/Models-pics/"+ models[x].name + ".png";

            //Debug.Log("SHIT " + myTexture.name);

            newButton.GetComponentInChildren<RawImage>().texture = LoadPNG(path);
           
            newButton.transform.parent = contents.transform;
            newButton.transform.localPosition = buttonPrefab.transform.localPosition;
            newButton.transform.localScale = buttonPrefab.transform.localScale;

            newButton.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            var ray = arcamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                if (!hit.transform.name.Equals(current) && !scrollView.active) {
                    reset();
                    cGameTransform = hit.transform;
                    current = hit.transform.name;
                    

                    xArrow.SetActive(true);
                    yArrow.SetActive(true);
                    zArrow.SetActive(true);
                    
                    cGameTransform.parent = arcamera.transform;
                    xArrow.transform.localPosition = cGameTransform.localPosition;
                    yArrow.transform.localPosition = cGameTransform.localPosition;
                    zArrow.transform.localPosition = cGameTransform.localPosition;
                }
            } else {
                if(!scrollView.active && !backButton.active) 
                    reset();
            }
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
        
    }

    public void RemoveModel() {
        Debug.Log("in");
        for (int x = 0; x < models.Length; x++) {
            if (cGameTransform.name.Contains(models[x].name)) {
                //Debug.Log(cGameTransform.name);
                models[x].SetActive(false);
            }
        }
        reset();
    }

    public void reset() {
        current = "";
        scrollView.SetActive(false);

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);

        if(cGameTransform != null)
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
