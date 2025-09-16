using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CollisionManager : MonoBehaviour
{
    private float m_baseDamage = 1f; //used to tweak damagescaling

    private Rigidbody m_rb;
    private HealthManager m_health;
   [SerializeField] private ParticleSystem m_collisionSparks;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_health = GetComponent<HealthManager>();
    }

    void OnCollisionEnter(Collision collision)
    {
       

        HealthManager otherHealth = collision.gameObject.GetComponent<HealthManager>();
        if (otherHealth == null) return; // not a damageable object

        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null) return;

        if (m_rb.GetInstanceID() < otherRb.GetInstanceID()) return; //Reassure that only 1 CollisionManager does damage
       
        // Velocities at impact
        Vector3 v1 = m_rb.velocity;
        Vector3 v2 = otherRb.velocity;

        // Relative velocity
        Vector3 relativeVelocity = collision.relativeVelocity;
        float relativeSpeed = relativeVelocity.magnitude;

        // Direction to other car
        Vector3 toOther = (otherRb.position - m_rb.position).normalized;

        // My forward
        Vector3 myForward = transform.forward;

        // Angle between my forward and "toOther"
        float angle = Vector3.Angle(myForward, toOther);

        // Base scaling from collision force
        float baseImpactDamage = relativeSpeed * m_baseDamage;
       // m_sparkScaling = relativeSpeed * m_scaleMultiplier;
       // transScale.localScale *= m_sparkScaling;
        if (relativeSpeed < 5)
        {
            return;
        }
        else
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                Instantiate(m_collisionSparks, contact.point, Quaternion.identity);
            }

            if (angle < 45f) // I hit them with my front
            {
                // Check if they’re also facing me (head-on)
                float otherAngle = Vector3.Angle(collision.transform.forward, -toOther);

                if (otherAngle < 45f)
                {
                    // --- HEAD-ON ---
                    float mySpeed = v1.magnitude;
                    float theirSpeed = v2.magnitude;
                    float totalSpeed = mySpeed + theirSpeed;

                    if (totalSpeed <= 0f) return; // avoid div by zero

                    // Damage is split proportionally:
                    // Faster car takes less, slower car takes more
                    float myShare = theirSpeed / totalSpeed;   // I take damage relative to their speed
                    float theirShare = mySpeed / totalSpeed;   // They take damage relative to my speed

                    m_health.TakeDamage(baseImpactDamage * myShare);
                    otherHealth.TakeDamage(baseImpactDamage * theirShare);
                }
                else
                {
                    // I rammed their side/rear: they take damage
                    otherHealth.TakeDamage(baseImpactDamage);
                }
            }
            else if (angle > 135f) // They hit me from behind
            {
                m_health.TakeDamage(baseImpactDamage);
            }
            else // Side impact
            {
                m_health.TakeDamage(baseImpactDamage);
            }
        }
    }

}