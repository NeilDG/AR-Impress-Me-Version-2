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

    public ClientSocketScript(string pic64, string message, string ipAdress)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64, message, ipAdress);
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

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var reqSocket = new RequestSocket())
        {
            string message = _message + "," + _picture;
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
    }
    
    public NetMqListener(MessageDelegate messageDelegate, string picture, string message, string ipAdress)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
        _message = message;
        _ipAdress = ipAdress;
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
