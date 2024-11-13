using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public Rigidbody rb;
    public int movementSpeed = 5;
    public float jumpForce = 8f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    float rotationX;

    private Vector3 moveForce;

    private bool execJump = false; // To update in input and apply in physics
    private bool didJump = false; // To check and prevent double jumping


    public Camera playerCamera;

    // Start is called before the first frame update
    void Start() {
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Catch input updates here
    void Update() {
        // Clamp player rotation to mouse movement
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        //playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        moveForce = Vector3.zero; // Avoid compounding

        // Generic keypress movement
        if (Input.GetKeyDown("w") || Input.GetKey("w")){
            moveForce += movementSpeed * Time.deltaTime * transform.forward;
        }
        if (Input.GetKeyDown("s") || Input.GetKey("s")){ // TODO: DENY ALL BACKWARD MOVEMENT
            moveForce -= movementSpeed * Time.deltaTime * transform.forward;
        }
        if (Input.GetKeyDown("d") || Input.GetKey("d")){
            moveForce += movementSpeed * Time.deltaTime * transform.right;
        }
        if (Input.GetKeyDown("a") || Input.GetKey("a")){
            moveForce -= movementSpeed * Time.deltaTime * transform.right;
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
        rb.AddForce(moveForce, ForceMode.VelocityChange);
        if (execJump) {
            execJump = false;
            didJump = true;
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }
}
