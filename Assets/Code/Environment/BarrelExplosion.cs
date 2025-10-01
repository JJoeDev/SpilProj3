using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelExplosion : Explodable
{
    [SerializeField] private float m_explodeRadius;
    [SerializeField] private float m_explosionForce;
    [SerializeField] private float m_explosionLiftMultiplier;


    private HealthManager m_health;

    private void Awake()
    {
        m_health = GetComponent<HealthManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float hitSpeed;
        GameObject m_hitObject = collision.gameObject;
        Vector3 relativeVel = collision.relativeVelocity;
        hitSpeed = relativeVel.magnitude;
       

        if (m_hitObject.GetComponent<VehicleExplosion>() && hitSpeed >= 5f)
        { 
            Explode(transform.position, m_explosionForce);
        }
    }

    public override void Explode(Vector3 exploderPos, float explosionForce)
    {
        HashSet<HealthManager> damagedObjects = new HashSet<HealthManager>();
        Collider[] m_affectedObjects = Physics.OverlapSphere(transform.position, m_explodeRadius);

        foreach (Collider collider in m_affectedObjects)
        {
            if (collider == null || collider.gameObject == gameObject) continue;


            Rigidbody rb = collider.attachedRigidbody;
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce,transform.position,m_explodeRadius,m_explosionLiftMultiplier,ForceMode.Impulse);
            }

            HealthManager health = collider.gameObject.GetComponentInParent<HealthManager>();
            if (health != null  && !damagedObjects.Contains(health))
            {
                health.TakeDamage(15/4);
            }
        }

        VehicleExplosion ExplosionEfx = GetComponent<VehicleExplosion>();
        if (ExplosionEfx != null)
        {
            ExplosionEfx.Explodes();
        }
    }

    // Så du kan se radius i editoren
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_explodeRadius);
    }
}

