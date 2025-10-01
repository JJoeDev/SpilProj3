using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)] // kør før CarController.Update
public class SpeedBosst : MonoBehaviour
{
    [Header("Boost")]
    [SerializeField] private int m_SpeedBoost = 1;               // Hvor meget vi løfter bilens tophastighed, mens der boostes.
    [SerializeField] private int m_accelerationSpeedBoost = 1;   // Hvor meget ekstra acceleration bilen får, mens der boostes.
    [SerializeField] private float m_maxBoostTime = 10f;          // Den samlede “energi” til boost: tælles ned når aktiv, op når inaktiv.

    [Header("Refs")]
    [SerializeField] private CarController m_CarController;       // Bilens controller, som læser maxSpeed og accelerationMultiplier.
    [SerializeField] private Rigidbody m_rb;                      // Bilens rigidbody til fysisk skub.
    [SerializeField] private ParticleSystem m_RocketFire;         // Visuel effekt, der kører mens boost er aktivt.

    [Header("Fysik-hjælp")]
    [SerializeField] private float m_physicsBoostAccel = 15f;     // Et ekstra fremad-skub, men kun når hjulene rører jorden.
    [SerializeField] private float m_extraDownForce = 25f;      // Et ekstra nedad-skub, når bilen er i luften, så den falder hurtigere.

    // Intern tilstand: vi gemmer bilens normale værdier og den aktuelle mængde boost-energi.
    private float m_boostTime;
    private int m_startaccalertion;
    private int m_StarterMaxBil;

    void Start()
    {
        // Vi sørger for at have en rigidbody-reference, og gemmer bilens standard-acceleration og -tophastighed,
        // så vi kan sætte dem tilbage, når boost ikke er aktivt. Vi fylder også boost-energien helt op ved start.
        if (!m_rb) m_rb = GetComponent<Rigidbody>();
        if (!m_rb && m_CarController) m_rb = m_CarController.GetComponent<Rigidbody>();

        m_startaccalertion = m_CarController.accelerationMultiplier;
        m_StarterMaxBil = m_CarController.maxSpeed;
        m_boostTime = m_maxBoostTime;

        // Effekten er slukket fra start, så den kun ses, når spilleren faktisk booster.
        if (m_RocketFire != null)
            m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        // Hele logikken her handler om at hæve bilens grænser, mens spilleren holder Shift, og samtidig
        // trække på boost-energien. Når Shift ikke holdes, sætter vi værdierne tilbage og lader energien op igen.
        if (m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;

        if (boosting)
        {
            // Vi hæver tophastighed og acceleration, så CarController tillader hurtigere kørsel og hurtigere optræk.
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost;
            m_CarController.accelerationMultiplier = m_startaccalertion + m_accelerationSpeedBoost;

            // Boost-energien bruges op mens der boostes; når den rammer 0, stopper boost.
            m_boostTime -= Time.deltaTime;
            if (m_boostTime < 0f) m_boostTime = 0f;

            // Den visuelle effekt følger boost-tilstanden.
            if (m_RocketFire != null && !m_RocketFire.isPlaying)
                m_RocketFire.Play(true);
        }
        else
        {
            // Når der ikke boostes, fører vi bilen tilbage til dens normale grænser.
            m_CarController.maxSpeed = m_StarterMaxBil;
            m_CarController.accelerationMultiplier = m_startaccalertion;

            // Effekten stopper og ryddes for at undgå efterglød.
            if (m_RocketFire != null && m_RocketFire.isPlaying)
                m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Boost-energien lader langsomt op igen, så spilleren kan booste senere.
            if (m_boostTime < m_maxBoostTime)
            {
                m_boostTime += Time.deltaTime;
                if (m_boostTime > m_maxBoostTime) m_boostTime = m_maxBoostTime;
            }
        }
    }

    void FixedUpdate()
    {
        // I fysik-opdateringen afgør vi, om hjulene rører jorden. Kun der giver det mening at skubbe fremad.
        if (m_rb == null || m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;
        bool grounded = IsGrounded();

        // På jorden: et lille ekstra skub i bilens fremad-retning gør, at man hurtigere når den nye tophastighed.
        if (boosting && grounded)
        {
            m_rb.AddForce(transform.forward * m_physicsBoostAccel, ForceMode.Acceleration);
        }

        // I luften: et kontrolleret nedad-skub gør, at bilen ikke “svæver”, men falder mere naturligt tilbage mod banen.
        if (!grounded)
        {
            m_rb.AddForce(Vector3.down * m_extraDownForce, ForceMode.Acceleration);
        }
    }

    // For at beslutte om bilen er “på jorden”, spørger vi først hvert hjul, om deres affjedring har kontakt.
    // Hvis ingen hjul siger ja, laver vi en kort raycast ned under bilen som backup.
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
