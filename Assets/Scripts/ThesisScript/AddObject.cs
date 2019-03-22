using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObject : MonoBehaviour
{
    [SerializeField] private GameObject model1;
    [SerializeField] private GameObject model2;
    [SerializeField] private GameObject model3;
    [SerializeField] private GameObject addButton;
    [SerializeField] private GameObject removeButton;
    int count;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void activate() {
        if (count == 0) {
            model1.SetActive(true);
            count++;
        }
        else if(count == 1) {
            model2.SetActive(true);
            count++;
        }
        else if (count == 2) {
            model3.SetActive(true);
            count++;
            addButton.SetActive(false);
            removeButton.SetActive(true);
        }
        else if (count == 3) {
            model1.SetActive(false);
            count++;
        }
        else if (count == 4) {
            model2.SetActive(false);
            count++;
        }
        else if (count == 5) {
            model3.SetActive(false);
            count = 0;
            addButton.SetActive(true);
            removeButton.SetActive(false);
        }
    }
}
