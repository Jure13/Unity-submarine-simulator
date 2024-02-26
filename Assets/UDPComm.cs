using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;

public class UDPComm : MonoBehaviour
{
    public int port = 1234; // Subjektivan odabir
    public string targetAddress = "127.0.0.1"; // lokalna adresa
    private UdpClient udpClient;

    // Start is called before the first frame update
    void Start()
    {
        udpClient = new UdpClient();
    }

    // Funkcija za slanje podataka preko UDP-a
    public void SendUDP(float curSpeed)
    {
        byte[] data = BitConverter.GetBytes(curSpeed); // ".Send" prima iskljucivo vrijednost tipa "byte"
        udpClient.Send(data, data.Length, targetAddress, port); // Slanje na predodredeno odrediste
    }

}
