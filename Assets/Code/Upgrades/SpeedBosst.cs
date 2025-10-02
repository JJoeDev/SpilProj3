using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    [SerializeField] private float m_physicsBoostAccel = 15f;     // Et ekstra fremad-skub, men kun når hjulene rører jorden.
    [SerializeField] private float m_extraDownForce = 25f;      // Et ekstra nedad-skub, når bilen er i luften, så den falder hurtigere.

   
    private float m_boostTime; // Hvor meget boost-energi der er tilbage.
    private int m_startaccalertion; // Bilens standard-acceleration.
    private int m_StarterMaxBil; // Bilens standard-tophastighed.

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
        if (m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;

        if (boosting)
        {
            m_CarController.maxSpeed = m_StarterMaxBil + m_SpeedBoost; // Øger tophastighed
            m_CarController.accelerationMultiplier = m_startaccalertion + m_accelerationSpeedBoost; // Øger acceleration

            m_boostTime -= Time.deltaTime;
            if (m_boostTime < 0f) m_boostTime = 0f; // når vi rammer 0 stopper vi med at booste

            
            if (m_RocketFire != null && !m_RocketFire.isPlaying) 
                m_RocketFire.Play(true); // Starter effekten, hvis den ikke allerede kører
        }
        else
        {
            
            m_CarController.maxSpeed = m_StarterMaxBil; // Sætter tophastighed tilbage til normal
            m_CarController.accelerationMultiplier = m_startaccalertion; // Sætter acceleration tilbage til normal

      
            if (m_RocketFire != null && m_RocketFire.isPlaying)
                m_RocketFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Stopper effekten, hvis den kører

          
            if (m_boostTime < m_maxBoostTime) // Genoplad boost-energien over tid
            {
                m_boostTime += Time.deltaTime;
                if (m_boostTime > m_maxBoostTime) m_boostTime = m_maxBoostTime;
            }
        }
    }

    void FixedUpdate()
    {
        // bruger extra fysik til at få bilen til at falde mere naturligt og få en bedre acceleration.
        if (m_rb == null || m_CarController == null) return;

        bool boosting = Input.GetKey(KeyCode.LeftShift) && m_boostTime > 0f;
        bool grounded = IsGrounded(); // Tjekker om bilen er på jorden

      
        if (boosting && !grounded)
        {
            m_rb.AddForce(transform.forward * m_physicsBoostAccel, ForceMode.Acceleration); // Ekstra frem i luften, når der boostes
            m_rb.AddForce(Vector3.down * m_extraDownForce, ForceMode.Acceleration); // Ekstra nedad-skub, når bilen er i luften
        }
    }
    bool IsGrounded()
    {
        if (m_CarController.frontLeftCollider && m_CarController.frontLeftCollider.isGrounded) return true; // Tjekker hvert hjul
        if (m_CarController.frontRightCollider && m_CarController.frontRightCollider.isGrounded) return true;
        if (m_CarController.rearLeftCollider && m_CarController.rearLeftCollider.isGrounded) return true;
        if (m_CarController.rearRightCollider && m_CarController.rearRightCollider.isGrounded) return true;

        Ray ray = new Ray(transform.position + Vector3.up * 0.2f, Vector3.down); // Raycast som backup
        return Physics.Raycast(ray, 1.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore); // Raycast på 1 meter
    }

}
