using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AWSFileHandler : MonoBehaviour
{
    private static string IDENTITY_POOL_ID = "us-east-1:7848746b-3415-470e-b86f-357326351577";
    AmazonS3Client Client;
    string S3BucketName;
    string SampleFileName;

    public void Start()
    {
        S3BucketName = "impress-me-bucket";
        SampleFileName = "dahyun.png";

        UnityInitializer.AttachToGameObject(this.gameObject);

        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        CognitoAWSCredentials credentials = new CognitoAWSCredentials(IDENTITY_POOL_ID, RegionEndpoint.USEast1);
        Client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PostObject2(string fileName)
    {

        string ResultText = "";
        string txt = "testtest";
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + fileName, false);

        sw.WriteLine(txt);
        sw.Flush();
        sw.Close();


        string allText1 = File.ReadAllText(Application.persistentDataPath + fileName);
        ResultText += allText1;


        var stream = new FileStream(Application.persistentDataPath + fileName,
        FileMode.Open, FileAccess.Read, FileShare.Read);
        ResultText += Application.persistentDataPath + fileName;

        var request = new PutObjectRequest();
        request.BucketName = "bucket-test";
        request.Key = "input-data" + fileName;
        request.InputStream = stream;
        request.CannedACL = S3CannedACL.Private;

        Client.PutObjectAsync(request, (responseObj) => {

            if (responseObj.Exception == null)
            {
                Debug.Log("SUCCESS");
                ResultText = "success";
            }
            else
            {
                Debug.LogError("ERROR");
                ResultText += responseObj.Exception.ToString();
            }
        });
    }

    public void PostObject(string fileName)
    {
        string ResultText;
        ResultText = "Retrieving the file";

        //Debug.Log(Application.persistentDataPath + Path.DirectorySeparatorChar + fileName);

        FileStream stream = new FileStream(Application.dataPath +
        Path.DirectorySeparatorChar + fileName,
        FileMode.Open, FileAccess.Read, FileShare.Read);

        Debug.Log("WE HERE");

        ResultText += "\nCreating request object";
        PostObjectRequest request = new PostObjectRequest()
        {
            Bucket = S3BucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private
        };

        ResultText += "\nMaking HTTP post call";

        Client.PostObjectAsync(request, (responseObj) =>
        {
            if (responseObj.Exception == null)
            {
                ResultText += string.Format("\nobject {0} posted to bucket {1}",
                responseObj.Request.Key, responseObj.Request.Bucket);
            }
            else
            {
                ResultText += "\nException while posting the result object";
                ResultText += string.Format("\n receieved error {0}",
                responseObj.Response.HttpStatusCode.ToString());
            }
        });
    }

    public void ListBucket()
    {
        string ResultText;
        // ResultText is a label used for displaying status information
        ResultText = "Fetching all the Buckets";
        Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
        {
            ResultText += "\n";
            if (responseObject.Exception == null)
            {
                ResultText += "Got Response \nPrinting now \n";
                responseObject.Response.Buckets.ForEach((s3b) =>
                {
                    ResultText += string.Format("bucket = {0}, created date = {1} \n",
                    s3b.BucketName, s3b.CreationDate);
                });
            }
            else
            {
                ResultText += "Got Exception \n";
            }
        });
    } 

    public void ListObjects()
    {
        string ResultText;

        ResultText = "Fetching all the Objects from " + S3BucketName;

        var request = new ListObjectsRequest()
        {
            BucketName = S3BucketName
        };

        Client.ListObjectsAsync(request, (responseObject) =>
        {
            ResultText += "\n";
            if (responseObject.Exception == null)
            {
                ResultText += "Got Response \nPrinting now \n";
                responseObject.Response.S3Objects.ForEach((o) =>
                {
                    ResultText += string.Format("{0}\n", o.Key);
                });
            }
            else
            {
                ResultText += "Got Exception \n";
            }
        });
    }

    private void GetObject()
    {
        string ResultText;
        ResultText = string.Format("fetching {0} from bucket {1}",
        SampleFileName, S3BucketName);
        Client.GetObjectAsync(S3BucketName, SampleFileName, (responseObj) =>
        {
            string data = null;
            var response = responseObj.Response;
            if (response.ResponseStream != null)
            {
                using (StreamReader reader = new StreamReader(response.ResponseStream))
                {
                    data = reader.ReadToEnd();
                }

                ResultText += "\n";
                ResultText += data;
            }
        });
    }
}
