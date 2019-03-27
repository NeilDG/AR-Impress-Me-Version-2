using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSwitcher : MonoBehaviour
{
    public GameObject BasketBall;
    public GameObject BoxOfCereal;
    public GameObject CruchFlakesBox;
    public GameObject FootBallHelmet;
    public GameObject RubicksCube;
    public GameObject TreeTrunk;
    public GameObject StarPlatinum;
    public GameObject Bear;
    public GameObject MilleniumFalcon;
    public GameObject CartoonMill;

    private int nSwitch;

    // Start is called before the first frame update
    void Start()
    {
        nSwitch = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            SwitchMeshes();

        }
    }

    public void SwitchMeshes()
    {
        nSwitch += 1;
        switch (nSwitch)
        {
            case 1:
                CartoonMill.SetActive(false);
                BasketBall.SetActive(true);
                break;
            case 2:
                BasketBall.SetActive(false);
                BoxOfCereal.SetActive(true);
                break;
            case 3:
                BoxOfCereal.SetActive(false);
                CruchFlakesBox.SetActive(true);
                break;
            case 4:
                CruchFlakesBox.SetActive(false);
                FootBallHelmet.SetActive(true);
                break;
            case 5:
                FootBallHelmet.SetActive(false);
                RubicksCube.SetActive(true);
                break;
            case 6:
                RubicksCube.SetActive(false);
                TreeTrunk.SetActive(true);
                break;
            case 7:
                TreeTrunk.SetActive(false);
                StarPlatinum.SetActive(true);
                break;
            case 8:
                StarPlatinum.SetActive(false);
                Bear.SetActive(true);
                break;
            case 9:
                Bear.SetActive(false);
                MilleniumFalcon.SetActive(true);
                break;
            case 10:
                MilleniumFalcon.SetActive(false);
                CartoonMill.SetActive(true);
                nSwitch = 0;
                break;

        }
            /*
            public GameObject TreeTrunk;
            public GameObject StarPlatinum;
             public GameObject Bear;
            public GameObject MilleniumFalcon;
            public GameObject CartoonMill;
            */
}


}
