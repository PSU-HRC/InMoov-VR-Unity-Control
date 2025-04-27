using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class SendData : MonoBehaviour
{
    private SerialPort port;
    [SerializeField] private string COM = "COM3";
    [SerializeField] private int baudRate = 9600;
    private string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky", "Elbow" };

    void Start()
    {
        port = new SerialPort(COM, baudRate);
        try {
            port.Open();
            Debug.Log($"Port opened: {port.IsOpen}");
        } catch (System.Exception e) {
            Debug.LogError($"Port error: {e.Message}");
        }
    }

    public void SendToArduino(XRHandsManager.HandData handData)
    {
        if (!port.IsOpen) return;
        
        string data = $"{handData.handedness}:";
        string debugOutput = $"{handData.handedness} Hand - ";
        
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Clamp(handData.rotations[i].eulerAngles.x, 0, 180);
            int value = (i < 5) ? 3 * Mathf.RoundToInt(angle) : Mathf.RoundToInt(angle);
            
            data += $"{value} ";
            debugOutput += $"{fingerNames[i]}:{value} ";
        }
        
        port.WriteLine(data.Trim());
        Debug.Log(debugOutput);
    }

    void OnApplicationQuit()
    {
        if (port.IsOpen) port.Close();
    }
}


/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class SendData : MonoBehaviour
{
    private SerialPort port;
    [SerializeField] private string COM;
    [SerializeField] private int baudRate;
    [SerializeField] private int x = 0;

    string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

    // Start is called before the first frame update
    void Start()
    {
        port = new SerialPort(COM, baudRate);
        try {
            port.Open();
            Debug.Log($"Port opened: {port.IsOpen}");
        } catch (System.Exception e) {
            Debug.LogError($"Port error: {e.Message}");
        }
        // Debug.Log($"Com: {COM}, baud: {baudRate}");
        // if (!port.IsOpen) {
        //     port.Open();
        //     Debug.Log("Port opened");
        //     System.Threading.Thread.Sleep(100);
        // }
        // Debug.Log($"TEST: {port.IsOpen}");
    }

    public void SendToArduino(HandData handData) {
        if (!port.IsOpen) return;

        string data = ""; 
        data = ConfigureData(handData);

        if (data != null && data != "" && port.IsOpen)
        {
            port.WriteLine(data);
            Debug.Log("Sent to Arduino: " + data);
        }
        
    }

    // Arduino will receive a string like this:
    // Left:32.32
private string ConfigureData(HandData handData) 
{
    // Start with hand identifier (Left/Right)
    string formattedData = $"{handData.handedness}:";
    string debugData = $"{handData.handedness} Hand - ";

    for (int i = 0; i <= 4; i++) 
    {
        // Vector3 pos = handData.positions[i];
        Quaternion rot = handData.rotations[i];

        // Get the x-axis rotation and clamp it to [0, 180]
        float fingerBend = Mathf.Clamp(rot.eulerAngles.x, 0, 180);
        int servoValue = 3 * Mathf.RoundToInt(fingerBend);

        // Format for Arduino (values separated by spaces)
        formattedData += $"{servoValue} ";
        
        // Format for debug output
        debugData += $"{fingerNames[i]}:{servoValue} ";

        // Debug output for the last finger
        if (i == 4) 
        {
            Debug.Log(debugData);
        }
    }

    // Remove trailing space and return
    return formattedData.Trim();
}



    void OnApplicationQuit()
    {
        // Close the serial port when the application ends
        if (port.IsOpen)
        {
            port.Close();
            Debug.Log("Serial Port Closed");
        }
    }
}
*/