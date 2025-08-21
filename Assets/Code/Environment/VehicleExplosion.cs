using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleExplosion : MonoBehaviour {
  [SerializeField] private int m_Health;

    
    void Update ()
    {
        if (m_Health <= 0)
        {
            Explode();
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            m_Health--;
        }
    }

   public void Explode()
    {
        GameObject.Destroy(gameObject);
        Debug.Log("Kaboom");
      //  <ParticleSystem>();

    }

}
