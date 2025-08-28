using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleExplosion : Explodable 
{
    [SerializeField] private GameObject m_explosion;
   // [SerializeField] public float  health;
   public void Explodes()
    {
        m_explosion = Instantiate(m_explosion, transform.position, Quaternion.identity);
        Instantiate(m_explosion);
        GameObject.Destroy(gameObject);
    }
}
