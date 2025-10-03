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

    private bool isBoosting = false;
    
    private float m_currentboostCD = 5f;
    [SerializeField] private float m_boostCD = 5f;

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

    void EndBoost()
    {
        m_CarController.maxSpeed = m_StarterMaxBil;
        m_CarController.accelerationMultiplier = m_startaccalertion;
        isBoosting = false;
    }
    void Update()
    {
        if (m_CarController == null) return;

       
        if(Input.GetKeyDown(KeyCode.LeftShift) && m_boostTime > 0)
        {
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost; // sæt max speed op
            m_CarController.accelerationMultiplier = m_startaccalertion + m_accelerationSpeedBoost; // sæt accel op

            isBoosting = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift) )
        {
            EndBoost();
        }


        if (isBoosting)
        {
            m_boostTime = Mathf.Clamp(m_boostTime - Time.deltaTime, 0, m_maxBoostTime);
            m_currentboostCD = m_boostCD;
            if (m_RocketFire != null && !m_RocketFire.isPlaying) m_RocketFire.Play(true);
            if (m_boostTime <= 0)
            {
                EndBoost();
            }
        }
        else
        {
            m_currentboostCD = Mathf.Clamp(m_currentboostCD -  Time.deltaTime,0, m_boostCD);    // cooldown på boost

            if (m_RocketFire != null && m_RocketFire.isPlaying)
                m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // stop med det samme
            if (m_currentboostCD <= 0) // genoplad boost 
            {
                Debug.Log("BOOST GENOPLADES");
                m_boostTime = Mathf.Clamp(m_boostTime + Time.deltaTime, 0, m_maxBoostTime); // genoplad boost

            }
        }
        
        

    }
}
