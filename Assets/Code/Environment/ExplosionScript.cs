using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleExplosion : Explodable 
{
    [SerializeField] private GameObject m_explosion;
    [SerializeField] private float  health;
   public void Explodes()
    {
        m_explosion = Instantiate(m_explosion, transform.position, Quaternion.identity);
        Instantiate(m_explosion);
        GameObject.Destroy(gameObject);
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage");

        if (health <= 0f)
        {
            Explodes();
        }
    }

}
