using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;
    int port = 5054;

    public static string lastAction = "stand";
    public static string lastSide = "center";

    void Start()
    {
        Debug.Log("UDPReceiver started");
        client = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
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
                string text = Encoding.UTF8.GetString(data);

                string[] parts = text.Split(',');
                if (parts.Length == 2)
                {
                    lastAction = parts[0]; // e.g., "jump"
                    lastSide = parts[1];   // e.g., "left"
                    Debug.Log("Received: " + lastAction + ", " + lastSide);
                }
            }
            catch (System.Exception err)
            {
                Debug.LogError("UDP receive error: " + err.ToString());
            }
        }
    }

    void OnApplicationQuit()
    {
        receiveThread?.Abort();
        client?.Close();
    }
}
