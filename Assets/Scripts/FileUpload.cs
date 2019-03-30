// C# file names: "FileUpload.cs" https://answers.unity.com/questions/48686/uploading-photo-and-video-on-a-web-server.html
using UnityEngine;
using System.Collections;


public class FileUpload : MonoBehaviour
{
    //private string m_LocalFileName = "C:/boot.ini";
    private string m_URL = "http://192.168.178.29/php/upload";
    private string m_LocalFileName = "C:/Users/Jordan/Desktop/chika pouting.png";
    //private string m_URL = "http://127.0.0.1/php/upload.php";


    IEnumerator UploadFileCo(string localFileName, string uploadURL)
    {
        print("1");
        WWW localFile = new WWW("file:///" + localFileName);
        print("2");
        yield return localFile;
        print("3");
        if (localFile.error == null)
        {
            Debug.Log("Loaded file successfully");
            print("Loaded file successfully");
        }
        else
        {
            Debug.Log("Open file error: " + localFile.error);
            print("Open file error: " + localFile.error);
            yield break; // stop the coroutine here
        }
        print("1");
        WWWForm postForm = new WWWForm();
        // version 1
        //postForm.AddBinaryData("theFile",localFile.bytes);
        // version 2
        postForm.AddBinaryData("theFile", localFile.bytes, localFileName, "text/plain");
        WWW upload = new WWW(uploadURL, postForm);

        yield return upload;
        if (upload.error == null)
        {
            Debug.Log("upload done :" + upload.text);
            print("upload done :" + upload.text);
        }
        else
            Debug.Log("Error during upload: " + upload.error);
    }

    void UploadFile(string localFileName, string uploadURL)
    {
        StartCoroutine(UploadFileCo(localFileName, uploadURL));
    }

    public void upload()
    {
        print("Upload Pressed!!!");
        UploadFileCo(m_LocalFileName, m_URL);
    }

   
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        m_LocalFileName = GUILayout.TextField(m_LocalFileName);
        m_URL = GUILayout.TextField(m_URL);
        if (GUILayout.Button("Upload"))
        {
            UploadFile(m_LocalFileName, m_URL);
        }
        GUILayout.EndArea();
    }
    

}