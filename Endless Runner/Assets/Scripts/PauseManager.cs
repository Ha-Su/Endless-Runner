using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;    
    public GameObject gameOverCanvas;   

    [Header("Scenes")]
    public string calibrationSceneName = "Calibrate"; 

    bool isPaused = false;
    bool prevPray = false;

    void Update()
    {
        // Don’t allow pause/restart once Game Over is showing
        if (gameOverCanvas != null && gameOverCanvas.activeSelf)
            return;

        // Toggle pause on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else          Pause();
        }

        // Quick restart with R
        if (Input.GetKeyDown(KeyCode.R))
            ResetLevel();

        // Restart via pray gesture
        bool nowPray = UDPReceiver.lastAction == "praying";
        if (nowPray && !prevPray)
            ResetLevel();
        prevPray = nowPray;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
    public void ReturnToCalibrate()
    {
        Time.timeScale = 1f;  
        SceneManager.LoadScene(calibrationSceneName);
    }
}
