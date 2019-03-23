using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManipulation : MonoBehaviour
{
    [SerializeField] private GameObject model1;
    [SerializeField] private GameObject model2;
    [SerializeField] private GameObject model3;

    [SerializeField] private Text MButton1;
    [SerializeField] private Text MButton2;
    [SerializeField] private Text MButton3;

    [SerializeField] private GameObject xArrow;
    [SerializeField] private GameObject yArrow;
    [SerializeField] private GameObject zArrow;

    private string current;

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
                if (hit.transform.name.Equals(current)) {
                    Debug.Log("gottem");
                } else {
                    current = hit.transform.name;
                    xArrow.SetActive(true);
                    yArrow.SetActive(true);
                    zArrow.SetActive(true);
                    xArrow.transform.localPosition = hit.transform.localPosition;
                    yArrow.transform.localPosition = hit.transform.localPosition;
                    zArrow.transform.localPosition = hit.transform.localPosition;
                }
            } else {
                current = "";
                xArrow.SetActive(false);
                yArrow.SetActive(false);
                zArrow.SetActive(false);
            }
        }
    }

    public void activate1() {
        if(MButton1.text.Equals("+Sword")) {
            MButton1.text = "-Sword";
            model1.SetActive(true);
        } else {
            MButton1.text = "+Sword";
            model1.SetActive(false);
        }
    }

    public void activate2() {
        if (MButton2.text.Equals("+AK47")) {
            MButton2.text = "-AK47";
            model2.SetActive(true);
        }
        else {
            MButton2.text = "+AK47";
            model2.SetActive(false);
        }
    }

    public void activate3() {
        if (MButton3.text.Equals("+Dragon")) {
            MButton3.text = "-Dragon";
            model3.SetActive(true);
        }
        else {
            MButton3.text = "+Dragon";
            model3.SetActive(false);
        }
    }
}
