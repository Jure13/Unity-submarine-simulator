using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;

public class UDPComm3 : MonoBehaviour
{
    public int port = 9101;
    public string targetAddress = "127.0.0.1";
    private UdpClient udpClient;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
    }

    public void SendUDP(float curSpeed)
    {
        byte[] data = BitConverter.GetBytes(curSpeed);
        udpClient.Send(data, data.Length, targetAddress, port);
    }
}
