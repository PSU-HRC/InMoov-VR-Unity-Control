
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class SendData : MonoBehaviour
{
    private SerialPort port;
    [SerializeField]
    private string COM;
    [SerializeField]
    private int baudRate;
    [SerializeField]
    private int x = 0;

    string[] fingerNames = { "Thumb", "Index", "Middle", "Ring", "Pinky" };

    // Start is called before the first frame update
    void Start()
    {
        port = new SerialPort(COM, baudRate);
        Debug.Log($"Com: {COM}, baud: {baudRate}");
        if (!port.IsOpen) {
            port.Open();
            Debug.Log("Port opened");
            System.Threading.Thread.Sleep(100);
        }
        Debug.Log($"TEST: {port.IsOpen}");
    }

    public void SendToArduino(HandData handData) {
        string data = ""; 
        data = ConfigureData(handData);

        if (data != null && data != "" && port.IsOpen)
        {
            port.WriteLine(data);
            //Debug.Log("Sent to Arduino: " + data);
        }
        
    }

    // Arduino will receive a string like this:
    // Left:32.32
  private string ConfigureData(HandData handData) {
    string formattedData = "";
    string debugData = "";

    for (int i = 0; i <= 4; i++) {
        Vector3 pos = handData.positions[i];
        Quaternion rot = handData.rotations[i];

        // Get the x-axis rotation and clamp it to [0, 180]
        float fingerBend = Mathf.Clamp(rot.eulerAngles.x, 0, 180);

        // Use the clamped value
        formattedData += $"{3 * Mathf.RoundToInt(fingerBend)} ";
        debugData += $"{fingerNames[i]}: {3 * Mathf.RoundToInt(fingerBend)} ";

        // Debug output for the last finger
        if (i == 4) {
            Debug.Log($"{debugData}");
        }
    }

    return formattedData;
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