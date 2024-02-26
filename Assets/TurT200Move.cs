using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;
using System.Text;


public class TurT200Move : MonoBehaviour
{
    private Rigidbody rb;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public float TempCurSpeed { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        udpClient = new UdpClient(5678);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        StartUDPListener();
    }

    void StartUDPListener()
    {
        udpClient.BeginReceive(new AsyncCallback(ReceivedUDP), null);
    }

    void ReceivedUDP(IAsyncResult temp)
    {
        try
        {
            byte[] bytes = udpClient.EndReceive(temp, ref remoteEndPoint);

            if (bytes.Length == 4)
            {

                TempCurSpeed = BitConverter.ToSingle(bytes, 0);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP greska motora za okretanje: {e.Message}");
        }
        finally
        {
            StartUDPListener();
        }
    }
}
