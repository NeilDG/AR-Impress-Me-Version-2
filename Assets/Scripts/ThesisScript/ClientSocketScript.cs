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
    private string picture;
    private void HandleMessage(string message)
    {
        this.picture = message;
    }

    public ClientSocketScript(string pic64)
    {
        _netMqListener = new NetMqListener(HandleMessage, pic64);
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
    
    public string getPicture()
    {
        return picture;
    }

    public void setPicture(string picture)
    {
        this.picture = picture; 
    }

}

public class NetMqListener
{
    private readonly Thread _listenerWorker;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private string _picture;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var reqSocket = new RequestSocket())
        {
            reqSocket.Connect("tcp://localhost:12345");
            reqSocket.SendFrame(_picture);
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

    public NetMqListener(MessageDelegate messageDelegate, string picture)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _picture = picture;
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
