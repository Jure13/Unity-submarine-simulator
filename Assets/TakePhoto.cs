using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakePhoto : MonoBehaviour
{
    private int i = 0; // Brojac
    private string name; // Pomocna varijabla za naziv datoteke

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && i < 20) // Proizvoljan odabir mogucih fotografija
        {
            string name = $"Screenshot_{i}.png";
            ScreenCapture.CaptureScreenshot($"Assets/Screenshots/{name}"); // Putanja na kojoj se spremaju fotografije
            i++;

            if (i == 20)
                Debug.Log("Dosta!");
        }
    }
}
