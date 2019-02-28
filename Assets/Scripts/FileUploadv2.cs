using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileUploadv2 : MonoBehaviour
{

    //private string m_LocalFileName = "C:/boot.ini";
    private string m_URL = "http://192.168.178.29/";
    private string m_LocalFileName = "C:/Users/Jordan/Desktop/chika pouting.png";
    //private string m_URL = "http://127.0.0.1/";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Upload(string localFileName, string uploadURL)
    {
        byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        UnityWebRequest www = UnityWebRequest.Put(uploadURL, myData);

        //var uwr = new UnityWebRequest(uploadURL, UnityWebRequest.kHttpVerbPUT)
        //uwr.uploadHandler = new UploadHandlerFile(localFileName);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
    }
    

    /*
    IEnumerator Upload(string localFileName, string uploadURL)
    {
        byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
        UnityWebRequest www = UnityWebRequest.Put(uploadURL, myData);
        yield return www.Send();

        if (www.isError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
    }
    */
    

    void UploadFile(string localFileName, string uploadURL)
    {
        StartCoroutine(Upload(localFileName, uploadURL));
    }

    /*
    public byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        MemoryStream ms = new MemoryStream();
        imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }
    */

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        m_LocalFileName = GUILayout.TextField(m_LocalFileName);
        m_URL = GUILayout.TextField(m_URL);
        if (GUILayout.Button("Upload"))
        {
            print("Upload Pressed!!");
            UploadFile(m_LocalFileName, m_URL);
        }
        GUILayout.EndArea();
    }
    
}
