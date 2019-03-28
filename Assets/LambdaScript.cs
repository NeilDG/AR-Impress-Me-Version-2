using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LambdaScript : MonoBehaviour
{
    private static string IDENTITY_POOL_ID = "us-east-1:7848746b-3415-470e-b86f-357326351577";

    // Start is called before the first frame update
    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);   

        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        CognitoAWSCredentials credentials = new CognitoAWSCredentials(IDENTITY_POOL_ID, RegionEndpoint.USEast1);
        AmazonLambdaClient Client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast1);

        InvokeRequest request = new InvokeRequest()
        {
            FunctionName = "ImpressMeTest",
            Payload = "{\"key1\" : \"Hello World!\"}",
            InvocationType = InvocationType.RequestResponse
        };

        Client.InvokeAsync(request, (result) =>
        {
            if (result.Exception == null)
            {
                Debug.Log(Encoding.ASCII.GetString(result.Response.Payload.ToArray()));
            }
            else
            {
                Debug.LogError(result.Exception);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
