using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubController : MonoBehaviour
{
    public float speedChangeAmount; // The amount the speed will change
    public float maxForwardSpeed; // The maximum speed the submarine can move forwards
    public float maxBackwardSpeed; // The maximum speed the submarine can move backwards
    public float minSpeed; // The minimum speed the submarine can move before snapping to 0
    public float turnSpeed; // The speed the submarine turns left and right
    public float stabilizationSmoothing; // The smoothing applied to the correction turning
    public float riseSpeed; // The speed the submarine rises and lowers

    private float curSpeed; // The current stores forwards and backwards speed
    private Rigidbody rb; // Reference to the Rigidbody of the submarine
    private SubAnimations subAnimations; // Reference to the submarine animations script

    // nove skripte
    public UDPComm udpComm;
    public UDPComm2 udpComm2;
    public UDPComm3 udpComm3;
    private HorT200Move horT200Move;
    private TurT200Move turT200Move;
    private VerT200Move verT200Move;

    // za automatiziranu misiju
    public TemperatureSensorScript temperatureSensor;

    // enumerator za korake misije
    public enum MissionState
    {
        Idle,
        Forward,
        Up,
        TakePicture,
        Down,
        SpecialReading,
        ColorChange
    } // inicijalizacija na "Idle"
    private MissionState missionState = MissionState.Idle;
    private string[] nextArray = { "Forward", "Up", "Forward", "TakePicture", "Up", "Down", "SpecialReading", "Forward" }; // po volji programera
    public MissionState[] states;
    public MissionState[] nextStates;
    public MissionState nextState;
    public int h;

    private float elapsedTime = 0f;

    public float seaCurrentEffect = 20f;
    public Transform seaCurrentTransform;
    public float proximityThreshold = 0.5f;
    public bool isInSeaCurrent = false;

    private EnvironmentManagerScript environmentManagerScript;
    public float seaCurrentHorizontalEffect = 30f;
    public Transform seaCurrentHorizontalTransform;

    private bool isCompensationMode = false;
    public Text cmText;

    // Start is called before the first frame update
    void Start()
    {
        // pronadji skripte motora u sustavu
        Transform horT200Grandchild = FindDeepChild(transform, "HorT200");
        if (horT200Grandchild != null)
        {
            // Get the HorT200Move script from the found grandchild GameObject
            horT200Move = horT200Grandchild.GetComponent<HorT200Move>();
            //Debug.Log("HorT200Move script found!");

            if (horT200Move == null)
            {
                Debug.LogError("HorT200Move skripta se ne moze pronaci!");
            }
        }
        else
        {
            Debug.LogError("HorT200 GameObject ne postoji?!");
        }

        Transform turT200Grandchild = FindDeepChild(transform, "TurT200");
        if (turT200Grandchild != null)
        {
            turT200Move = turT200Grandchild.GetComponent<TurT200Move>();

            if (turT200Move == null)
            {
                Debug.LogError("TurT200Move skripta se ne moze pronaci!");
            }
        }
        else
        {
            Debug.LogError("TurT200Move GameObject ne postoji?!");
        }

        Transform verT200Grandchild = FindDeepChild(transform, "VerT200");
        if (verT200Grandchild != null)
        {
            verT200Move = verT200Grandchild.GetComponent<VerT200Move>();

            if (verT200Move == null)
            {
                Debug.LogError("VerT200Move skripta se ne moze pronaci!");
            }
        }
        else
        {
            Debug.LogError("VerT200 GameObject ne postoji?!");
        }

        rb = GetComponent<Rigidbody>(); // Getting the Rigidbody reference
        subAnimations = GetComponent<SubAnimations>(); // Getting the Sub Animations script reference
        udpComm = GetComponent<UDPComm>();
        udpComm2 = GetComponent<UDPComm2>();
        udpComm3 = GetComponent<UDPComm3>();
        temperatureSensor = GetComponent<TemperatureSensorScript>();

        environmentManagerScript = GameObject.Find("Environment").GetComponent<EnvironmentManagerScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move(); // Move the submarine forwards and backwards
        Turn(); // Turn the submarine left and rise
        Rise(); // Rise and lower the submarine
        //Stabilize(); // Correct the submarine's rotation to be upright even when it knocks into objects

        if (Input.GetKeyDown(KeyCode.M)) // ako se pritisne "M", pokrece se isprogramirana misija
            StartMission(0);
        HandleMission(); // omogucuje azuriranje stanja misije

        SeaCurrentVerticalEffect(); // za morsku struju
        SeaCurrentHorizontalEffect();

        environmentManagerScript.UpdateMagnetometer();

        if (Input.GetKey(KeyCode.N))
        {
            isCompensationMode = !isCompensationMode;
        }

        UpdateCMUI(isCompensationMode);
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W)) // When the player presses the W key
        {
            curSpeed += speedChangeAmount; // Add to the submarine's speed
            subAnimations.Spin(1); // Spin the turbine forwards based on the current speed

            if (Mathf.Abs(curSpeed) > 0.1f)
            {
                environmentManagerScript.UpdateMagnetometer();
            }
        }
        else if (Input.GetKey(KeyCode.S)) // When the player presses the W key
        {
            curSpeed -= speedChangeAmount; // Subtract from the submarine's speed
            subAnimations.Spin(-1); // Spin the turbine backwards based on the current speed

            if (Mathf.Abs(curSpeed) > 0.1f)
            {
                environmentManagerScript.UpdateMagnetometer();
            }
        }
        else if (Mathf.Abs(curSpeed) <= minSpeed) // If the player is not pressing W or S and the current speed is less than the minumum speed
        {
            curSpeed = 0; // Snap the submarine to not move
            subAnimations.Spin(0); // Stop spinning the turbine
        }
        else if (curSpeed != 0) // If the player is not pressing W or S but is moving
        {
            subAnimations.Spin(curSpeed/Mathf.Abs(curSpeed)/2); // Idly spin the turbine based on the current speed
        }
        curSpeed = Mathf.Clamp(curSpeed, -maxBackwardSpeed, maxForwardSpeed); // Clamp the current speed based on it's max values in both directions
        //rb.AddForce(transform.forward * curSpeed); // Apply the force to the Rigidbody to move the submarine

        udpComm.SendUDP(curSpeed); // salji podatak protokolom UDP
        //Debug.Log("CS" + curSpeed);

        // Dodatna provjera
        if (horT200Move != null)
        {
            float speed = horT200Move.TempCurSpeed; // dohvati propertyjem
            //Debug.Log("ts" + speed);
            rb.AddForce(transform.forward * speed); // naredba za kretanje
        }
        else
        {
            Debug.LogError("horT200Move is null!");
        }

    }

    void Turn()
    {
        if (Input.GetKey(KeyCode.D)) // When the player presses the D key
        {
            udpComm2.SendUDP(turnSpeed);
            if (turT200Move != null)
            {
                float speed = turT200Move.TempCurSpeed;
                //Debug.Log("ts" + speed);
                rb.AddTorque(transform.up * speed);

                if (Mathf.Abs(turnSpeed) > 0.01f)
                {
                    environmentManagerScript.UpdateMagnetometer();
                }
            }
            else
            {
                Debug.LogError("horT200Move is null!");
            }
            //rb.AddTorque(transform.up * turnSpeed); // Apply torque to turn the submarine right
            subAnimations.DrillTurn(1); // Turn the drill right
        }
        else if (Input.GetKey(KeyCode.A)) // When the player presses the A key
        {
            udpComm2.SendUDP(turnSpeed);
            if (turT200Move != null)
            {
                float speed = turT200Move.TempCurSpeed;
                //Debug.Log("ts" + speed);
                rb.AddTorque(transform.up * -speed);

                if (Mathf.Abs(turnSpeed) > 0.01f)
                {
                    environmentManagerScript.UpdateMagnetometer();
                }
            }
            else
            {
                Debug.LogError("horT200Move is null!");
            }
            //rb.AddTorque(transform.up * -turnSpeed); // Apply torque to turn the submarine left
            subAnimations.DrillTurn(-1); // Turn the drill left
        }
        else // If the player is not turning
        {
            subAnimations.DrillTurn(0); // Stop turning the drill
        }
    }

    void Rise()
    {
        if (Input.GetKey(KeyCode.LeftShift)) // When the player presses the Left Shift key
        {
            udpComm3.SendUDP(riseSpeed);
            if (verT200Move != null)
            {
                float speed = verT200Move.TempCurSpeed;
                //Debug.Log("ts" + speed);
                rb.AddForce(transform.up * speed);
            }
            else
            {
                Debug.LogError("horT200Move is null!");
            }
            //rb.AddForce(transform.up * riseSpeed); // Apply force to make the submarine rise
            subAnimations.FinTurn(-1); // Turn the fins down
            environmentManagerScript.UpdatePressure();
        }
        else if (Input.GetKey(KeyCode.LeftControl)) // When the player presses the Left Control key
        {
            udpComm3.SendUDP(riseSpeed);
            if (verT200Move != null)
            {
                float speed = verT200Move.TempCurSpeed;
                //Debug.Log("ts" + speed);
                rb.AddForce(transform.up * -speed);
            }
            else
            {
                Debug.LogError("horT200Move is null!");
            }
            //rb.AddForce(transform.up * -riseSpeed); // Apply force to make the submarine lower
            subAnimations.FinTurn(1); // Turn the fins up
            environmentManagerScript.UpdatePressure();
        }
        else // If the player is not moving vertically
        {
            subAnimations.FinTurn(0); // Stop turning the fins
        }
    }

    void Stabilize()
    {
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.Euler(new Vector3(0, rb.rotation.eulerAngles.y, 0)), stabilizationSmoothing)); // Smoothly and slowly rotate the submarine to be upright
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        Transform result = parent.Find(name); // naredba za trazenje podkomponenti

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

    void StartMission(int index)
    {
        //Debug.Log("Pocetak.");
        states = new MissionState[nextArray.Length];
        nextStates = new MissionState[nextArray.Length - 1];

        for (int i = 0; i < nextArray.Length; i++)
        {
            //Debug.Log("Petlja: " + i);
            switch (nextArray[i])
            {
                case "Idle":
                    states[i] = MissionState.Idle;
                    break;

                case "Forward":
                    states[i] = MissionState.Forward;
                    break;

                case "Up":
                    states[i] = MissionState.Up;
                    break;

                case "TakePicture":
                    states[i] = MissionState.TakePicture;
                    break;

                case "Down":
                    states[i] = MissionState.Down;
                    break;

                case "SpecialReading":
                    states[i] = MissionState.SpecialReading;
                    break;

                case "ColorChange":
                    states[i] = MissionState.ColorChange;
                    break;

                default:
                    Debug.LogError("Nepoznato stanje: " + nextArray[i]);
                    break;
            }
        }
        //Debug.Log("Inicijalizirano.");
        if (index >= 0 && index < states.Length)
        {
            missionState = states[index];
            nextState = states[index + 1];
            //Debug.Log("Pocetno stanje: " + missionState);
        }
        else
        {
            Debug.LogError("Pogresan indeks!");
        }
        //Debug.Log("Stanja u novoj misiji: " + string.Join(", ", states));
        for (int m = 0; m < nextArray.Length - 1; m++)
        {
            nextStates[m] = states[m + 1];
        }
        //Debug.Log("Stanja u novoj misiji: " + string.Join(", ", nextStates));
    }

    void HandleMission()
    {
        switch (missionState)
        { // ovisno o koraku, promjena 
            case MissionState.Forward:
                //Debug.Log("naprid");
                if (environmentManagerScript.UpdatePressure() == true)
                {
                    udpComm3.SendUDP(riseSpeed);
                    float speed = verT200Move.TempCurSpeed;
                    rb.AddForce(transform.up * -speed);
                    subAnimations.FinTurn(1);
                    MoveForward(2f, nextState);
                }
                else if (environmentManagerScript.UpdateMagnetometer() == -1)
                {
                    subAnimations.Spin(1);
                    udpComm.SendUDP(curSpeed);
                    float speed = horT200Move.TempCurSpeed;
                    rb.AddForce(transform.forward * speed);
                    MoveForward(2f, nextState);
                }
                else
                    MoveForward(2f, nextState);
                break;

            case MissionState.Up:
                //Debug.Log("gori");
                MoveUp(1f, nextState);
                break;

            case MissionState.TakePicture:
                //Debug.Log("pic");
                TakePicture(nextState);
                break;

            case MissionState.Down:
                //Debug.Log("doli");
                MoveDown(1f, nextState);
                break;

            case MissionState.SpecialReading:
                HandleSpecialReading(nextState);
                break;
        }
    }

    void MoveForward(float distance, MissionState newState)
    {
        curSpeed = 2f; // trenutna brzina podmornice

        subAnimations.Spin(1); // predprogramirana animacija
        udpComm.SendUDP(curSpeed); // slanje naredbe motoru preko UDP-a
        float speed = horT200Move.TempCurSpeed; // dohvat trenutne brzine propertyjem
        rb.AddForce(transform.forward * speed);

        elapsedTime += Time.fixedDeltaTime; // azuriranje za rad u stvarnom vremenu
        if (elapsedTime >= distance) // kada predje zadanu udaljenost, prelazi na novi korak
        {
            h++;
            if (h < nextStates.Length)
            {
                missionState = newState;
                nextState = nextStates[h];
            }
            else if (h == nextStates.Length)
            {
                missionState = nextStates[h - 1];
            }
            else
            {
                missionState = MissionState.Idle;
                h = 0;
            }
            elapsedTime = 0f;
            curSpeed = 0f;
        }
    }


    void MoveUp(float distance, MissionState newState)
    {
        udpComm3.SendUDP(riseSpeed);
        float speed = verT200Move.TempCurSpeed;
        rb.AddForce(transform.up * speed);
        subAnimations.FinTurn(-1);

        elapsedTime += Time.fixedDeltaTime;
        //Debug.Log($"MoveUp - Elapsed Time: {elapsedTime}, Speed: {speed}");

        if (elapsedTime >= distance)
        {
            h++;
            if (h < nextStates.Length)
            {
                missionState = newState;
                nextState = nextStates[h];
            }
            else if (h == nextStates.Length)
            {
                missionState = nextStates[h - 1];
            }
            else
            {
                missionState = MissionState.Idle;
                h = 0;
            }
            elapsedTime = 0f;
            //Debug.Log("MoveUp - Transition to TakePicture state");
        }
    }


    void TakePicture(MissionState newState)
    {
        ScreenCapture.CaptureScreenshot("Assets/Screenshots/MissionScreenshot.png");
        h++;
        if (h < nextStates.Length)
        {
            missionState = newState;
            nextState = nextStates[h];
        }
        else if (h == nextStates.Length)
        {
            missionState = nextStates[h - 1];
        }
        else
        {
            missionState = MissionState.Idle;
            h = 0;
        };
    }

    void MoveDown(float distance, MissionState newState)
    {
        udpComm3.SendUDP(riseSpeed);
        float speed = verT200Move.TempCurSpeed;
        rb.AddForce(transform.up * -speed);
        subAnimations.FinTurn(1);

        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= distance)
        {
            h++;
            if (h < nextStates.Length)
            {
                missionState = newState;
                nextState = nextStates[h];
            }
            else if (h == nextStates.Length)
            {
                missionState = nextStates[h - 1];
            }
            else
            {
                missionState = MissionState.Idle;
                h = 0;
            }
            elapsedTime = 0f;
        }
    }

    void HandleSpecialReading(MissionState newState)
    {
        TemperatureSensorScript temperatureSensor = GetTemperatureSensorReference();
        temperatureSensor.ChangeTextColor(Color.red, 3f, Color.white);
        h++;
        if (h < nextStates.Length)
        {
            missionState = newState;
            nextState = nextStates[h];
        }
        else if (h == nextStates.Length)
        {
            missionState = nextStates[h-1];
        }
        else
        {
            missionState = MissionState.Idle;
            h = 0;
        }
    }

    TemperatureSensorScript GetTemperatureSensorReference()
    {
        GameObject submarine = GameObject.Find("Submarine");

        if (submarine != null)
        {
            Transform tempSensorTransform = submarine.transform.Find("TempSensor");

            if (tempSensorTransform != null)
            {
                TemperatureSensorScript temperatureSensor = tempSensorTransform.GetComponent<TemperatureSensorScript>();

                if (temperatureSensor != null)
                {
                    return temperatureSensor;
                }
            }
        }

        return null;
    }

    void SeaCurrentVerticalEffect()
    {
        Vector3 subPosition = transform.position; // trenutna pozicija
        Vector3 seaCurrentPosition = seaCurrentTransform.position; // pozicija struje

        if (Vector3.Distance(subPosition, seaCurrentPosition) < proximityThreshold)
        { // udaljenost u 3D prostoru
            if (!isCompensationMode)
            {
                rb.AddForce(Vector3.up * seaCurrentEffect);
            }
            else
            {
                StartCoroutine(CompensationModeForces());
            }
            environmentManagerScript.UpdatePressure();
        }
    }
    IEnumerator CompensationModeForces()
    {
        rb.AddForce(Vector3.up * seaCurrentEffect / 2);
        yield return new WaitForSeconds(1.0f); // Adjust the time as needed
        rb.AddForce(Vector3.down * seaCurrentEffect / 2);
    }

    void SeaCurrentHorizontalEffect()
    {
        Vector3 subPosition = transform.position; // trenutna pozicija
        Vector3 seaCurrentPosition = seaCurrentHorizontalTransform.position; // pozicija struje

        if (Vector3.Distance(subPosition, seaCurrentPosition) < proximityThreshold)
        {
            if (!isCompensationMode)
            {
                rb.AddForce(-Vector3.forward * seaCurrentHorizontalEffect);
            }
            else
            {
                StartCoroutine(CompensationModeHorizontalForces());
            }
        }
    }

    IEnumerator CompensationModeHorizontalForces()
    {
        rb.AddForce(-Vector3.forward * seaCurrentHorizontalEffect / 2);
        yield return new WaitForSeconds(1.0f); // Adjust the time as needed
        rb.AddForce(Vector3.forward * seaCurrentHorizontalEffect / 2);
    }

    void UpdateCMUI(bool cm)
    {
        cmText.text = "Mod kompenzacije: " + cm;
    }
}
