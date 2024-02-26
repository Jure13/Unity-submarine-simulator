using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;
using System.Text;

public class HorT200Move : MonoBehaviour
{
    private Rigidbody rb;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public float TempCurSpeed { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        udpClient = new UdpClient(1234);
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
            //Debug.Log("Received UDP bytes: " + BitConverter.ToString(bytes));

            if (bytes.Length == 4)
            {
                //float curSpeed = BitConverter.ToSingle(bytes, 0);
                //Debug.Log("Received UDP: " + curSpeed);

                TempCurSpeed = BitConverter.ToSingle(bytes, 0);
                //Debug.Log("Received UDP: " + TempCurSpeed);

                //MainThreadDispatcher.Enqueue(() => rb.AddForce(transform.forward * curSpeed));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP greska horizontalnog motora: {e.Message}");
        }
        finally
        {
            StartUDPListener();
        }
    }
}
