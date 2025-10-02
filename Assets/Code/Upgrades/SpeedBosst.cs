using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)] // sikrer at vi sætter max/accel før CarController kører sin Update
public class SpeedBosst : MonoBehaviour
{
    [Header("Boost")]
    [SerializeField] private int m_SpeedBoost = 1;
    [SerializeField] private int m_accelerationSpeedBoost = 1;
    [SerializeField] private float m_maxBoostTime = 10f;

    [Header("Refs")]
    [SerializeField] private CarController m_CarController;
    [SerializeField] private Rigidbody m_rb;
    [SerializeField] private ParticleSystem m_RocketFire;

    [Header("Fysik-hjælp")]
    [SerializeField] private float m_physicsBoostAccel = 15f; // ekstra fremad under boost (nu både jord+luft)
    [SerializeField] private float m_extraDownForce = 25f;    // ekstra nedad når i luften (altid)

    private float m_boostTime;
    private int m_startaccalertion;
    private int m_StarterMaxBil;

    void Start()
    {
        if (!m_rb) m_rb = GetComponent<Rigidbody>();
        if (!m_rb && m_CarController) m_rb = m_CarController.GetComponent<Rigidbody>();

        m_startaccalertion = m_CarController.accelerationMultiplier;
        m_StarterMaxBil = m_CarController.maxSpeed;
        m_boostTime = m_maxBoostTime;

        if (m_RocketFire != null)
            m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        if (m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;

        if (boosting)
        {
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost;
            m_CarController.accelerationMultiplier = m_startaccalertion + m_accelerationSpeedBoost;

            m_boostTime -= Time.deltaTime;
            if (m_boostTime < 0f) m_boostTime = 0f;

            if (m_RocketFire != null && !m_RocketFire.isPlaying) m_RocketFire.Play(true);
        }
        else
        {
            m_CarController.maxSpeed = m_StarterMaxBil;
            m_CarController.accelerationMultiplier = m_startaccalertion;

            if (m_RocketFire != null && m_RocketFire.isPlaying)
                m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (m_boostTime < m_maxBoostTime)
            {
                m_boostTime += Time.deltaTime;
                if (m_boostTime > m_maxBoostTime) m_boostTime = m_maxBoostTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (m_rb == null || m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;
        bool grounded = IsGrounded();

        if (!grounded)
            m_rb.AddForce(Vector3.down * m_extraDownForce, ForceMode.Acceleration);

        if (boosting)
            m_rb.AddForce(transform.forward * m_physicsBoostAccel, ForceMode.Acceleration);
    }

    bool IsGrounded()
    {
        if (m_CarController.frontLeftCollider && m_CarController.frontLeftCollider.isGrounded) return true;
        if (m_CarController.frontRightCollider && m_CarController.frontRightCollider.isGrounded) return true;
        if (m_CarController.rearLeftCollider && m_CarController.rearLeftCollider.isGrounded) return true;
        if (m_CarController.rearRightCollider && m_CarController.rearRightCollider.isGrounded) return true;

        Ray ray = new Ray(transform.position + Vector3.up * 0.2f, Vector3.down);
        return Physics.Raycast(ray, 1.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }
}
