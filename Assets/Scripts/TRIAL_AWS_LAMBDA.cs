using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TRIAL_AWS_LAMBDA : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
        Trial1();

    }

    // Update is called once per frame
    void Update()
    {
      
    }

    private void Trial1()
    {
        Debug.Log("ENTERED LAMBDA");
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
         "us-east-2:f0200bb0-1b84-48fb-8809-5bcd1037dcb5", // Identity pool ID
             RegionEndpoint.USEast2 // Region
           );
        //var credentials = new CognitoAWSCredentials("arn:aws:lambda:us-east-2:884293217470:function:Sample-Thesis-ML", RegionEndpoint.USEast2);
        var Client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast2);

        Debug.Log("1");
        var request = new InvokeRequest()
        {
            FunctionName = "Sample-Thesis-ML",
            Payload = "{\"key1\" : \"Hello World!\"}",
            InvocationType = InvocationType.RequestResponse
        };
        
        Debug.Log("2");
        Client.InvokeAsync(request, (result) =>
        {
            Debug.Log("3");
            if (result.Exception == null)
            {
                Debug.Log("4");
                Debug.Log(Encoding.ASCII.GetString(result.Response.Payload.ToArray()));
            }
            else
            {
                Debug.Log("5");
                Debug.LogError(result.Exception);
            }
        });
        Debug.Log("5");
    }
}
