using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    [SerializeField] InputManager input;

    public float Acceleration = 500f;
    public float BreakingForce = 300f;
    public float maxTurnAngle = 15f;

    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;

    private void FixedUpdate()
    {
        // Get inputs from InputManager
        float verticalInput = input.OnMove().y;
        float horizontalInput = input.OnMove().x;

        // Acceleration
        currentAcceleration = Acceleration * verticalInput;

        // Braking
        if (input.OnHandBreak().ReadValue<float>() > 0.1f)
        {
            currentBreakForce = BreakingForce; // full brake
        }
        else if (Mathf.Approximately(verticalInput, 0f))
        {
            currentBreakForce = BreakingForce * 0.3f; // engine braking
        }
        else
        {
            currentBreakForce = 0f;
        }
        
        // Apply acceleration (currently front wheel drive)
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        // Apply brakes to all wheels
        frontRight.brakeTorque = currentBreakForce;
        frontLeft.brakeTorque = currentBreakForce;
        backRight.brakeTorque = currentBreakForce;
        backLeft.brakeTorque = currentBreakForce;

        // Steering
        currentTurnAngle = -maxTurnAngle * horizontalInput;
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;
    }
}
