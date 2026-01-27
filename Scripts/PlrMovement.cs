using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Experimental;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlrMovement : NetworkBehaviour
{
   
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;
    public float sprintingSpeed = 8f;
    float normalSpeed;

   
   
    public LayerMask groundMask;

    private Rigidbody rb;
    public GameObject playerCamera;

    public GameObject Scoreboardgmobj;

    private float pitch;
    private float yaw;
    private bool isGrounded;
    private bool isSprinting = false;
    bool isSliding = false;
    public float SlidingForce = 20f;

    void Awake()
    {
        normalSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

       // Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (playerCamera != null)
            playerCamera.SetActive(false);

        if (IsOwner)
        {
            rb.isKinematic = false;
            EnableLocalCamera(true);
        }
        else
        {
            rb.isKinematic = true;
            EnableLocalCamera(false);
        }
    }

    public override void OnNetworkDespawn()
    {

        EnableLocalCamera(false);
        base.OnNetworkDespawn();
    }

    void EnableLocalCamera(bool enabled)
    {
        if (playerCamera != null)
        {
            playerCamera.SetActive(enabled);


            var listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null)
                listener.enabled = enabled;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AblitiesMovementServerRpc(int whatabl)
    {
        switch (whatabl)
        {
            case 1://flash
                moveSpeed = 20;
                normalSpeed = moveSpeed;
                sprintingSpeed = moveSpeed + 5;
            break;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleLook();
        
        if (Input.GetButtonDown("Jump")) HandleJump();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = sprintingSpeed;
            isSprinting = true;

        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && isSprinting && !isSliding)
        {
           
            
         //sliding kiedys

           
            isSliding = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isSliding = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
            moveSpeed = normalSpeed;
        }


        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ScoreBoardShow(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ScoreBoardShow(false);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ziemia")) isGrounded = true;
    }

    void ScoreBoardShow(bool shw)
    {
        
        Scoreboardgmobj.SetActive(shw);
    }



    void FixedUpdate()
    {
        if (!IsOwner) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 frwrd = playerCamera.transform.forward;
        Vector3 rght = playerCamera.transform.right;

        frwrd.y = 0;
        rght.y = 0;

        Vector3 moveDir = (rght * h + frwrd * v).normalized;
        Vector3 targetVel = moveDir * moveSpeed;
        Vector3 currentVel = rb.velocity;

        Vector3 velocityChange = new Vector3(
            targetVel.x - currentVel.x,
            0f,
            targetVel.z - currentVel.z
        );

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void HandleJump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void CheckGround()
    {
        
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
