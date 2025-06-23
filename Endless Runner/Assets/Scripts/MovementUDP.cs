using UnityEngine;

public class CharacterControllerUDP : MonoBehaviour
{
    [Header("Lane Positions")]
    public Vector3 leftLane  = new Vector3(-25f, -1.8f, -1f);
    public Vector3 rightLane = new Vector3(-25f, -1.8f, -7.25f);

    [Header("Movement Settings")]
    public float laneChangeSpeed = 5f;   // Speed of shifting between lanes
    public float jumpForce        = 5f;  // Jump impulse
    public float gravity          = -9.8f;

    [Header("Animation")]
    public Animator animator;

    private Vector3 velocity;
    private bool    isGrounded = true;
    private Vector3 targetPosition;

    void Start()
    {
        // Start in left lane
        transform.position = leftLane;
        targetPosition = leftLane;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Read your inputs / UDP data
        string action = UDPReceiver.lastAction;
        string side = UDPReceiver.lastSide;

        // Fallback to keyboard for testing
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            side = "left";
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            side = "right";

        if (Input.GetKeyDown(KeyCode.Space))
            action = "jump";

        // Set which lane we want to move to
        if (side == "left")
            targetPosition = leftLane;
        else if (side == "right")
            targetPosition = rightLane;

        // Smoothly slide character toward the target lane
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetPosition.x, transform.position.y, targetPosition.z),
            laneChangeSpeed * Time.deltaTime
        );

        // Jump logic
        if (isGrounded && action == "jump")
        {
            velocity.y = jumpForce;
            isGrounded = false;
            animator.SetTrigger("Jump");
        }

        // Apply gravity and vertical movement
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            transform.position += new Vector3(0f, velocity.y * Time.deltaTime, 0f);

            // Simple ground check at y = 0.5
            if (transform.position.y <= -1.8f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    -1.8f,
                    transform.position.z
                );
                velocity.y = 0f;
                isGrounded = true;
            }
        }
        float runParam = isGrounded ? 1f : 0f;
        animator.SetFloat("MoveSpeed", runParam);

        animator.SetBool("Grounded", isGrounded);
    }
}
