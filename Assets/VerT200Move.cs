using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System;

public class VerT200Move : MonoBehaviour
{
    private Rigidbody rb;
    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    public float TempCurSpeed { get; private set; } // Property za dohvat trenutne brzine

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Podmornica
        udpClient = new UdpClient(9101); // Port iz UDPComm
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // "endpoint" za za primanje naredbi UDP protokolom, sintaksa
        StartUDPListener(); // Trazenje UDP poruka
    }

    void StartUDPListener()
    {
        // Asinkrono primanje podataka, sintaksa
        udpClient.BeginReceive(new AsyncCallback(ReceivedUDP), null);
    }

    // "Callback" funkcija koja se pozove svaki put kada se primi nova poruka UDP protokolom, sintaksa
    void ReceivedUDP(IAsyncResult temp)
    {
        try
        {
            byte[] bytes = udpClient.EndReceive(temp, ref remoteEndPoint); // spremi dohvacene podatke

            if (bytes.Length == 4) // vrijednost je 4 jer treba biti tipa "float"
            {
                //Debug.log("Bajtovi: " + bytes);
                TempCurSpeed = BitConverter.ToSingle(bytes, 0); // Pretvori u "float"
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP greska vertikalnog motora : {e.Message}");
        }
        finally
        {
            StartUDPListener();
        }
    }
}
