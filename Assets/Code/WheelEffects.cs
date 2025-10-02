using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour
{
    /*WheelHit hit;
    Rigidbody rb;
    [SerializeField] private float m_minForwardSlideThreshold;
    [SerializeField] private float m_maxForwardSlideThreshold;
   [SerializeField] private WheelCollider[] m_colliders;
    [SerializeField] private ParticleSystem m_dustParticles;
    [SerializeField] private Transform m_dustTransform;
    private ParticleSystem m_runtimeDustParticle;
   [SerializeField] private WheelEffects[] m_wheelEff;
   [SerializeField] WheelCollider m_wheelColl; */
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
    [SerializeField] private float m_minDustSpeed = 2f;
    [SerializeField] private float m_maxStartSpeed = 5f;
    public WheelParameters[] wheels;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Spawn all particle systems
        foreach (var w in wheels)
        {
            if (w.dustParticle != null && w.dustTransform != null)
            {
                w.runtimeDust = Instantiate(w.dustParticle, w.dustTransform.position, w.dustTransform.rotation,w.dustTransform);
                w.runtimeDust.Stop();
            }
        }
    }

    void Update()
    {
        foreach (var w in wheels)
        {
            if (w.wheelColl == null) continue;

            WheelHit hit;
            if (w.wheelColl.GetGroundHit(out hit))
            {
                float slip = Mathf.Max(Mathf.Abs(hit.forwardSlip), Mathf.Abs(hit.sidewaysSlip));
                bool shouldPlay = rb.velocity.magnitude > m_minDustSpeed && slip > m_slipThreshold;
                bool startPlay = shouldPlay && m_maxStartSpeed < rb.velocity.magnitude;
                //bool dirtTooBig = w.runtimeDust.transform.localScale < ;

                if (shouldPlay && !w.runtimeDust.isPlaying) w.runtimeDust.Play();
                else if (startPlay && !w.runtimeDust.isPlaying)
                {
                    w.dustParticle.transform.localScale *= 1.5f;
                    w.runtimeDust.Play();
                    Debug.Log("yarrr");
                }
                else if (!shouldPlay && w.runtimeDust.isPlaying) w.runtimeDust.Stop();
            }
            else
            {
                if (w.runtimeDust != null && w.runtimeDust.isPlaying)
                {
                    w.runtimeDust.Stop();
                    w.runtimeDust.transform.localScale = Vector3.one;
                }
                    

            }
            //if (w.runtimeDust.transform.localScale)
        }
    }























    /*private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        foreach (var w in m_wheelEff)
        {
            if (w.m_dustParticles != null && m_dustTransform != null)
            {
                w.m_runtimeDustParticle = Instantiate(w.m_dustParticles, m_dustTransform.position, w.m_dustTransform.rotation, w.m_dustTransform);
                w.m_runtimeDustParticle.Stop();
            }
        }
    }

    void Update()
    {
        foreach (var w in m_wheelEff)
        {
            if (w.m_wheelColl.GetGroundHit(out hit))
            {
                float slip = Mathf.Max(Mathf.Abs(hit.forwardSlip));
                bool dustState = rb.velocity.magnitude > 2f && slip > m_maxForwardSlideThreshold;

                if (dustState && !w.m_runtimeDustParticle.isPlaying)
                {
                    w.m_runtimeDustParticle.Play();
                }
                else if (!dustState && w.m_runtimeDustParticle.isPlaying)
                {
                    w.m_runtimeDustParticle.Stop();
                }

                if (w.m_runtimeDustParticle !=null)
                {
                    w.m_runtimeDustParticle.transform.position = w.m_dustTransform.position;
                    w.m_runtimeDustParticle.transform.rotation = w.m_dustTransform.rotation;
                }
            }

            else
            {
                if (w.m_runtimeDustParticle !=null && w.m_runtimeDustParticle.isPlaying)
                {
                    w.m_runtimeDustParticle.Stop();
                }
            }
        }


        /*for (int i = 0; i < m_colliders.Length; i++)
        {

            if (m_colliders[i].GetGroundHit(out hit))
            {
                if (hit.forwardSlip > m_minForwardSlideThreshold && hit.forwardSlip < m_maxForwardSlideThreshold)
                {
                    
                  Debug.Log($"Wheel {i} FORWARD slide with: {hit.forwardSlip}");
                }

            }
        } 
    }
    */
} 
