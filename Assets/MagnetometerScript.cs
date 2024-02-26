using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MagnetometerScript : MonoBehaviour
{
    public Text magnetText;

    private const int AF_INET = 2;
    private const int SOCK_DGRAM = 2;
    //private int magUdpSocket;
    private Socket magUdpSocket;
    private IPEndPoint magDestAddr;
    private string bus = "/dev/i2c-1";
    private int magI2CFile;
    private const int I2C_SLAVE = 0x0703;

    // C# pozivi "libc" biblioteka (niska razina programiranja, C)
    [DllImport("libc", SetLastError = true)]
    private static extern int socket(int domain, int type, int protocol); // endpoint komunikacije

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd); // zatvori datoteku

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int d, int request, int arg); // upravljacke naredbe

    [DllImport("libc", SetLastError = true)]
    private static extern int write(int fd, byte[] buf, int count); // pisi u datoteku

    [DllImport("libc", SetLastError = true)]
    private static extern int read(int fd, byte[] buf, int count); // citaj iz datoteke

    [DllImport("libc", SetLastError = true)]
    private static extern int open(string path, int flags); // otvori datoteku

    // Start is called before the first frame update
    void Start()
    {
        InitializeMagnetometer();
    }

    // Update is called once per frame
    void Update()
    {
        //float currentX = EnvironmentManagerScript.currentMagnetometerX;
        //float currentY = EnvironmentManagerScript.currentMagnetometerY;
        //float currentZ = EnvironmentManagerScript.currentMagnetometerZ;

        float currentX = ReadXFromPixhawk();
        float currentY = ReadYFromPixhawk();

        UpdateUIText(currentX, currentY);
    }

    void UpdateUIText(float currentX, float currentY)
    {
        magnetText.text = "Magnetno polje X: " + currentX.ToString("F2") + " Y:" + currentY.ToString("F2");
    }

    public void InitializeMagnetometer()
    {
        /*if ((magUdpSocket = socket(AF_INET, SOCK_DGRAM, 0)) < 0)
        {
            Console.WriteLine("Initialization error!");
            Environment.Exit(1);
        }*/

        magUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        magDestAddr = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 5000);

        //magDestAddr = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 5000);

        string bus = "/dev/i2c-1"; // Provjeri, ovisi o uređaju
        magI2CFile = open(bus, 2);
        if (magI2CFile < 0)
        {
            throw new System.IO.IOException("Ne moze se stvoriti datoteka (magnetometar)!");
        }

        if (ioctl(magI2CFile, I2C_SLAVE, 0x1E) < 0)
        {
            Console.WriteLine("Losa adresa!");
            Environment.Exit(1);
        }

        byte[] config = { 0x00, 0x60 };
        if (write(magI2CFile, config, 2) != 2)
        {
            Console.WriteLine("I2C greska!");
            Environment.Exit(1);
        }

        config[0] = 0x02;
        config[1] = 0x00;
        // konfiguracijski podatci I2C datoteke
        if (write(magI2CFile, config, 2) != 2)
        {
            Console.WriteLine("Greska pisanja!");
            Environment.Exit(1);
        }
    }


    private void CloseSockets()
    {
        //close(magUdpSocket);
        close(magI2CFile);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int sendto(int socket, string message, int length, int flags, IPEndPoint dest_addr, int dest_len);
    // slanje podataka preko socketa na adresu

    private bool SendToUdp(string sensorData)
    {
        // return sendto(magUdpSocket, sensorData, sensorData.Length, 0, magDestAddr, magDestAddr.AddressFamily);
        byte[] data = Encoding.ASCII.GetBytes(sensorData);
        magUdpSocket.SendTo(data, magDestAddr);
        return true;
    }

    private float ReadXFromPixhawk()
    {
        string filePath = "Assets/GenData/MagXData.txt";

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string xMagStr = file.ReadLine();

                if (xMagStr != null)
                {
                    if (float.TryParse(xMagStr, out float xMag))
                    {
                        return xMag;
                    }
                }
            }

            Console.WriteLine($"Los format (mag x): {filePath}");
            Environment.Exit(1);
            return 0f;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ne moze citati koordinatu x: {e.Message}");
            Environment.Exit(1);
            return 0f;
        }
    }

    private float ReadYFromPixhawk()
    {
        string filePath = "Assets/GenData/MagYData.txt";

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string yMagStr = file.ReadLine();

                if (yMagStr != null)
                {
                    if (float.TryParse(yMagStr, out float yMag))
                    {
                        return yMag;
                    }
                }
            }

            Console.WriteLine($"Los format (mag y): {filePath}");
            Environment.Exit(1);
            return 0f;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ne moze citati koordinatu y: {e.Message}");
            Environment.Exit(1);
            return 0f;
        }
    }
}
