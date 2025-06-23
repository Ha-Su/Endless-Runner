using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CalibrationManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchingPanel;
    public GameObject tutorialPanel;

    [Header("Tutorial UI References (inside TutorialPanel)")]
    public Text  titleText;
    public GameObject left;
    public GameObject right;
    public Text leftCounterText;
    public Text  rightCounterText;
    public GameObject jumpCounterContainer;
    public Text  jumpCounterValueText;
    public Text  instructionText;

    [Header("Start Button")]
    public Button startButton;     // Assign a UI Button here
    public Text   countdownText;   // A Text in the center to show "3", "2", "1"

    [Header("Settings")]
    public int   targetCount      = 5;
    public float startHoldSeconds = 3f;
    public string gameSceneName   = "Game";

    enum State { Launching, Sides, Jump, ReadyToStart, CountingDown }
    State state = State.Launching;

    int leftCount  = 0, rightCount = 0, jumpCount = 0;
    string prevSide = "", prevJump = "";

    void Start()
    {
        // Hook up button listener
        startButton.onClick.AddListener(OnStartButtonClicked);

        // Initial UI
        startButton.gameObject.SetActive(false);
        countdownText .gameObject.SetActive(false);

        if (UDPReceiver.isCalibrated)
            BeginTutorial();
        else
        {
            launchingPanel.SetActive(true);
            tutorialPanel .SetActive(false);
            state = State.Launching;
        }
    }

    void Update()
    {
        switch (state)
        {
            case State.Launching:
                if (UDPReceiver.isCalibrated || UDPReceiver.lastSide != "none")
                    BeginTutorial();
                break;

            case State.Sides:
                HandleSideTutorial(UDPReceiver.lastSide);
                break;

            case State.Jump:
                HandleJumpTutorial(UDPReceiver.lastAction);
                break;

            // State.ReadyToStart and CountingDown are driven by UI/coroutines
        }
    }

    void BeginTutorial()
    {
        launchingPanel.SetActive(false);
        tutorialPanel .SetActive(true);

        state = State.Sides;
        leftCount = rightCount = 0;
        prevSide = "";
        leftCounterText .text = $"0 / {targetCount}";
        rightCounterText.text = $"0 / {targetCount}";
        titleText       .text = "LET’S CALIBRATE YOUR MOVEMENTS!";
        instructionText .text = "Move Left and Right to fill both bars.";

        // ensure jump & start UI hidden
        jumpCounterContainer.SetActive(false);
        startButton.gameObject.SetActive(false);
        countdownText .gameObject.SetActive(false);
    }

    void HandleSideTutorial(string side)
    {
        if (side != prevSide)
        {
            if (side == "left"  && leftCount  < targetCount) leftCount++;
            if (side == "right" && rightCount < targetCount) rightCount++;
            prevSide = side;
            leftCounterText .text = $"{leftCount} / {targetCount}";
            rightCounterText.text = $"{rightCount} / {targetCount}";
        }

        if (leftCount >= targetCount && rightCount >= targetCount)
        {
            state = State.Jump;
            left.SetActive(false);
            right.SetActive(false);

            jumpCount = 0;
            jumpCounterValueText.text = $"0 / {targetCount}";
            jumpCounterContainer.SetActive(true);

            titleText .text = "JUMP TUTORIAL!";
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
            EnterReadyToStart();
        }
    }

    void EnterReadyToStart()
    {
        state = State.ReadyToStart;

        titleText .text = "ALL SET!";
        jumpCounterContainer.SetActive(false);

        // Show start button
        instructionText.text = "";
        startButton.gameObject.SetActive(true);
    }

    void OnStartButtonClicked()
    {
        // hide the button and start the countdown
        startButton.gameObject.SetActive(false);
        StartCoroutine(StartGameCountdown());
    }

    IEnumerator StartGameCountdown()
    {
        state = State.CountingDown;

        float t = startHoldSeconds;
        countdownText.gameObject.SetActive(true);

        while (t > 0f)
        {
            countdownText.text = Mathf.CeilToInt(t).ToString();
            yield return null;
            t -= Time.deltaTime;
        }

        // load main scene
        SceneManager.LoadScene(gameSceneName);
    }
}
