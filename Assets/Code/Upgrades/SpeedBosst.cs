using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)] // sikrer at vi sætter max/accel før CarController kører sin Update
public class SpeedBosst : MonoBehaviour
{
    [Header("Boost")]
    [SerializeField] private int m_SpeedBoost = 1; // hvor meget max speed skal op
    [SerializeField] private int m_accelerationSpeedBoost = 1; // hvor meget accel skal op
    [SerializeField] private float m_maxBoostTime = 10f;

    [Header("Refs")]
    [SerializeField] private CarController m_CarController;
    [SerializeField] private Rigidbody m_rb;
    [SerializeField] private ParticleSystem m_RocketFire;

    private float m_boostTime;
    private int m_startaccalertion;
    private int m_StarterMaxBil;

    private bool M_isBoosting = false;

    private float m_currentboostCD = 5f;
    [SerializeField] private float m_boostCD = 5f;

    // --- interne, kun til “rocket feel” (ændrer ikke dine serialized navne) ---
    const float kExtraAccelGround = 28f;   // m/s^2 – stærkt skub på jorden
    const float kExtraAccelAir = 10f;   // m/s^2 – mildere i luft
    //const float kExtraDownforceN = 2500f; // N – ekstra greb
    const float kOverspeedDragMul = 2.0f;  // proportional “bremsning” over base topfart
    const float kClampSlack = 1.02f; // lille slack, undgår hak ved grænsen

    void Awake()
    {
        if (!m_CarController) m_CarController = GetComponent<CarController>() ?? GetComponentInParent<CarController>();
        if (!m_rb) m_rb = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
    }

    void Start()
    {
        if (!m_CarController || !m_rb)
        {
            Debug.LogWarning("[SpeedBosst] Mangler CarController eller Rigidbody. Boost inaktivt.");
            enabled = false; return;
        }

        m_startaccalertion = m_CarController.accelerationMultiplier; // gem start accel
        m_StarterMaxBil = m_CarController.maxSpeed; // gem start max speed
        m_boostTime = m_maxBoostTime;

        if (m_RocketFire != null)
            m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDisable()
    {
        // fail-safe: rul tilbage hvis komponenten deaktiveres
        if (m_CarController)
        {
            m_CarController.maxSpeed = m_StarterMaxBil;
            m_CarController.accelerationMultiplier = m_startaccalertion;
        }
        M_isBoosting = false;
        if (m_RocketFire != null)
            m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void EndBoost()
    {
        m_CarController.maxSpeed = m_StarterMaxBil;
        m_CarController.accelerationMultiplier = m_startaccalertion;
        M_isBoosting = false;

        if (m_RocketFire != null && m_RocketFire.isPlaying)
            m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        if (m_CarController == null) return;

        // Start boost KUN hvis vi har tid tilbage
        if (Input.GetKeyDown(KeyCode.LeftShift) && m_boostTime > 0f)
        {
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost; // midlertidigt højere top
            m_CarController.accelerationMultiplier = m_startaccalertion + m_accelerationSpeedBoost; // ekstra accel
            M_isBoosting = true;

            if (m_RocketFire != null && !m_RocketFire.isPlaying) m_RocketFire.Play(true);
        }

        // Slip tasten → stop boost
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            EndBoost();
        }

        if (M_isBoosting)
        {
            // tøm boost-ressource
            m_boostTime = Mathf.Clamp(m_boostTime - Time.deltaTime, 0f, m_maxBoostTime);
            // “arm” cooldown mens vi bruger boost
            m_currentboostCD = m_boostCD;

            if (m_boostTime <= 0f)
                EndBoost();
        }
        else
        {
            // kør cooldown ned
            m_currentboostCD = Mathf.Clamp(m_currentboostCD - Time.deltaTime, 0f, m_boostCD);

            // stop fx når vi ikke booster
            if (m_RocketFire != null && m_RocketFire.isPlaying)
                m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // genoplad først når cooldown er færdig
            if (m_currentboostCD <= 0f)
            {
                m_boostTime = Mathf.Clamp(m_boostTime + Time.deltaTime, 0f, m_maxBoostTime);
            }
        }
    }

    void FixedUpdate()
    {
        if (!m_rb) return;

        if (M_isBoosting)
        {
            // brug den aktuelle (boostede) top som mål
            float targetTopMS = (m_CarController.maxSpeed / 3.6f);
            float speed = m_rb.velocity.magnitude;

            // Rocket push KUN mens vi booster
            if (speed < targetTopMS * kClampSlack)
            {
                float a = AnyWheelGrounded() ? kExtraAccelGround : kExtraAccelAir;
                m_rb.AddForce(transform.forward * a * m_rb.mass, ForceMode.Force);
            }

            // ekstra greb/stabilitet
            //m_rb.AddForce(-Vector3.up * kExtraDownforceN, ForceMode.Force);

            // hård clamp ved ekstreme spikes
            if (speed > targetTopMS)
                m_rb.velocity = m_rb.velocity.normalized * targetTopMS;
        }
        else
        {
            // *** VIGTIGT: ingen boost → ingen rocket push ***
            // Sørg for at man ikke kan holde boost-fart uden at booste:
            float baseTopMS = m_StarterMaxBil / 3.6f;
            float speed = m_rb.velocity.magnitude;

            if (speed > baseTopMS)
            {
                // glat proportional “drag” der trækker farten ned mod base topfart
                float overspeed = speed - baseTopMS;
                Vector3 drag = -m_rb.velocity.normalized * (overspeed * kOverspeedDragMul * m_rb.mass);
                m_rb.AddForce(drag, ForceMode.Force);

                // sikkerhed: hvis vi er ALT for højt over, clamp lidt tættere
                float hardCap = baseTopMS * 1.10f; // 10% slack
                if (m_rb.velocity.magnitude > hardCap)
                    m_rb.velocity = m_rb.velocity.normalized * hardCap;
            }

            // sikrer også at vores accel ikke “hænger” ved en fejl
            if (m_CarController.accelerationMultiplier != m_startaccalertion)
                m_CarController.accelerationMultiplier = m_startaccalertion;

            if (m_CarController.maxSpeed != m_StarterMaxBil)
                m_CarController.maxSpeed = m_StarterMaxBil;
        }
    }

    bool AnyWheelGrounded()
    {
        if (m_CarController == null) return true;
        return (m_CarController.frontLeftCollider && m_CarController.frontLeftCollider.isGrounded)
            || (m_CarController.frontRightCollider && m_CarController.frontRightCollider.isGrounded)
            || (m_CarController.rearLeftCollider && m_CarController.rearLeftCollider.isGrounded)
            || (m_CarController.rearRightCollider && m_CarController.rearRightCollider.isGrounded);
    }
}
