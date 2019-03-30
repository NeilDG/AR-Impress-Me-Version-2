using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private Thread tcpListenerThread;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;
    #endregion

    int counter = 0;

    Texture2D image;
    byte[] byte_image;

    // Use this for initialization
    void Start()
    {
        // Start TcpServer background thread 		
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequestsImage));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
        Debug.Log("SERVER GO!!!");
    }   

    /*
	// Update is called once per frame
	void Update () { 		
		if (Input.GetKeyDown(KeyCode.Space)) {             
			SendMessage();         
		} 	
	}
	*/

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 			
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            tcpListener.Start();
            Debug.Log("Server is listening");
            Byte[] bytes = new Byte[1024];
            byte_image = new Byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to JSON message. 							
                            String clientMessage = Encoding.UTF8.GetString(incommingData);

                            // Added: convert string to JSON
                            JSONObject clientMessage_json = new JSONObject(clientMessage);
                            Debug.Log("client message received as: " + clientMessage_json);
                            //RequestBlendshapes(clientMessage_json);
                        }
                        Debug.Log("DONE READING");
                    }
             
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    private void ListenForIncommingRequestsImage()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            tcpListener.Start();
            Debug.Log("Server is listening");
            byte[] bytes = new Byte[1024];
            byte_image = new Byte[0];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            //Loader = incommingData;
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            //Debug.Log("client message received as: " + clientMessage);
                            Debug.Log("LENGTH: " + length);


                            var s = new MemoryStream();
                            s.Write(byte_image, 0, byte_image.Length);
                            s.Write(bytes, 0, bytes.Length);
                            byte_image = s.ToArray();
                            //Debug.Log("LENGTH: " + byte_image.Length);
                        }
                        Debug.Log("1");
                    }
                    Debug.Log("2");
                }
                Debug.Log("3");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }

        //image.LoadRawTextureData(bytes);
        //image.Apply();
        //GameObject.Find("Plane").GetComponent<Renderer>().material.mainTexture = image;
        //Debug.Log("DONE");
    }

    public void SendImage()
    {
        if (connectedTcpClient == null)
        {
            return;
        }


        if (byte_image == null)
        {
            Debug.Log("I LOVE U");
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                // Added: create dict to be a JSON object
                Dictionary<string, string> serverMessage = new Dictionary<string, string>();

                if(image != null)
                {
                    Debug.Log("Image Exists");
                }
                
                string image_string = System.Text.Encoding.UTF8.GetString(byte_image);

                Debug.Log(image_string);

                //serverMessage["unity"] = String.Format(image_string, counter);
                serverMessage["unity"] = String.Format("Unity sends its regards {0}", counter);

                string image_byte_debug = Encoding.ASCII.GetString(byte_image);
                Debug.Log("IMAGE BYTE FORMAT(SERVER): " + image_byte_debug);
                Debug.Log("Image byte size(SERVER): " + byte_image.Length);

                counter++;
                JSONObject serverMessage_json = new JSONObject(serverMessage);
                String serverMessage_string = serverMessage_json.ToString();
                //string serverMessage = "This is a message from your server.";  // original code			
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.UTF8.GetBytes(serverMessage_string);  // serverMessage				
                                                                                                 // Write byte array to socketConnection stream.               
                //stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                stream.Write(byte_image, 0, byte_image.Length);
                Debug.Log("Server sent his message - should be received by client");
                byte_image = new Byte[0];
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    private void SendMessage()
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                // Added: create dict to be a JSON object
                Dictionary<string, string> serverMessage = new Dictionary<string, string>();
                serverMessage["unity"] = String.Format("Unity sends its regards {0}", counter);
                counter++;
                JSONObject serverMessage_json = new JSONObject(serverMessage);
                String serverMessage_string = serverMessage_json.ToString();
                //string serverMessage = "This is a message from your server.";  // original code			
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.UTF8.GetBytes(serverMessage_string);  // serverMessage				
                                                                                                 // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}
