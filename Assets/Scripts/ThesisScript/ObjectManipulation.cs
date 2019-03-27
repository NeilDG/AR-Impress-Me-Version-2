using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManipulation : MonoBehaviour
{
    [SerializeField] Camera arcamera;
    [SerializeField] GameObject imagetarget;

    [SerializeField] private GameObject model1;
    [SerializeField] private GameObject model2;
    [SerializeField] private GameObject model3;

    [SerializeField] private GameObject MButt1;
    [SerializeField] private GameObject MButt2;
    [SerializeField] private GameObject MButt3;

    [SerializeField] private Text MText1;
    [SerializeField] private Text MText2;
    [SerializeField] private Text MText3;

    [SerializeField] private GameObject xArrow;
    [SerializeField] private GameObject yArrow;
    [SerializeField] private GameObject zArrow;



    private string current = "";
    private string cTransform = "";
    private Transform cGameTransform;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                if (!hit.transform.name.Equals(current)) {
                    reset();
                    cGameTransform = hit.transform;
                    current = hit.transform.name;

                    MButt1.SetActive(false);
                    MButt2.SetActive(false);
                    MButt3.SetActive(false);

                    xArrow.SetActive(true);
                    yArrow.SetActive(true);
                    zArrow.SetActive(true);
                    
                    cGameTransform.parent = arcamera.transform;
                    xArrow.transform.localPosition = cGameTransform.localPosition;
                    yArrow.transform.localPosition = cGameTransform.localPosition;
                    zArrow.transform.localPosition = cGameTransform.localPosition;
                }
            } else {
                reset();
            }
        }
    }

    public void reset() {
        current = "";
        cTransform = "";
        MButt1.SetActive(true);
        MButt2.SetActive(true);
        MButt3.SetActive(true);

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);

        if(cGameTransform != null)
            cGameTransform.parent = imagetarget.transform;
    }

    public void moveByX() {
        cTransform = "x";
    }

    public void moveByY() {
        cTransform = "y";
    }

    public void moveByZ() {
        cTransform = "z";
    }

    public void activate1() {
        if(MText1.text.Equals("+Sword")) {
            MText1.text = "-Sword";
            model1.SetActive(true);
        } else {
            MText1.text = "+Sword";
            model1.SetActive(false);
        }
    }

    public void activate2() {
        if (MText2.text.Equals("+AK47")) {
            MText2.text = "-AK47";
            model2.SetActive(true);
        }
        else {
            MText2.text = "+AK47";
            model2.SetActive(false);
        }
    }

    public void activate3() {
        if (MText3.text.Equals("+Dragon")) {
            MText3.text = "-Dragon";
            model3.SetActive(true);
        }
        else {
            MText3.text = "+Dragon";
            model3.SetActive(false);
        }
    }
}
