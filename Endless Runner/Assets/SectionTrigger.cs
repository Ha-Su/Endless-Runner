using UnityEngine;
using UnityEngine.UI;

public class SectionTrigger : MonoBehaviour
{
    [Header("Road Spawning")]
    public GameObject roadSection;

    [Header("Game Over UI")]
    public GameObject gameOverCanvas;   
    public Text     finalScoreText;  
    public ScoreManager scoreManager;   

    bool isGameOver = false;

    void Start()
    {
        // Hide the Game Over panel at start
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Trigger"))
        {
            // Spawn next road section
            Instantiate(
                roadSection,
                new Vector3(60f, 0f, 60f),
                Quaternion.identity
            );
        }
        else if (other.CompareTag("Obstacles"))
        {
            // Player hit an obstacle = Game Over
            DoGameOver();
        }
    }

    void DoGameOver()
    {
        isGameOver = true;

        Time.timeScale = 0f;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (finalScoreText != null && scoreManager != null)
            finalScoreText.text = scoreManager.scoreText.text;
    }
}
