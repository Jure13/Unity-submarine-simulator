using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class SQLiteDB : MonoBehaviour
{
    private IDbConnection dbConnection;
    public class SubmarineData
    {
        public int ID { get; set; }
        public string Timestamp { get; set; }
        public float SubmarinePositionX { get; set; }
        public float SubmarinePositionY { get; set; }
        public float SubmarinePositionZ { get; set; }
        public float SubmarineRotationX { get; set; }
        public float SubmarineRotationY { get; set; }
        public float SubmarineRotationZ { get; set; }
        public float SubmarineScaleX { get; set; }
        public float SubmarineScaleY { get; set; }
        public float SubmarineScaleZ { get; set; }
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float TubeTemperature { get; set; }
        public float TubeHumidity { get; set; }
        public float MagX { get; set; }
        public float MagY { get; set; }
        public float Voltage { get; set; }
        public float Current { get; set; }
        public float TempCurSpeedHorizontal { get; set; }
        public float TempCurSpeedVertical { get; set; }
        public float TempCurSpeedTurn { get; set; }
    }

    private HorT200Move horT200Move;
    private TurT200Move turT200Move;
    private VerT200Move verT200Move;

    // Start is called before the first frame update
    void Start()
    {
        string dbUri = "URI=file:" + Application.dataPath + "/SubmarineData.db";
        dbConnection = new SqliteConnection(dbUri);
        dbConnection.Open();

        CreateTable();

        // Ažuriraj tablicu svakih 5 sekundi
        InvokeRepeating("InsertDataPeriodically", 0f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateTable()
    {
        using (IDbCommand dbCmd = dbConnection.CreateCommand())
        {
            string createTableQuery = "CREATE TABLE IF NOT EXISTS SubmarineData " +
            "(ID INTEGER PRIMARY KEY, Timestamp TEXT, " +
            "SubmarinePositionX REAL, SubmarinePositionY REAL, SubmarinePositionZ REAL, " +
            "SubmarineRotationX REAL, SubmarineRotationY REAL, SubmarineRotationZ REAL, " +
            "SubmarineScaleX REAL, SubmarineScaleY REAL, SubmarineScaleZ REAL, " +
            "Temperature REAL, Pressure REAL, TubeTemperature REAL, TubeHumidity REAL, " +
            "MagX REAL, MagY REAL, Voltage REAL, Current REAL, " +
            "TempCurSpeedHorizontal REAL, TempCurSpeedVertical REAL, TempCurSpeedTurn REAL)";
            dbCmd.CommandText = createTableQuery;
            dbCmd.ExecuteNonQuery();
        }
    }

    void InsertData(SubmarineData data)
    {
        using (IDbCommand dbCmd = dbConnection.CreateCommand())
        {
            string insertDataQuery = "INSERT INTO SubmarineData " +
            "(Timestamp, SubmarinePositionX, SubmarinePositionY, SubmarinePositionZ, " +
            "SubmarineRotationX, SubmarineRotationY, SubmarineRotationZ, " +
            "SubmarineScaleX, SubmarineScaleY, SubmarineScaleZ, " +
            "Temperature, Pressure, TubeTemperature, TubeHumidity, " +
            "MagX, MagY, Voltage, Current, " +
            "TempCurSpeedHorizontal, TempCurSpeedVertical, TempCurSpeedTurn) " +
            $"VALUES ('{data.Timestamp}', " +
            $"{data.SubmarinePositionX}, {data.SubmarinePositionY}, {data.SubmarinePositionZ}, " +
            $"{data.SubmarineRotationX}, {data.SubmarineRotationY}, {data.SubmarineRotationZ}, " +
            $"{data.SubmarineScaleX}, {data.SubmarineScaleY}, {data.SubmarineScaleZ}, " +
            $"{data.Temperature}, {data.Pressure}, {data.TubeTemperature}, {data.TubeHumidity}, " +
            $"{data.MagX}, {data.MagY}, {data.Voltage}, {data.Current}, " +
            $"{data.TempCurSpeedHorizontal}, {data.TempCurSpeedVertical}, {data.TempCurSpeedTurn})";
            dbCmd.CommandText = insertDataQuery;

            dbCmd.ExecuteNonQuery();
        }
    }

    void InsertDataPeriodically()
    {
        Vector3 submarinePosition = transform.position;
        Vector3 submarineRotation = transform.rotation.eulerAngles;
        Vector3 submarineScale = transform.localScale;
        float temperature = EnvironmentManagerScript.currentTemperature;
        float tubeTemperature = EnvironmentManagerScript.currentTubeTemperature;
        float tubeHumidity = EnvironmentManagerScript.currentTubeHumidity;
        float voltage = EnvironmentManagerScript.currentVoltage;
        float current = EnvironmentManagerScript.currentCurrent;
        float pressure = EnvironmentManagerScript.currentPressure;
        float magX = EnvironmentManagerScript.magX;
        float magY = EnvironmentManagerScript.magY;
        Transform horT200Grandchild = FindDeepChild(transform, "HorT200");
        horT200Move = horT200Grandchild.GetComponent<HorT200Move>();
        float horSpeed = horT200Move.TempCurSpeed;
        Transform turT200Grandchild = FindDeepChild(transform, "TurT200");
        turT200Move = turT200Grandchild.GetComponent<TurT200Move>();
        float turSpeed = turT200Move.TempCurSpeed;
        Transform verT200Grandchild = FindDeepChild(transform, "VerT200");
        verT200Move = verT200Grandchild.GetComponent<VerT200Move>();
        float verSpeed = verT200Move.TempCurSpeed;

        SubmarineData data = new SubmarineData
        {
            Timestamp = System.DateTime.Now.ToString(),
            SubmarinePositionX = submarinePosition.x,
            SubmarinePositionY = submarinePosition.y,
            SubmarinePositionZ = submarinePosition.z,
            SubmarineRotationX = submarineRotation.x,
            SubmarineRotationY = submarineRotation.y,
            SubmarineRotationZ = submarineRotation.z,
            SubmarineScaleX = submarineScale.x,
            SubmarineScaleY = submarineScale.y,
            SubmarineScaleZ = submarineScale.z,
            Temperature = temperature,
            Pressure = pressure,
            TubeTemperature = tubeTemperature,
            TubeHumidity = tubeHumidity,
            MagX = magX,
            MagY = magY,
            Voltage = voltage,
            Current = current,
            TempCurSpeedHorizontal = horSpeed,
            TempCurSpeedVertical = verSpeed,
            TempCurSpeedTurn = turSpeed,
        };

        InsertData(data);
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        Transform result = parent.Find(name);

        if (result != null)
        {
            return result;
        }

        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    void OnDestroy()
    {
        if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
        {
            dbConnection.Close();
        }
    }
}
