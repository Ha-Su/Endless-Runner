using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonLauncher : MonoBehaviour
{
    private static bool _launched = false;
    private Process      _pyProc;

    void Awake()
    {
        if (_launched)
        {
            Destroy(gameObject);
            return;
        }
        _launched = true;
        DontDestroyOnLoad(gameObject);

        string exeName = "moveDetectioncvZone";

        // Application.dataPath == ".../Endless Running.app/Contents"
        // Point into Contents/MacOS/exeName
        string exePath = Path.Combine(Application.dataPath, "MacOS", exeName);

        UnityEngine.Debug.Log($"[PythonLauncher] Looking for detector at: {exePath}");
        if (!File.Exists(exePath))
        {
            UnityEngine.Debug.LogError($"[PythonLauncher] NOT FOUND: {exeName} at {exePath}");
            return;
        }

        var startInfo = new ProcessStartInfo {
            FileName        = exePath,
            UseShellExecute = false,
            CreateNoWindow  = true
        };
        _pyProc = Process.Start(startInfo);
        UnityEngine.Debug.Log("[PythonLauncher] Detector launched");
    }

    void OnApplicationQuit()
    {
        if (_pyProc != null && !_pyProc.HasExited)
            _pyProc.Kill();
    }
}
