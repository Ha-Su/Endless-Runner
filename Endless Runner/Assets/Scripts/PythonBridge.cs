using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonLauncher : MonoBehaviour
{
    Process pyProcess;

    void Awake()
    {
        // Build path to the exe in the application folder
        string exeName = "moveDetectioncvZone";
        string exePath = Path.Combine(Application.dataPath, "../" + exeName);

        // If you want to debug path issues:
            UnityEngine.Debug.Log("Looking for Python exe at: " + exePath);

        var startInfo = new ProcessStartInfo {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        pyProcess = Process.Start(startInfo);
    }

    void OnApplicationQuit()
    {
        if (pyProcess != null && !pyProcess.HasExited)
            pyProcess.Kill();
    }
}
