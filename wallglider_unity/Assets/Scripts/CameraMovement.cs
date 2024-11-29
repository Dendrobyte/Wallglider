using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    float rotationX, rotationY;
    float lookSpeed;
    float lookXLimit, lookYLimit;

    public Movement playerModel;
    void Start() {
        // Set the same speeds as the player model's movement
        lookSpeed = GetComponentInParent<Movement>().lookSpeed;
        lookXLimit = GetComponentInParent<Movement>().lookXLimit;
        lookYLimit = lookXLimit;
        transform.position = playerModel.transform.position;
    }

    // Update is called once per frame
    void Update() {

        // transform.localRotation = playerModel.transform.localRotation;
        transform.position = playerModel.transform.position;

    }
}
