using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleExplosion : MonoBehaviour {
    [SerializeField] private GameObject m_explosion;
   public void Explode()
    {
        m_explosion = Instantiate(m_explosion, transform.position, Quaternion.identity);
        Instantiate(m_explosion);
        GameObject.Destroy(gameObject);
    }
}
