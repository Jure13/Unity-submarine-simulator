using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;

public class PowerModuleScript : MonoBehaviour
{

    private UdpClient powerUdpClient;
    private IPEndPoint powerServerEndPoint;

    public Text powerText;

    // Start is called before the first frame update
    void Start()
    {
        InitializeModule();
    }

    // Update is called once per frame
    void Update()
    {
        //float currentVoltage = EnvironmentManagerScript.currentVoltage;
        //float currentCurrent = EnvironmentManagerScript.currentCurrent;

        float currentVoltage = ReadVoltageFromPixhawk();
        float currentCurrent = ReadCurrentFromPixhawk();

        UpdateUIText(currentVoltage, currentCurrent);
    }

    void UpdateUIText(float voltage, float current)
    {
        powerText.text = "Napon: " + voltage.ToString("F2") + " V " + "Struja: " + current.ToString("F2") + " A";
    }


    public void InitializeModule()
    {
        powerUdpClient = new UdpClient();
        powerServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);

        try
        {
            powerUdpClient.Connect(powerServerEndPoint);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greksa pri spajanju na server (energija): {e.Message}");
            Environment.Exit(1);
        }
    }

    public void ClosePowerModule()
    {
        powerUdpClient.Close();
    }

    private float ReadVoltageFromPixhawk()
    {
        string filePath = "Assets/GenData/VData.txt";

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string voltageStr = file.ReadLine();

                if (voltageStr != null)
                {
                    if (float.TryParse(voltageStr, out float voltage))
                    {
                        return voltage;
                    }
                }
            }

            Console.WriteLine($"Los format (napon): {filePath}");
            Environment.Exit(1);
            return 0.0f;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ne cita napon: {e.Message}");
            Environment.Exit(1);
            return 0.0f;
        }
    }

    private float ReadCurrentFromPixhawk()
    {
        string filePath = "Assets/GenData/CData.txt";

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string currentStr = file.ReadLine();

                if (currentStr != null)
                {
                    if (float.TryParse(currentStr, out float current))
                    {
                        return current;
                    }
                }
            }

            Console.WriteLine($"Los format (struja): {filePath}");
            Environment.Exit(1);
            return 0.0f;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ne cita struju: {e.Message}");
            Environment.Exit(1);
            return 0.0f;
        }
    }

}
