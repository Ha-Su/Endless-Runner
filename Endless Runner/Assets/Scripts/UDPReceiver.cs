using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    public static UDPReceiver Instance { get; private set; }

    UdpClient client;
    Thread   receiveThread;
    int      port = 5054;

    // these already existed:
    public static string lastAction = "none";
    public static string lastSide   = "none";

    // NEW: flag to let CalibrationManager know we're ready
    public static bool isCalibrated = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("UDPReceiver started on port " + port);
        client = new UdpClient(port);
        receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true
        };
        receiveThread.Start();
    }

    void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data).Trim();

                var parts = text.Split(',');
                if (parts.Length == 2)
                {
                    // detect our special calibration packet
                    if (parts[0] == "calibrated")
                    {
                        isCalibrated = true;
                        Debug.Log("[UDP] Calibration complete!");
                    }
                    else
                    {
                        // normal gameplay messages
                        lastAction = parts[0]; 
                        lastSide   = parts[1];  
                        Debug.Log($"[UDP] Received → Action: {lastAction}, Side: {lastSide}");
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // expected on quit
                return;
            }
            catch (System.Exception err)
            {
                Debug.LogError("UDP receive error: " + err);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Abort();
        client?.Close();
    }
}
