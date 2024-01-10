using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class TPSCamera : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10.0f;
    //public float gravity = 20.0f;
    public float gravityMultiplier = 2f;
    public float terminalVelocity = 10f;
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    public float FallingThreshold = -0.01f;  //Adjust in inspector to appropriate value for the speed you want to trigger detecting a fall, probably by just testing (use negative numbers probably)
    [HideInInspector]

    bool isGrounded;

    CharacterController characterController;
    Rigidbody rb;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;
    public Animator anim;


    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        rotation.y = transform.eulerAngles.y;
        Cursor.visible = false;
    }

    //bool IsGrounded()
    //{
    //    return Physics.Raycast(transform.position, -Vector3.up, 1.1f);
    //}

    void Update()
    {

        // Check if the player is on the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.12f);

        // Player movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 cameraForward = playerCameraParent.forward;
        Vector3 cameraRight = playerCameraParent.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        moveDirection = (cameraForward.normalized * verticalInput + cameraRight.normalized * horizontalInput).normalized;
        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
        }
    }
    private void FixedUpdate()
    {
        //moveDirection = new Vector3(horizontalInput, 0f, verticalInput);
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

        // Player jump
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            Debug.Log("Jumping");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

        // Limit the terminal velocity to avoid unrealistic speeds
        if (rb.velocity.y < -terminalVelocity)
        {
            rb.velocity = new Vector3(rb.velocity.x, -terminalVelocity, rb.velocity.z);
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        //moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        //characterController.Move(moveDirection * Time.deltaTime);
        //rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);

        // Player and Camera rotation
    }
}