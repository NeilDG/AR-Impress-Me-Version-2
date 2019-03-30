//https://docs.unity3d.com/Manual/UnityWebRequest-RetrievingTextBinaryData.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class FileRetriever : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetFile(string localFileName, string uploadURL)
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://192.168.178.29/");
        //UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1/");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }

    /*
    public Image byteArrayToImage(byte[] byteArrayIn)
    {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        Image returnImage = Image.FromStream(ms);
        return returnImage;
    }
    */
}
