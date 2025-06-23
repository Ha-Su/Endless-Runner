using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CalibrationManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchingPanel;   // cover screen with “Launching…” text
    public GameObject tutorialPanel;    // your existing tutorial UI

    [Header("Tutorial UI References (inside TutorialPanel)")]
    public Text  titleText;
    public Text  leftCounterText;
    public Text  rightCounterText;
    public GameObject jumpCounterContainer;
    public Text  jumpCounterValueText;
    public Text  instructionText;

    [Header("Settings")]
    public int   targetCount      = 5;
    public float startHoldSeconds = 3f;
    public string gameSceneName   = "Game";

    int leftCount  = 0;
    int rightCount = 0;
    int jumpCount  = 0;

    string prevSide = "";
    string prevJump = "";

    enum State { Launching, Sides, Jump, ReadyToStart, CountingDown }
    State state = State.Launching;

    void Start()
    {
        // start in Launching
        launchingPanel.SetActive(true);
        tutorialPanel.SetActive(false);
    }

    void Update()
    {
        switch (state)
        {
            case State.Launching:
                // if Python explicitly sent calibrated OR
                // if we've seen ANY movement packet yet, swap panels
                if (UDPReceiver.isCalibrated || UDPReceiver.lastSide != "none")
                {
                    launchingPanel.SetActive(false);
                    tutorialPanel.SetActive(true);

                    // init the side‐tutorial
                    leftCount = rightCount = 0;
                    prevSide = "";
                    leftCounterText.text  = $"0 / {targetCount}";
                    rightCounterText.text = $"0 / {targetCount}";

                    titleText.text       = "LET’S CALIBRATE YOUR MOVEMENTS!";
                    instructionText.text = "Move Left and Right to fill both bars.";

                    state = State.Sides;
                }
                break;

            case State.Sides:
                HandleSideTutorial(UDPReceiver.lastSide);
                break;

            case State.Jump:
                HandleJumpTutorial(UDPReceiver.lastAction);
                break;

            case State.ReadyToStart:
                instructionText.text =
                    "Good! Hold your hands together in front of your chest\nfor 3 seconds to start the game";

                if (UDPReceiver.lastAction == "praying")
                {
                    StartCoroutine(StartGameCountdown());
                    state = State.CountingDown;
                }
                break;

            case State.CountingDown:
                // nothing here; coroutine runs
                break;
        }
    }

    void HandleSideTutorial(string side)
    {
        if (side != prevSide)
        {
            if (side == "left"  && leftCount  < targetCount) leftCount++;
            if (side == "right" && rightCount < targetCount) rightCount++;
            prevSide = side;

            leftCounterText.text  = $"{leftCount} / {targetCount}";
            rightCounterText.text = $"{rightCount} / {targetCount}";
        }

        if (leftCount >= targetCount && rightCount >= targetCount)
        {
            state = State.Jump;
            leftCounterText.gameObject.SetActive(false);
            rightCounterText.gameObject.SetActive(false);

            jumpCount = 0;
            jumpCounterValueText.text = $"0 / {targetCount}";
            jumpCounterContainer.SetActive(true);

            titleText.text       = "JUMP TUTORIAL!";
            instructionText.text = "Perform 5 jumps!";
        }
    }

    void HandleJumpTutorial(string action)
    {
        if (action == "jump" && prevJump != "jump" && jumpCount < targetCount)
        {
            jumpCount++;
            jumpCounterValueText.text = $"{jumpCount} / {targetCount}";
        }
        prevJump = action;

        if (jumpCount >= targetCount)
        {
            state = State.ReadyToStart;
            titleText.text = "ALL SET!";
            jumpCounterContainer.SetActive(false);
        }
    }

    IEnumerator StartGameCountdown()
    {
        float t = startHoldSeconds;
        while (t > 0f)
        {
            instructionText.text = $"Starting in {Mathf.CeilToInt(t)}...";
            yield return null;
            t -= Time.deltaTime;
        }
        SceneManager.LoadScene(gameSceneName);
    }
}
