using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.IO;

public class PressureSensorScript : MonoBehaviour
{
    private Socket pressureUdpSocket;
    private IPEndPoint pressureDestAddr;
    private const string GPIO_CHIP_PATH = "/dev/gpiochip0";
    private const int PRESSURE_PIN = 17;

    public Text pressureText;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    // Update is called once per frame
    void Update()
    {
        float currentEnvironmentPressure = ReadFromPixhawkSensor();
        //float currentEnvironmentPressure = EnvironmentManagerScript.currentPressure;

        UpdateUIText(currentEnvironmentPressure);
    }

    void UpdateUIText(float pressure)
    {
        pressureText.text = "Pritisak: " + pressure.ToString("F2") + " kPa";
    }

    private void InitializeSocket()
    {
        if (pressureUdpSocket != null)
        {
            Debug.LogError("Inicijalizacijska greska (pritisak)!");
            return;
        }

        pressureDestAddr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);

        try
        {
            pressureUdpSocket.Connect(pressureDestAddr);
            SendPressureData();
        }
        catch (Exception e)
        {
            //Debug.LogError($"Server unreachable: {e.Message}");
            float error = 0;
        }
    }

    private float ReadFromPixhawkSensor()
    {
        string filePath = "Assets/GenData/PressureData.txt";
        //Za pixhawk, using (var chip = new GpiodChip(GPIO_CHIP_PATH))

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string pressureStr = file.ReadLine();

                // Dodatna provjera
                if (pressureStr != null)
                {
                    if (float.TryParse(pressureStr, out float pressure))
                    {
                        return pressure;
                    }
                }
            }

            Debug.LogError("Ne valja format datoteke (pritisak).");
            return 0f;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ne mogu se dohvatiti podatci (pritisak): {e.Message}");
            return 0f;
        }    
    }

    private void SendPressureData()
    {
        float pressureData = ReadFromPixhawkSensor();

        try
        {
            byte[] pressureBytes = BitConverter.GetBytes(pressureData);
            pressureUdpSocket.Send(pressureBytes);
            // Debug.Log($"Pritisak poslan: {pressureData}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska u slanju podataka (pritisak): {e.Message}");
        }
    }
}
