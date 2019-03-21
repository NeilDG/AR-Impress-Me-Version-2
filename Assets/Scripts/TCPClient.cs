using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
   #region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread;
    #endregion
    // Use this for initialization 	

    Texture2D screenCap;
    Texture2D border;
    bool shot = false;

    Texture2D recieved_image;
    byte[] byte_image;

    void Start () {
		ConnectToTcpServer();
        recieved_image = new Texture2D(200, 200, TextureFormat.RGBA32, false);
    }  	
	// Update is called once per frame
	void Update () {         
		if (Input.GetKeyDown(KeyCode.Space)) {             
			//SendMessage();         
		}     
	}  	
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForDataImage)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();
            Debug.Log("Connected to Server!!");
		} 		
		catch (Exception e) { 			
			Debug.Log("On client connect exception " + e); 		
		} 	
	}  	
	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     
	private void ListenForData() { 		
		try { 			
			socketConnection = new TcpClient("localhost", 8000);  			
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("server message received as: " + serverMessage); 					
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}

    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForDataImage()
    {
        try
        {
            socketConnection = new TcpClient("localhost", 45555);
            Byte[] bytes = new Byte[1024];
            byte_image = new Byte[0];
            while (true)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        //Debug.Log("server message received as: " + serverMessage);

                        var s = new MemoryStream();
                        s.Write(byte_image, 0, byte_image.Length);
                        s.Write(bytes, 0, bytes.Length);
                        byte_image = s.ToArray();
                        //Debug.Log("Image byte size: " + byte_image.Length);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
 
    /// <summary> 	
    /// Send message to server using socket connection. 	
    /// </summary> 	
    public void SendMessage() {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = "This is a message from one of your clients."; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);

                // Write byte array to socketConnection stream.  
                byte[] image_bytes = null;
                //image_bytes = ScreenshotHandler.TakeScreenshot_Static(Screen.width, Screen.width);

                //PaintScene prntscrn = new PaintScene();
                //prntscrn.Paint();
                //byte[] image_bytes = prntscrn.SendBytes();
                Texture2D print_screen = ScreenCapture.CaptureScreenshotAsTexture();

                image_bytes = print_screen.EncodeToPNG();

                if (image_bytes == null)
                {
                    Debug.Log("FUCK");
                }

                Debug.Log("INITIAL BYTE SIZE: " + image_bytes.Length);

                string image_byte_debug = Encoding.ASCII.GetString(image_bytes);
                Debug.Log("IMAGE BYTE FORMAT(CLIENT): " + image_byte_debug);

                stream.Write(image_bytes, 0, image_bytes.Length);                 
				Debug.Log("Client sent his message - should be received by server");
                byte_image = new Byte[0];
            }         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 

    private void bytestoImage()
    {
        Debug.Log("Image byte size: " + byte_image.Length);
        recieved_image.LoadImage(byte_image);
    }

    public void saveRecievedImage()
    {
        Debug.Log("Image byte size: " + byte_image.Length);
        bytestoImage();
        byte[] _bytes = recieved_image.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/../Rendered_Image.png", _bytes);
        Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + Application.dataPath);
    }
}
