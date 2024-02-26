using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EnvironmentManagerScript : MonoBehaviour
{
    // Deklariranje varijabli
    public static float currentTemperature;
    public static float currentPressure;
    public static float currentTubeTemperature;
    public static float currentTubeHumidity;
    public static float currentMagnetometerX;
    public static float currentMagnetometerY;
    public static float currentVoltage;
    public static float currentCurrent;
    private float lastUpdateTime;

    public static float oldY;
    public Rigidbody submarineRb;
    public static float magX;
    public static float magY;
    public static float angle;
    private float previousMagX;

    // Start is called before the first frame update
    void Start()
    {
        // Inicijalizacija funkcija na pocetku programa
        UpdateTemperature();
        UpdatePressure();
        UpdateTubeTemperature();
        UpdateTubeHumidity();
        UpdateMagnetometer();
        UpdateEnergy();
        lastUpdateTime = Time.time; // Time.time vraca vrijeme koje je proslo od pocetka pokretanja simulacije
        //submarineRb = GameObject.Find("Submarine").GetComponent<Rigidbody>();
        oldY = submarineRb.transform.position.y;

        GameObject magneticNorthObject = GameObject.Find("Magnetic North");
        if (magneticNorthObject != null)
        {
            Transform magneticNorthTransform = magneticNorthObject.transform;
            angle = GetAngleTowardsMagneticNorth(magneticNorthTransform);
        }
        else
        {
            Debug.LogError("Ne moze pronaci objekt.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Azuriraj funkcije svake jedne sekunde
        if (Time.time - lastUpdateTime >= 1f) // Za koristenje stvarnog vremena, Time.realtimeSinceStartup
        {
            //UpdatePressure();
            UpdateTemperature();
            UpdateTubeTemperature();
            UpdateTubeHumidity();
            //UpdateMagnetometer();
            UpdateEnergy();
            lastUpdateTime = Time.time;
        }
    }

    // Azuriraj temperaturu okoline
    void UpdateTemperature()
    {
        // Subjektivan odabir vrijednosti temperature okoline
        currentTemperature = UnityEngine.Random.Range(5f, 25f);
        SaveTemperatureToFile(currentTemperature);
    }

    // Azuriraj pritisak okoline
    public bool UpdatePressure()
    {
        // Subjektivan odabir vrijednosti pritiska okoline
        //currentPressure = UnityEngine.Random.Range(10f, 11f);

        float newY = submarineRb.transform.position.y;

        if (Mathf.Abs(newY - oldY) > 0.5f)
        {
            currentPressure = UnityEngine.Random.Range(10f, 11f);
            oldY = newY;
            SavePressureToFile(currentPressure);
            return true;
        }

        return false;
    }

    // Azuriraj temperaturu senzora cijevi
    void UpdateTubeTemperature()
    {
        // Subjektivan odabir vrijednosti temperature senzora cijevi
        currentTubeTemperature = UnityEngine.Random.Range(25f, 40f);
        SaveTubeTempToFile(currentTubeTemperature);
    }

    // Azuriraj vlaznost senzora cijevi
    void UpdateTubeHumidity()
    {
        // Subjektivan odabir vrijednosti vlaznosti senzora cijevi
        currentTubeHumidity = UnityEngine.Random.Range(40f, 80f);
        SaveTubeHumToFile(currentTubeHumidity);
    }

    // Azuriraj magnetsko polje
    public int UpdateMagnetometer()
    {
        // Subjektivan odabir vrijednosti koodrinata magnetskog polja
        float newMagX = submarineRb.transform.position.x;
        float newMagY = newMagX * Mathf.Sin(angle * Mathf.Deg2Rad);

        if (Mathf.Abs(newMagX - magX) > 0.1f || Mathf.Abs(newMagY - magY) > 0.1f)
        {
            magX = newMagX;
            magY = newMagY;
            SaveMagXToFile(newMagX);
            SaveMagYToFile(newMagY);

            if ((newMagX - previousMagX) < -0.1f)
            {
                previousMagX = newMagX; // Update the previous value for the next comparison
                return -1;
            }
            previousMagX = newMagX;
            return 1;
        }
        return 0;
    }

    public float GetAngleTowardsMagneticNorth(Transform magneticNorth)
    {
        if (magneticNorth == null)
        {
            Debug.LogError("Nema sjevera!");
            return 0f;
        }

        Vector3 directionToNorth = magneticNorth.position - submarineRb.transform.position;
        float angle = Vector3.SignedAngle(Vector3.forward, directionToNorth, Vector3.up);

        angle = (angle + 360f) % 360f;

        return angle;
    }

    // Azuriraj energetske vrijednosti podmornice
    void UpdateEnergy()
    {
        // Subjektivan odabir razina napona i jakosti struje podmornice
        currentVoltage = UnityEngine.Random.Range(35.5f, 36.5f);
        currentCurrent = UnityEngine.Random.Range(5f, 15f);
        SaveVToFile(currentVoltage);
        SaveCToFile(currentCurrent);
    }

    // Temperaturni podatci se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveTemperatureToFile(float temperature)
    {
        string filePath = "Assets/GenData/TemperatureData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(temperature.ToString());
            }

            //Debug.Log("Temperature saved to file: " + temperature);
        }
        catch (Exception e)
        {
            Debug.LogError("Greska u spremanju vrijednosti temperature okoline: " + e.Message);
        }
    }

    // Podatci pritiska se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SavePressureToFile(float pressure)
    {
        string filePath = "Assets/GenData/PressureData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(pressure.ToString());
            }

            //Debug.Log("Pressure saved to file: " + pressure);
        }
        catch (Exception e)
        {
            Debug.LogError("Greska u spremanju vrijednosti pritiska okoline: " + e.Message);
        }
    }

    // Podatci temperature cijevi se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveTubeTempToFile(float temp)
    {
        string filePath = "Assets/GenData/TubeTemp.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(temp.ToString());
            }

            //Debug.Log("Temp saved to file: " + temp);
        }
        catch (Exception e)
        {
            Debug.LogError("Greska u spremanju vrijednosti temperature cijevi: " + e.Message);
        }
    }

    // Podatci vlaznosti cijevi se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveTubeHumToFile(float hum)
    {
        string filePath = "Assets/GenData/TubeHum.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(hum.ToString());
            }

            //Debug.Log("Humidity saved to file: " + hum);
        }
        catch (Exception e)
        {
            Debug.LogError("Greska u spremanju vrijednosti vlaznosti cijevi: " + e.Message);
        }
    }

    // Podatci koordinate x se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveMagXToFile(float x)
    {
        string filePath = "Assets/GenData/MagXData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(x.ToString());
            }

            //Debug.Log($"Magnetometer X data saved to file: {x}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska u spremanju vrijednosti koordinate x: {e.Message}");
        }
    }

    // Podatci koordinate y se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveMagYToFile(float y)
    {
        string filePath = "Assets/GenData/MagYData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(y.ToString());
            }

            //Debug.Log($"Magnetometer Y data saved to file: {y}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska u spremanju vrijednosti koordinate y: {e.Message}");
        }
    }

    // Podatci napona se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveVToFile(float v)
    {
        string filePath = "Assets/GenData/VData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(v.ToString());
            }

            //Debug.Log($"Magnetometer voltage data saved to file: {v}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska u spremanju vrijednosti napona: {e.Message}");
        }
    }

    // Podatci jakosti struje se spremaju u datoteku (putanja datoteke simulira putanju drivera Pixhawka)
    void SaveCToFile(float c)
    {
        string filePath = "Assets/GenData/CData.txt";

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(c.ToString());
            }

            //Debug.Log($"Magnetometer current data saved to file: {c}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Greska u spremanju vrijednosti struje: {e.Message}");
        }
    }
}
