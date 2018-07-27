using UnityEngine;
using System.Collections;
using WebSocketSharp;

using System.Collections.Generic;


public class EasyTransfer : MonoBehaviour
{
    public string host;
    public byte deviceID;
    public string socketName;
    public bool sendActive;
    public bool receiveActive;

    private float oldTime; // Used to calculate data speed
    private int dataSizePerSec; // Used to calculate data speed

    private WebSocket socket;

    private bool isOpen;

    // COMMANDS
    // TODO: Ideally commands would be ported to somewhere else
	[System.Serializable]
	public class Commands
	{
        public static byte SetMultiServo = 0x30;
        public static byte SetSingleServo = 0x35;
	}
    
    // Use this for initialization
    void Start()
    {
        isOpen = false;
        oldTime = Time.time;
        dataSizePerSec = 0;

        WebSocket newSocket = new WebSocket("ws://" + host);

        newSocket.OnOpen += (sender, e) => {
            SocketOpen(sender, e);
        };
        newSocket.OnMessage += (sender, e) =>
        {
            SocketReceive(sender, e);
        };
        newSocket.OnClose += (sender, e) => {
            SocketClose(sender, e);
        };

        newSocket.Connect(); 

        socket = newSocket;
    }

    void SocketOpen(object sender, System.EventArgs e)
    {
        Debug.Log("Opened " + socketName);

        isOpen = true;
    }

    void SocketClose(object sender, CloseEventArgs e)
    {
        Debug.Log("Closed " + socketName);

        isOpen = false;
    }

    void SocketReceive(object sender, MessageEventArgs e)
    {
        Debug.Log(socketName + " received " + e.Data);
    }

    // TODO: Should return immediately if socket is not open but easier to debug and calc speed, change in production
    public void SocketSend(byte cmd, byte[] values)
    {
        int dataSize = values.Length + 5;

        byte[] data = new byte[dataSize];
        data[0] = 0x06;
        data[1] = 0x85;
        data[2] = deviceID;
        data[3] = cmd;

        byte cs = (byte)values.Length;
        cs ^= deviceID;
        cs ^= cmd;

        for (int i = 0; i < values.Length; i++) {
            data[i+4] = values[i];
            cs ^= values[i];
        }

        data[data.Length-1] = cs;
        
        if (isOpen && sendActive) {
            socket.Send(data);
        }

        calcBytesPerSec(data.Length);

        Lebug.Log("Last Command", System.BitConverter.ToString(data), "Socket"); // Debug
    }

    //////////////
    // DEBUG
    //////////////

    void calcBytesPerSec(int dataLen)
    {
        dataSizePerSec += dataLen;

        if (Time.time > oldTime) {
            // ~1 sec has passed

            dataSizePerSec = 0;
            oldTime = Time.time;
        }

        Lebug.Log("Bytes per sec", dataSizePerSec, "Socket"); // Debug
    }

    //////////////
    // END DEBUG
    //////////////
}