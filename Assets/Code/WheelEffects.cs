using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour
{
    [System.Serializable]
    public class WheelParameters
    {
       public WheelCollider wheelColl;
       public Transform dustTransform;
       public ParticleSystem dustParticle;
       [HideInInspector]
       public ParticleSystem runtimeDust;
    }

    [SerializeField] private float m_slipThreshold = 0.3f;
    [SerializeField] private float m_minDustSpeed = 6f;
    [SerializeField] private float m_maxStartSpeed = 5f;
    public WheelParameters[] wheels;
    [SerializeField] public Vector3 dustMaxScale;
    [SerializeField] public Vector3 dustDefaultScale;
    public Vector3 currentDustScale;

    private float m_slip;
    private CarController m_car;

    private void Start()
    {
        m_car = GetComponent<CarController>();
        foreach (var wheel in wheels)
        {
            if (wheel.dustParticle != null && wheel.dustTransform != null)
            {
                wheel.runtimeDust = Instantiate(wheel.dustParticle, wheel.dustTransform.position, wheel.dustTransform.rotation,wheel.dustTransform);
                wheel.runtimeDust.Stop();
                wheel.runtimeDust.transform.localScale = dustDefaultScale;
            }
            
        }
    }

    void Update()
    {
        foreach (var wheel in wheels)
        {
            
            if (wheel.wheelColl == null) continue;
            WheelHit hit;
            if (wheel.wheelColl.GetGroundHit(out hit))
            {
                m_slip = Mathf.Max(Mathf.Abs(hit.forwardSlip));
                currentDustScale = Vector3.Lerp(currentDustScale, dustMaxScale, m_car.carSpeed);
                bool shouldPlay = m_car.carSpeed >= m_minDustSpeed && m_slip >= m_slipThreshold;
                bool startPlay = m_slip >= m_slipThreshold && m_car.carSpeed <= m_maxStartSpeed;
                
                wheel.runtimeDust.transform.localScale = currentDustScale;

                if (shouldPlay && !wheel.runtimeDust.isPlaying)
                {
                    wheel.runtimeDust.Play();
                    
                }
                if (startPlay && wheel.runtimeDust.isPlaying)
                {
                    currentDustScale = dustMaxScale;
                    wheel.runtimeDust.Play();
                    
                } 
                else if (!shouldPlay && wheel.runtimeDust.isPlaying)
                {
                    wheel.runtimeDust.Stop();
                    currentDustScale = dustDefaultScale;
                   
                }
            }
        }
    }
} 
