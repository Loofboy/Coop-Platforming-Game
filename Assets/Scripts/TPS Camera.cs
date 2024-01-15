using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

//[RequireComponent(typeof(CharacterController))]

public class TPSCamera : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Name = new NetworkVariable<FixedString32Bytes>("Default", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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

    Rigidbody rb;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;
    public Animator Anim;
    public TextMeshProUGUI nametext;

    bool jumping;
    bool falling;


    [HideInInspector]
    public bool canMove = true;

    private void Start()
    {
        //if (!IsOwner) return;
        rb = GetComponent<Rigidbody>();
        rotation.y = transform.eulerAngles.y;
        Cursor.visible = false;

        Debug.Log(GameObject.Find("Namefield").transform.GetChild(0).GetChild(2));
        Name.Value = GameObject.Find("Namefield").transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text; 
        nametext.text = Name.Value.ToString();

        GameObject.Find("UICanvas").SetActive(false);
    }
    public override void OnNetworkSpawn()
    {

    }



    //bool IsGrounded()
    //{
    //    return Physics.Raycast(transform.position, -Vector3.up, 1.1f);
    //}

    void Update()
    {
        if (!IsOwner) return;
        if (!playerCameraParent.GetChild(0).gameObject.activeInHierarchy)
        {
            playerCameraParent.GetChild(0).gameObject.SetActive(true);
        }
        // Check if the player is on the ground
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.12f);

        // Player movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float fh;
        int hor = (fh = horizontalInput) > 0 ? 1 : fh < 0 ? -1 : 0;
        Anim.SetInteger("HorMove", hor);

        float fv;
        int ver = (fv = verticalInput) > 0 ? 1 : fv < 0 ? -1 : 0;
        Anim.SetInteger("VertMove", ver);

        Vector3 cameraForward = Camera.main.transform.TransformDirection(Vector3.forward);
        Vector3 cameraRight = Camera.main.transform.TransformDirection(Vector3.right);

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        Vector3 desiredMoveDirection = cameraForward.normalized * verticalInput + cameraRight.normalized * horizontalInput;
        moveDirection = desiredMoveDirection.normalized * moveSpeed * Time.deltaTime;

        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
            transform.Translate(moveDirection);
        }

        if (isGrounded && Input.GetButtonDown("Jump") && canMove)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            Anim.SetBool("Jumping", true);
            jumping = true;
            Pause(0.05f);
            //Anim.SetBool("midjump", true);
        }

        if (rb.velocity.y > 0 && !falling)
        {
            jumping = true;
            Anim.SetBool("Jumping", true);
            Anim.SetBool("midjump", true);
            Anim.SetBool("Fall", false);
        }
        else if (rb.velocity.y < -0.2 && !jumping)
        {
            falling = true;
            Anim.SetBool("Jumping", false);
            Anim.SetBool("Fall", true);
        }
        else if (isGrounded)
        {
            falling = false;
            jumping = false;
            Anim.SetTrigger("Land");
            Anim.SetBool("Fall", false);
            Anim.SetBool("Jumping", false);
            Anim.SetBool("midjump", false);
        }
    }
    IEnumerator Pause(float time)
    {
        yield return new WaitForSeconds(time);
        Anim.SetBool("midjump", true);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && rb.velocity.y <= 0.1 && rb.velocity.y >= -0.1)
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Anim.ResetTrigger("Land");
            isGrounded = false;
        }
    }
    //private void FixedUpdate()
    //{
    //moveDirection = new Vector3(horizontalInput, 0f, verticalInput);
    //rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

    // Player jump
    //if (isGrounded && Input.GetButtonDown("Jump"))
    //{
    //    Debug.Log("Jumping");
    //    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    //}
    //rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);

    // Limit the terminal velocity to avoid unrealistic speeds
    //if (rb.velocity.y < -terminalVelocity)
    //{
    //    rb.velocity = new Vector3(rb.velocity.x, -terminalVelocity, rb.velocity.z);
    //}

    // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
    // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
    // as an acceleration (ms^-2)
    //moveDirection.y -= gravity * Time.deltaTime;

    // Move the controller
    //characterController.Move(moveDirection * Time.deltaTime);
    //rb.MovePosition(transform.position + moveDirection * speed * Time.deltaTime);

    // Player and Camera rotation
    //}
}