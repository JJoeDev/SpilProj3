using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] WheelCollider m_frontRight;
    [SerializeField] WheelCollider m_frontLeft;
    [SerializeField] WheelCollider m_backRight;
    [SerializeField] WheelCollider m_backLeft;

    InputManager m_iManager;

    public float Acceleration = 500f;
    public float BreakingForce = 500f;
    public float maxTurnAngle = 20f;
    public float maxSpeed = 100f; // km/h
    public float jumpForce = 5000f; 

    private float m_currentAcceleration = 0f;
    private float m_currentBreakForce = 0f;
    private float m_currentTurnAngle = 0f;

    private Rigidbody m_rb;

    private void Start()
    {
        m_iManager = InputManager.Instance;
        m_rb = GetComponent<Rigidbody>();

        m_rb.centerOfMass = new Vector3(0, -0.1f, 0);
    }

    private void FixedUpdate()
    {
        // Get inputs from InputManager
        Vector2 input = m_iManager.OnMove();

        // Convert velocity magnitude to km/h
        float speed = m_rb.velocity.magnitude * 3.6f;

        // Acceleration (only if under max speed OR braking/reversing)
        if (speed < maxSpeed || input.y < 0f)
        {
            m_currentAcceleration = Acceleration * input.y;
        }
        else
        {
            m_currentAcceleration = 0f;
        }

        // Braking
        if (m_iManager.OnHandBreak().ReadValue<float>() > 0.1f)
        {
            m_currentBreakForce = BreakingForce; // full brake
        }
        else if (Mathf.Approximately(input.y, 0f))
        {
            m_currentBreakForce = BreakingForce * 0.3f; // engine braking
        }
        else
        {
            m_currentBreakForce = 0f;
        }

        // Apply acceleration (currently front wheel drive)
        m_frontRight.motorTorque = m_currentAcceleration;
        m_frontLeft.motorTorque = m_currentAcceleration;

        // Apply brakes to all wheels
        m_frontRight.brakeTorque = m_currentBreakForce;
        m_frontLeft.brakeTorque = m_currentBreakForce;
        m_backRight.brakeTorque = m_currentBreakForce;
        m_backLeft.brakeTorque = m_currentBreakForce;

        // Steering
        m_currentTurnAngle = -maxTurnAngle * input.x;
        m_frontLeft.steerAngle = m_currentTurnAngle;
        m_frontRight.steerAngle = m_currentTurnAngle;

        // Jump
        if (m_iManager.OnJump().WasPressedThisFrame() && IsGrounded())
        {
            m_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        float wheelRayLength = 0.2f; // slightly longer than suspension
        return Physics.Raycast(m_frontLeft.transform.position, -transform.up, wheelRayLength) ||
               Physics.Raycast(m_frontRight.transform.position, -transform.up, wheelRayLength) ||
               Physics.Raycast(m_backLeft.transform.position, -transform.up, wheelRayLength) ||
               Physics.Raycast(m_backRight.transform.position, -transform.up, wheelRayLength);
    }

}
