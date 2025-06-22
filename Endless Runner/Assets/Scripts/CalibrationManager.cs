using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CalibrationManager : MonoBehaviour
{
    [Header("UI References")]
    public Text  titleText;
    public Text  leftCounterText;
    public Text  rightCounterText;

    [Header("Jump Counter UI")]
    public GameObject jumpCounterContainer;   // the Image container
    public Text       jumpCounterValueText;   // the "0 / 5" Text inside it

    public Text instructionText;

    [Header("Settings")]
    public int   targetCount       = 5;
    public float startHoldSeconds  = 3f;
    public string gameSceneName    = "Game";

    int leftCount   = 0;
    int rightCount  = 0;
    int jumpCount   = 0;

    string prevSide   = "";
    string prevJump   = "";

    enum State { Sides, Jump, ReadyToStart, CountingDown }
    State state = State.Sides;

    void Start()
    {
        titleText.text        = "LET’S CALIBRATE YOUR MOVEMENTS!";
        leftCounterText.text  = $"0 / {targetCount}";
        rightCounterText.text = $"0 / {targetCount}";

        jumpCounterContainer.SetActive(false);
        instructionText.text = "Move Left and Right to fill both bars.";
    }

    void Update()
    {
        string side   = UDPReceiver.lastSide;    // "left"/"right"/"none"
        string action = UDPReceiver.lastAction;  // "jump"/"stand"/"praying"/etc

        switch (state)
        {
            case State.Sides:
                HandleSideTutorial(side);
                break;

            case State.Jump:
                HandleJumpTutorial(action);
                break;

            case State.ReadyToStart:
                // Show the hold-hands prompt
                instructionText.text =
                    "Good! Hold your hands together in front of your chest\nfor 3 seconds to start the game";

                // As soon as we see the "praying" action, start the countdown
                if (action == "praying")
                {
                    StartCoroutine(StartGameCountdown());
                    state = State.CountingDown;
                }
                break;

            case State.CountingDown:
                // now in coroutine, nothing here
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
            // transition to Jump tutorial
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
