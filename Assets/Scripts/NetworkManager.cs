using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManager : NetworkBehaviour
{
    public bool isAtStartup = true;
    NetworkClient myClient;
    public Button Host;

    private string m_URL = "http://127.0.0.1/";
    private string m_LocalFileName = "C:/Users/Jordan/Desktop/chika pouting.png";

    void Update()
    {
        
    }
    void OnGUI()
    {
        if (isAtStartup)
        {
            //GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
            //GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");
            //GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
            /*if (GUI.Button(new Rect(10, 10, 150, 100), "I am a button"))
            {
                SetupServer();
                print("You clicked the button!");
            }*/
        }
    }

    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Listen(4444);
        isAtStartup = false;
        print("Server is Online");
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        //myClient.Connect("127.0.0.1", 4444);
        myClient.Connect("127.0.0.1", 4444);
        isAtStartup = false;
    }

    // Create a local client and connect to the local server
    public void SetupLocalClient()
    {
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        isAtStartup = false;
    }

    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }

    void OnMouseClickHost()
    {
        print("You HOST the button!");
    }
}
