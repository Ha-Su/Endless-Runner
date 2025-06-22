using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText;
    public Text finalScore;

    [Header("Scoring")]
    public float pointsPerSecond = 100f;  // 100 points each second
    private float rawScore = 0f;

    void Start()
    {
        rawScore = 0f;
        UpdateScoreText();
    }

    void Update()
    {
        rawScore += pointsPerSecond * Time.deltaTime;

        // Update the on-screen text by casting to int
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "" + Mathf.FloorToInt(rawScore);
    }
    public void ResetScore()
    {
        rawScore = 0f;
        UpdateScoreText();
    }

    public void ShowFinalScore()
    {
    int displayScore = Mathf.FloorToInt(rawScore);
    finalScore.text = displayScore.ToString();
    }

}
