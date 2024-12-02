using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public Rigidbody rb;
    public int ogMovementSpeed = 10;
    int currMovementSpeed;
    public float jumpYForce = 8f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    float rotationX, rotationY;

    private int MOVEMENT_SPEED_CONSTANT = 20;

    private Vector3 moveForce;
    private Vector3 jumpForce;
    private Vector3 exitVelocity;

    private bool execJump = false; // To update in input and apply in physics
    private bool didJump = false; // To check and prevent double jumping

    GameObject currColliderObj;
    ContactPoint currColliderContact;

    public Camera playerCamera;

    // Start is called before the first frame update
    void Start() {
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        currMovementSpeed = ogMovementSpeed;
    }

    // Catch input updates here
    void Update() {
        // Clamp player rotation to mouse movement
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        rotationY += Input.GetAxis("Mouse X") * lookSpeed; // Only for the camera

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        // Update camera
        Quaternion camRotation = Quaternion.LookRotation(transform.forward, transform.up);
        playerCamera.transform.localRotation = camRotation;
        playerCamera.transform.position = transform.position;

        moveForce = Vector3.zero; // Avoid compounding

        // Generic keypress movement
        if (Input.GetKeyDown("w") || Input.GetKey("w")){
            moveForce += MOVEMENT_SPEED_CONSTANT*currMovementSpeed * Time.deltaTime * transform.forward;
        }
        if (Input.GetKeyDown("s") || Input.GetKey("s")){ // TODO: DENY ALL BACKWARD MOVEMENT
            moveForce -= MOVEMENT_SPEED_CONSTANT*currMovementSpeed * Time.deltaTime * transform.forward;
        }
        if (Input.GetKeyDown("d") || Input.GetKey("d")){
            moveForce += MOVEMENT_SPEED_CONSTANT*currMovementSpeed * Time.deltaTime * transform.right;
        }
        if (Input.GetKeyDown("a") || Input.GetKey("a")){
            moveForce -= MOVEMENT_SPEED_CONSTANT*currMovementSpeed * Time.deltaTime * transform.right;
        }
        if (Input.GetKeyDown("space")) {
            // TODO: if (didJump == false)
            execJump = true;
        }

        if (Input.GetKeyDown("escape")) {
            Cursor.visible = !Cursor.visible;
            if (Cursor.lockState == CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.None;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

    }

    // Apply any respective physics updates
    void FixedUpdate() {
        if (currColliderObj != null) {
            rb.AddForce(moveForce, ForceMode.VelocityChange);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 15f);
        }
        
        if (execJump) {
            executePlayerJump();
        }
    }

    public void OnCollisionEnter(Collision collisionInfo){
        currColliderObj = collisionInfo.gameObject;
        if (currColliderObj.GetComponent<JumpObjectType>().jumpType == JumpType.WALL) {
            // Rotate the player to be parallel to the collision
            Vector3 colliderNormal = currColliderContact.normal;
            Vector3 wallParallel = Vector3.Cross(colliderNormal, Vector3.up);
            Quaternion playerRotation = Quaternion.LookRotation(-colliderNormal, Vector3.up);
            transform.rotation = playerRotation;

            // This only works for perfectly straight walls
            // transform.rotation *= Quaternion.Euler(0, 90, 0);
            // playerCamera.transform.localRotation *= Quaternion.Euler(0, 90, 0);
        }
    }

    public void OnCollisionStay(Collision collisionInfo) {
        didJump = false;
        currColliderObj = collisionInfo.gameObject;
        currColliderContact = collisionInfo.contacts[0];


        // TODO (WORKING): Maintain velocity and force movement direction
        // NOTE: Moving "forward" through a map is always towards positive X
        //       In the demo map, moving "right" is negative Z and moving "left" is positive Z
        if (currColliderObj.GetComponent<JumpObjectType>().jumpType == JumpType.WALL) {
            Vector3 colliderNormal = currColliderContact.normal;
            Quaternion playerRotation = Quaternion.FromToRotation(transform.forward, -colliderNormal);
            // transform.rotation = playerRotation;
            // float rotationRadians = 0f;
            // if (colliderNormal.z >= 0) { // Colliding with a left wall
            //     rotationRadians = colliderNormal.x + transform.rotation.y;
            // } else if (colliderNormal.z < 0) { // Colliding with a right wall
            //     rotationRadians = colliderNormal.x - transform.rotation.y;
            // }
        
            // // Rotate the player by the calculating diff/sum of normal.x and their current viewing angle- around the y axis
            // transform.rotation *= Quaternion.Euler(0, Mathf.Rad2Deg * rotationRadians, 0);
        }


    }

    public void OnCollisionExit(Collision collisionInfo) {
        // We know that the force won't be adjusted after an exit, so this is the movement we want to stick with while colliding
        exitVelocity = rb.velocity;

        currColliderObj = null;
    }

    private void executePlayerJump() {
        execJump = false;
        didJump = true;
        if (currColliderObj == null) {
            jumpForce = new Vector3(0, jumpYForce, 0);
            rb.AddForce(jumpForce, ForceMode.Impulse);
            return;
        }
        JumpType currColliderJumpType = currColliderObj.GetComponent<JumpObjectType>().jumpType;
        if (currColliderJumpType == JumpType.FLOOR || currColliderJumpType == JumpType.START_PLANE) {
                // Some "RESET" function would be nice here
                jumpForce = new Vector3(0, jumpYForce, 0);
                currMovementSpeed = ogMovementSpeed;
        } else if (currColliderJumpType == JumpType.WALL) {
            // Vector3 jumpDir = Vector3.Reflect(transform.forward, currColliderContact.normal);
            // jumpForce = jumpDir * jumpYForce;
            Vector3 contactNormal = currColliderContact.normal;
            jumpForce = new Vector3(contactNormal.x*jumpYForce, jumpYForce, contactNormal.z*jumpYForce);
            // TODO: currMovementSpeed += (int)(.1 * currMovementSpeed);
        } else {
            Debug.Log("Unknown jump type of current collider... this is sus!");
            return;
        }

        Debug.Log("Jumping off of a " + currColliderJumpType + "in direction: " + jumpForce.ToString());
        rb.AddForce(jumpForce, ForceMode.Impulse);
    }
}
