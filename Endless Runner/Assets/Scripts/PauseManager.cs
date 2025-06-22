using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuUI;    // Assign your Pause Menu Canvas here

    bool isPaused = false;

    void Update()
    {
        // Toggle pause on Escape (or whatever key you like)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else          Pause();
        }

        // Optional: allow quick restart with R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }
    }

    // Freeze time & show menu
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;    // stops Update(), physics, animations, etc.
        isPaused = true;
        // Optional: unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Unfreeze & hide menu
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        // Optional: re-lock cursor if you're using one
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Reloads the active scene
    public void ResetLevel()
    {
        // Make sure timeScale is back to normal
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
