using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonLauncher : MonoBehaviour
{
    Process _pythonProcess;

    void Start()
    {
        // 1) Build the full path to the script
        string scriptPath = Path.Combine(
            Application.streamingAssetsPath,
            "moveDetectioncvZone.py"
        );

        // 2) Configure the OS‐appropriate Python command
        // On Windows you might install Python and have "python" on PATH.
        // On macOS/Linux you may need "python3".
        string pythonExe = "python"; 
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            pythonExe = "python3";
        #endif

        // 3) Launch it
        var psi = new ProcessStartInfo
        {
            FileName              = pythonExe,
            Arguments             = $"\"{scriptPath}\"",
            UseShellExecute       = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow        = true
        };

        try
        {
            _pythonProcess = Process.Start(psi);
            // Optional: pipe its console output into Unity’s console
            _pythonProcess.OutputDataReceived += (s,e) => { if(e.Data!=null) UnityEngine.Debug.Log("[py] "+e.Data); };
            _pythonProcess.BeginOutputReadLine();
            _pythonProcess.BeginErrorReadLine();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to start Python: {ex.Message}");
        }
    }

    void OnApplicationQuit()
    {
        // clean up the Python process when Unity quits
        if (_pythonProcess != null && !_pythonProcess.HasExited)
        {
            _pythonProcess.Kill();
            _pythonProcess.Dispose();
        }
    }
}
