
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
        //string formattedData = $"{handData.handedness}:";
        string formattedData = "";
        string debugData = "";
        //from 1 to < 2 because we are isolating the pointer finger for testing. Should be 0 to 4
        for (int i = 0; i <= 4; i++)
        {
            Vector3 pos = handData.positions[i];
            Quaternion rot = handData.rotations[i];
            Vector3 angles = rot.eulerAngles;

            //formattedData += $"Joint{i} Pos: {pos.x:F2}, {pos.y:F2}, {pos.z:F2} ";
            //formattedData += $"Rot: {rot.x:F2}, {rot.y:F2}, {rot.z:F2}, {rot.w:F2} | ";

            float fingerBend = handData.rotations[i].eulerAngles.x;
            if (fingerBend > 180) {
                fingerBend = 0;
            }
            formattedData += $"{3*(Mathf.RoundToInt(fingerBend))} ";
            debugData += $"{fingerNames[i]}: {3*Mathf.RoundToInt(fingerBend)} ";
            if (i == 4) {
                Debug.Log($"{debugData}");
            }
            
        }       
        return formattedData;//.TrimEnd(); 
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