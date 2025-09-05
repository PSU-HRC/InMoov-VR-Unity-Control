using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SendData : MonoBehaviour
{
    private SerialPort port;
    [SerializeField] private string COM = "COM3";
    [SerializeField] private int baudRate = 9600;

    private Thread writerThread;
    private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
    private volatile bool running;

    void Start()
    {
        try
        {
            port = new SerialPort(COM, baudRate);
            port.WriteTimeout = 100; // short timeout
            port.Open();
            running = true;

            writerThread = new Thread(WriterLoop) { IsBackground = true };
            writerThread.Start();

            Debug.Log($"Serial port opened: {port.IsOpen}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Serial port error: {e.Message}");
        }
    }

    public void SendToArduino(XRHandsManager.HandData handData)
    {
        if (!running) return;

        string data = $"{handData.handedness}:";
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Clamp(handData.rotations[i].eulerAngles.x, 0, 180);
            int value = (i < 5) ? 3 * Mathf.RoundToInt(angle) : Mathf.RoundToInt(angle);
            data += $"{value} ";
        }

        queue.Enqueue(data.Trim());
    }

    private void WriterLoop()
    {
        while (running)
        {
            if (queue.TryDequeue(out string msg))
            {
                try
                {
                    if (port != null && port.IsOpen)
                    {
                        port.WriteLine(msg);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Serial write failed: {e.Message}");
                }
            }
            else
            {
                Thread.Sleep(1); // avoid busy loop
            }
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        if (writerThread != null && writerThread.IsAlive)
        {
            writerThread.Join(200);
        }

        if (port != null && port.IsOpen)
        {
            port.Close();
        }
    }
}
