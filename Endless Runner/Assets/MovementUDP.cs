using UnityEngine;

public class CharacterControllerUDP : MonoBehaviour
{
    public float laneOffset = 2f;        // Distance between center and side lanes on Z-axis
    public float laneChangeSpeed = 5f;   // Speed of shifting between lanes
    public float jumpForce = 5f;         // Jump height
    public float gravity = -9.8f;

    private Vector3 velocity;
    private bool isGrounded = true;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        string action = UDPReceiver.lastAction;
        string side = UDPReceiver.lastSide;

        if (Input.GetKeyDown(KeyCode.LeftArrow)){
            side = "left";
            Debug.Log("LEFT!");
        }
            
        else if (Input.GetKeyDown(KeyCode.A))
            side = "left";
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            side = "right";
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            side = "center";

        if (Input.GetKeyDown(KeyCode.Space))
            action = "jump";

        // Move left/right (on Z-axis!)
        if (side == "left" )
            targetPosition.z = laneOffset;
        else if (side == "right")
            targetPosition.z = -laneOffset;
        else
            targetPosition.z = 0f;

        // Smoothly slide character to target Z-position
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(transform.position.x, transform.position.y, targetPosition.z),
            laneChangeSpeed * Time.deltaTime
        );

        // Jump logic
        if (isGrounded && action == "jump")
        {
            velocity.y = jumpForce;
            isGrounded = false;
        }

        // Apply gravity
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);

            float groundY = 0.5f;
            if (transform.position.y <= groundY)
            {
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                velocity.y = 0f;
                isGrounded = true;
            }
        }
    }
}
