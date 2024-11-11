using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [Range(0, 40)]
    public int movementSpeed = 5;

    // TODO: Is there a way we can make these a variable based on the player size?
    //       Something to come back to if we flesh this out.
    [Range(1, 10)]
    public int jumpHeight = 10;

    [Range(1, 10)]
    public int jumpSpeed = 2;

    [Range(0.0f, 2.0f)]
    public int jumpDrag = 1;
    public bool jumpRequest = false;

    public bool didJump = false;
    private bool isCollidingWithWall = false;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public Rigidbody rb;
    public GameObject currColliderGameObj;
    private Vector3 jumpDir;
    
    public Camera playerCamera;
    private Vector3 startPosition;

    float rotationX;
    // Start is called before the first frame update
    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        startPosition = transform.position;
    }

    void Update() {
        // Jumping - Trigger request to be processed in fix update
        if (Input.GetKeyDown("space")) {
            jumpRequest = true;
        }    

        // Clamp forward direction
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        if (Input.GetKeyDown("escape")) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (jumpRequest) {
            // Handling jump so that we properly apply physics
            if (didJump == false) {
                // Calculate direction for jump
                JumpType currColliderType = currColliderGameObj.GetComponent<JumpObjectType>().jumpType;
                Vector3 jumpForce = new Vector3();
                if (currColliderType == JumpType.FLOOR || currColliderType == JumpType.START_PLANE) {
                    jumpForce = new Vector3(0.0f, .25f * jumpHeight, 0.0f);
                } else if (currColliderType == JumpType.WALL) {
                    rb.drag = jumpDrag;
                    // Get the inverse of the direction that the wall faces
                    jumpForce = new Vector3(jumpDir.x*jumpSpeed, .25f * jumpDir.y + jumpHeight, jumpDir.z*jumpSpeed);
                    // Debug.Log("Jumped off wall with direction of: " + jumpDir);
                    // Debug.Log("Jump force vector: " + jumpForce);
                }

                rb.AddForce(jumpForce, ForceMode.Impulse);
                didJump = true;
            }
            jumpRequest = false;
        }

        // Only let them move if they're not colliding with a wall
        Vector3 newPos = transform.position;
        if (!isCollidingWithWall) {
            // Generic keypress movement left/right/up/down
            if (Input.GetKeyDown("w") || Input.GetKey("w")){
                newPos += movementSpeed * Time.deltaTime * transform.forward;
            }
            // if (Input.GetKeyDown("s") || Input.GetKey("s")){ // DENY ALL BACKWARD MOVEMENT
            //     transform.position -= transform.forward * Time.deltaTime * movementSpeed;
            // }
            if (Input.GetKeyDown("d") || Input.GetKey("d")){
                newPos += movementSpeed * Time.deltaTime * transform.right;
            }
            if (Input.GetKeyDown("a") || Input.GetKey("a")){
                newPos -= movementSpeed * Time.deltaTime * transform.right;
            }
        } else {
            newPos += movementSpeed * Time.deltaTime * transform.forward;
        }

        rb.MovePosition(newPos);

    }

    public void OnCollisionEnter(Collision collisionInfo) {
        if (currColliderGameObj.GetComponent<JumpObjectType>().jumpType == JumpType.START_PLANE) {
            didJump = false;
        }

        // If the wall is new, we can reset and do boost stuff, etc.
        if (!ReferenceEquals(collisionInfo.gameObject, currColliderGameObj)) {
            didJump = false;

            currColliderGameObj = collisionInfo.gameObject;
            jumpDir = collisionInfo.contacts[0].normal;
            if (currColliderGameObj.GetComponent<JumpObjectType>().jumpType == JumpType.FLOOR) {
                // Some "RESET" function would be nice here
                transform.position = startPosition;
                movementSpeed = 5;
            } else if (currColliderGameObj.GetComponent<JumpObjectType>().jumpType == JumpType.WALL) {
                // Amplify the forward vector
                //Vector3 boostVector = new Vector3(transform.TransformDirection.x * 2f, transform.forward.y * 20f, transform.forward.z);
                //rb.AddForce(new Vector3(0f, 20f, 0f), ForceMode.VelocityChange);
                isCollidingWithWall = true;
                movementSpeed += 1;
                Debug.Log("Hit a wall.");
            }
        }
        
        // TODO: When they hit the floor, reset to original position.
        //       I should also have a starting "floor" that's type WALL to help jump off of
        
    }
    
    // public void OnCollisionStay(Collision collisionInfo) {
        //     // Every time we hit a wall, we want to make sure we nullify any movement in the direction of the wall
        // if (currColliderGameObj.GetComponent<JumpObjectType>().jumpType == JumpType.WALL) {
        //     Quaternion newRotation = transform.rotation;
        //     // We set the player's z ("forward") to the object's X (local axis alignment)
        //     newRotation.z = currColliderGameObj.transform.rotation.x;
        //     transform.rotation = newRotation;
        // }
    // }

    public void OnCollisionExit(Collision collisionInfo) {
        Debug.Log("Exiting.");
        isCollidingWithWall = false;
    }
}
