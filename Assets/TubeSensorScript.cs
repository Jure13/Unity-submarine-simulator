using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class TubeSensorScript : MonoBehaviour
{
    private UdpClient tubeUdpClient;
    private IPEndPoint tubeDestAddr;
    private const int DHT_PIN = 12;

    // UI za prikaz podataka
    public Text tempHumText;

    // Start is called before the first frame update
    void Start()
    {
        // Inicijalizacija klijenta (senzora)
        InitializeSocket();
    }

    // Update is called once per frame
    void Update()
    {
        // Citanje podataka iz okoline
        float currentTubeTemperature = ReadTemperaturePixhawk();
        float currentTubeHumidity = ReadHumidityPixhawk();

        // Azuriraj UI (prikaz na sucelju)
        UpdateUIText(currentTubeTemperature, currentTubeHumidity);
    }

    // Funkcija za azuriranje UI-a
    void UpdateUIText(float temperature, float humidity)
    {
        // Ono sto ce se prikazati
        tempHumText.text = "Temperatura cijevi: " + temperature.ToString("F2") + " °C" +
                           " Vlažnost cijevi: " + humidity.ToString("F2") + " %";
    }

    // Inicijalizacija
    public void InitializeSocket()
    {
        tubeUdpClient = new UdpClient();

        // Lokalna adresa, subjektivni port, sintaksa
        tubeDestAddr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);

        try
        {
            // Funkcija za spajanje na odredjenu adresu
            tubeUdpClient.Connect(tubeDestAddr);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Greska pri spajanju na server (senzor cijevi): {e.Message}");
            Environment.Exit(1);
        }
    }

    private float ReadTemperaturePixhawk()
    {
        // Putanja na kojoj su spremljeni podatci - simulacija putanje drivera na Pixhawku
        string filePath = "Assets/GenData/TubeTemp.txt";

        try
        {
            // Citaj iz datoteke, sintaksa
            using (StreamReader file = new StreamReader(filePath))
            {
                string temperatureStr = file.ReadLine();

                // Dodatna provjera
                if (temperatureStr != null)
                {
                    if (float.TryParse(temperatureStr, out float temperature))
                    {
                        return temperature;
                    }
                }
            }

            Debug.LogError("Krivi format u datoteci (temperatura cijevi).");
            return 0f;
        }
        catch (Exception e)
        {
            Debug.LogError($"Lose citanje iz datoteke (temperature cijevi): {e.Message}");
            return 0f;
        }
    }

    // Sve isto, vlaznost cijevi
    private float ReadHumidityPixhawk()
    {
        string filePath = "Assets/GenData/TubeHum.txt";

        try
        {
            using (StreamReader file = new StreamReader(filePath))
            {
                string humStr = file.ReadLine();

                if (humStr != null)
                {
                    if (float.TryParse(humStr, out float humidity))
                    {
                        return humidity;
                    }
                }
            }

            Debug.LogError("Krivi format u datoteci (vlaznost cijevi).");
            return 0f;
        }
        catch (Exception e)
        {
            // Log an error if reading from the humidity sensor fails
            Debug.LogError($"Lose citanje iz datoteke (vlaznost cijevi): {e.Message}");
            return 0f;
        }
    }
}
