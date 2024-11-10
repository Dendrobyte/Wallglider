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

    public bool didJump = false;
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
        
        startPosition = transform.position;
    }

    void Update() {
        // Jump
        if (Input.GetKeyDown("space")) {
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
                    Debug.Log("Player transform view: " + transform.forward);
                }

                rb.AddForce(jumpForce, ForceMode.Impulse);
                didJump = true;
            }
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
        // Generic keypress movement left/right/up/down
        if (Input.GetKeyDown("w") || Input.GetKey("w")){
            transform.position += transform.forward * Time.deltaTime * movementSpeed;
        }
        // if (Input.GetKeyDown("s") || Input.GetKey("s")){ // DENY ALL BACKWARD MOVEMENT
        //     transform.position -= transform.forward * Time.deltaTime * movementSpeed;
        // }
        if (Input.GetKeyDown("d") || Input.GetKey("d")){
            transform.position += transform.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKeyDown("a") || Input.GetKey("a")){
            transform.position -= transform.right * Time.deltaTime * movementSpeed;
        }

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
                //Vector3 boostVector = new Vector3(transform.forward.x, transform.forward.y * 20f, transform.forward.z);
                rb.AddForce(new Vector3(0f, 4f, 0f), ForceMode.VelocityChange);
                movementSpeed += 1;
            }
        } else {
            Debug.Log("Same wall, not resetting anything");
        }
        
        // TODO: When they hit the floor, reset to original position.
        //       I should also have a starting "floor" that's type WALL to help jump off of
        
    }
}
