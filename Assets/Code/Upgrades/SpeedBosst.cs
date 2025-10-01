using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBosst : MonoBehaviour
{
    [Header("RocketSpeed")]
    [SerializeField] private int m_SpeedBoost = 1; // extra hastighed.
    [SerializeField] private float m_BoostCooldown = 1f; // cooldown
    [SerializeField] private float m_BoostDurationTime = 1f; // Duration tid.
    [SerializeField] private ParticleSystem m_RocketFire;
    [SerializeField] int accelerationSpeedBoost = 1;
    private int m_startaccalertion; 
    private int m_StarterMaxBil;
    [Header("Car Controller")]
    [SerializeField] private CarController m_CarController;
    [SerializeField] Rigidbody rb;

    private float  boostTime;
    [SerializeField] private float maxBoostTime = 10;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_startaccalertion = m_CarController.accelerationMultiplier;
        m_StarterMaxBil = m_CarController.maxSpeed;
        boostTime = maxBoostTime;
        m_RocketFire.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && boostTime > 0)
        {
            //tilføjere extra speed til spiller. 
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost;
            m_CarController.accelerationMultiplier = m_startaccalertion + accelerationSpeedBoost; 
            boostTime -= Time.deltaTime;
            m_RocketFire.Simulate(boostTime); // aktiver emision effect

        }
        else
        {
            m_CarController.maxSpeed = m_StarterMaxBil;
            m_CarController.accelerationMultiplier = m_startaccalertion;
            m_RocketFire.Stop();
            if (boostTime < 10)
            {
                boostTime += Time.deltaTime;

            }
        }

    }
}