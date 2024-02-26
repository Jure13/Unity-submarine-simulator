using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class TemperatureSensorScript : MonoBehaviour
{
    public Text temperatureText;

    private Socket tempUdpSocket;
    private IPEndPoint tempServerEndPoint;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSocket();
    }

    // Update is called once per frame
    void Update()
    {
        float currentEnvironmentTemperature = ReadFromPixhawkSensor();
        // float currentEnvironmentTemperature = EnvironmentManagerScript.currentTemperature;

        UpdateUIText(currentEnvironmentTemperature);
    }

    void UpdateUIText(float temperature)
    {
        temperatureText.text = "Temperatura: " + temperature.ToString("F2") + " °C";
    }

    // Koristi se prilikom automatizirane misije
    public void ChangeTextColor(Color newColor, float duration, Color finalColor)
    {
        StartCoroutine(ChangeTextColorCoroutine(newColor, duration, finalColor));
    }

    // Coroutine za mijenjane boje, sintaksa Unityja
    IEnumerator ChangeTextColorCoroutine(Color newColor, float duration, Color finalColor)
    {
        Color originalColor = temperatureText.color;
        temperatureText.color = newColor;
        yield return new WaitForSeconds(duration); // sintaksa za cekanje
        temperatureText.color = finalColor;
    }

    private void InitializeSocket()
    {
        tempUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        tempServerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.100"), 5000);

        try
        {
            tempUdpSocket.Connect(tempServerEndPoint);
            SendTemperatureData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska pri spajanju na server (temperatura): {e.Message}");
        }
    }

    private void SendTemperatureData()
    {
        float currentTemperature = ReadFromPixhawkSensor();

        try
        {
            
            byte[] temperatureBytes = BitConverter.GetBytes(currentTemperature);

            
            tempUdpSocket.Send(temperatureBytes);

            //Debug.Log($"Temperature data sent: {currentTemperature}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska kod slanja temperature: {e.Message}");
        }
    }

    private float ReadFromPixhawkSensor()
    {
        string filePath = "Assets/GenData/TemperatureData.txt";
        // kod Pixhawka, putanja bi bila /sys/bus/w1/devices/28-0000027a4334/w1_slave

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string temperatureStr = file.ReadLine();

                if (temperatureStr != null)
                {
                    if (float.TryParse(temperatureStr, out float temperature))
                    {
                        return temperature;
                    }
                }
            }

            Debug.LogError("Los format datoteke (temperatura).");
            return 0f;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ne mogu se citati podatci senzora (temperatura): {e.Message}");
            return 0f;
        }
    }

}
