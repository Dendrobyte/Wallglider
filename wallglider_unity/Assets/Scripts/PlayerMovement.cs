using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public int movementSpeed = 5;
    public int jumpHeight = 5;
    public bool didJump = false;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public Rigidbody rb;
    public GameObject currColliderGameObj;
    private Vector3 jumpDir;
    public Camera playerCamera;

    float rotationX;
    // Start is called before the first frame update
    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    void Update() {
        // Jump
        if (Input.GetKeyDown("space")) {
            if (didJump == false) {
                // Calculate direction for jump
                JumpType currColliderType = currColliderGameObj.GetComponent<JumpObjectType>().jumpType;
                Vector3 jumpForce = new Vector3();
                if (currColliderType == JumpType.FLOOR) {
                    jumpForce = new Vector3(0.0f, jumpHeight, 0.0f);
                } else if (currColliderType == JumpType.WALL) {
                    // Get the inverse of the direction that the wall faces
                    jumpForce = new Vector3(jumpDir.x*jumpHeight, jumpDir.y+jumpHeight, jumpDir.z*jumpHeight);
                    Debug.Log("Jumped off wall with direction of: " + jumpDir);
                    Debug.Log("Jump force vector: " + jumpForce);
                }

                rb.AddForce(jumpForce, ForceMode.Impulse);
                didJump = true;
            }
        }

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
        if (Input.GetKeyDown("s") || Input.GetKey("s")){
            transform.position -= transform.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKeyDown("d") || Input.GetKey("d")){
            transform.position += transform.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKeyDown("a") || Input.GetKey("a")){
            transform.position -= transform.right * Time.deltaTime * movementSpeed;
        }

    }

    public void OnCollisionEnter(Collision collisionInfo) {
        didJump = false;

        currColliderGameObj = collisionInfo.gameObject;
        jumpDir = collisionInfo.contacts[0].normal;
    }
}
