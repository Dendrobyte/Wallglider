using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public Rigidbody rb;
    public int ogMovementSpeed = 20;
    int currMovementSpeed;
    public float jumpYForce = 8f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    float rotationX, rotationY;

    private int MOVEMENT_SPEED_CONSTANT = 20;

    private Vector3 moveForce;
    private Vector3 jumpForce;

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

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
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
        Debug.Log("Force on enter: " + rb.velocity.ToString());
    }

    public void OnCollisionStay(Collision collisionInfo) {
        didJump = false;
        currColliderObj = collisionInfo.gameObject;
        currColliderContact = collisionInfo.contacts[0];
    }

    public void OnCollisionExit(Collision collisionInfo) {
        currColliderObj = null;
        Debug.Log("Force on exit: " + rb.velocity.ToString());
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
