using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

using UnityEngine;

using NetMQ;
using NetMQ.Sockets;

public class ClientSocketScript
{
    private NetMqListener _netMqListener;
    private string response;
    private void HandleMessage(string message)
    {
        setResponse(message);
    }

    public ClientSocketScript(string pic64, string message)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64, message);
    }
    
    public ClientSocketScript(string pic64, string message, int brushStrokeIndex, float[] brushValues)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64, message, brushStrokeIndex, brushValues);
    }

    public ClientSocketScript(string pic64, string message, string ipAdress)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64, message, ipAdress);
    }
    
    public ClientSocketScript(string pic64, string message, string ipAdress, int brushStrokeIndex, float[] brushValues)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64, message, ipAdress, brushStrokeIndex, brushValues);
    }

    public void Start()
    {
        _netMqListener.Start();
    }

    public void Update()
    {
        _netMqListener.Update();
    }

    public void Stop()
    {
        _netMqListener.Stop();
    }

    public string getResponse()
    {
        return response;
    }

    public void setResponse(string response)
    {
        this.response = response;
    }
}

public class NetMqListener
{
    private readonly Thread _listenerWorker;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private string _picture;

    private string _ipAdress;

    private string _message;

    private string _brushStrokeIndex;

    private string _brushValues;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var reqSocket = new RequestSocket())
        {
            string message = "";

            if (_brushStrokeIndex == "")
                message = _message + "," + _picture;
            
            else
                message = _message + "," + _picture + "," + _brushStrokeIndex + ',' + _brushValues;
            
            reqSocket.Connect("tcp://localhost:12345");
            reqSocket.SendFrame(message);
            
            string frameString = "";

            while (frameString == null || frameString.Equals(""))
            {
                if (!reqSocket.TryReceiveFrameString(out frameString))
                {

                }
                _messageQueue.Enqueue(frameString);
                _messageDelegate(frameString);
            }
            reqSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate, string picture, string message)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
        _message = message;
        _brushStrokeIndex = "";
    }
    
    public NetMqListener(MessageDelegate messageDelegate, string picture, string message, string ipAdress)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
        _message = message;
        _ipAdress = ipAdress;
        _brushStrokeIndex = "";
    }
    
    public NetMqListener(MessageDelegate messageDelegate, string picture, string message, int brushStrokeIndex, float[] brushValues)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
        _message = message;
        _brushStrokeIndex = brushStrokeIndex.ToString();
        _brushValues = "";
        for(int i = 0; i < brushValues.Length; i++)
        {
            if (i < 5)
                _brushValues += brushValues[i].ToString() + ",";
            else
                _brushValues += brushValues[i].ToString();
        }
    }
    
    public NetMqListener(MessageDelegate messageDelegate, string picture, string message, string ipAdress, int brushStrokeIndex, float[] brushValues)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
        _message = message;
        _ipAdress = ipAdress;
        _brushStrokeIndex = brushStrokeIndex.ToString();
        _brushValues = "";
        for (int i = 0; i < brushValues.Length; i++)
        {
            if (i < 5)
                _brushValues += brushValues[i].ToString() + ",";
            else
                _brushValues += brushValues[i].ToString();
        }
    }

    public void Start()
    {
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerWorker.Join();
    }
}
