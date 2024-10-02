using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] public float turnSpeed = 180f;

    private Rigidbody rb; 
    private Animator animator; // Reference to the animator
    private Vector3 moveInput;     // Stores the player's input for movement
    private Vector3 moveVelocity; //stores player's velocity for movement

    private float gravityScale = 10f;
    private bool isGrounded; // Flag to track if the player is grounded
    
    //Variables all here for easier code modification
    private static readonly int hashStartWalkButton = Animator.StringToHash("StartWalking"); //converts the trigger to int for better perfomance 
    private static readonly int hashStopWalkButton = Animator.StringToHash("StopWalking"); //converts the trigger to int for better perfomance

    private PlayerStats playerStats;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player object
        animator = GetComponent<Animator>();   
        animator.SetTrigger(hashStopWalkButton);  //by default character is not moving
        playerStats = GetComponent<PlayerStats>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //freeze rotation on x and z
    }

    
    void Update()
    {

        //deltaTime is used to make it frame independent
        float moveZ = Input.GetAxis("Vertical") * playerStats.moveSpeed * Time.deltaTime;
        
        //deltaTime is used to make it frame independent
        float rotationY = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime; 

        // Set the walking animation
        if (moveZ != 0){
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk_Cycle"))
            {
                animator.SetTrigger(hashStartWalkButton);
            }
        }else{
            animator.SetTrigger(hashStopWalkButton);
        }

        // Move the player
        transform.Translate(Vector3.forward * moveZ);
        transform.Rotate(Vector3.up * rotationY);

    }

    //The FixedUpdate() method is called every fixed framerate frame, and it's used for physics calculations.
    void FixedUpdate()
    {
        isGrounded = IsGrounded();
        // Apply additional gravity to make the player fall faster
        rb.AddForce(Vector3.down * Physics.gravity.magnitude * gravityScale, ForceMode.Acceleration);
    }

    bool IsGrounded(){
        RaycastHit hit;
        float raycastDistance = 0.1f; // Adjust this distance based on your player's size

        // Cast a ray downwards from the player's position
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance)){
            // Check if the object hit by the ray has the tag "Terrain"
            if (hit.collider.CompareTag("Terrain")){
                return true; // Player is grounded
            }
        }

        return false; // Player is not grounded
    }

}
