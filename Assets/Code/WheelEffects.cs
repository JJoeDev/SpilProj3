using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour
{
    WheelHit hit = new WheelHit();
    [SerializeField] private WheelCollider m_wheelCollider;

    private void Awake()
    {
        m_wheelCollider.GetComponent<WheelCollider>();
    }

    void Update()
    {
     if (m_wheelCollider.GetGroundHit(out hit))
        {
            if (hit.forwardSlip > 0.5)

            {
              Debug.Log("Slidin'");
            }

        }

    }
    
}
