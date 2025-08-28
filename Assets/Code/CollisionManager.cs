using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class CollisionManager : MonoBehaviour
{
    private Transform m_player;
    private GameObject m_hitObject;
    Vector3 contactNormal;
    Vector3 relativeVelocity;
   [SerializeField] float m_damage;
   [SerializeField] float m_damageMultiplier;
    float m_dotFoward;
    float m_dotSideway;
    float m_baseDamage;
    private Rigidbody m_rigidbody;
    private Rigidbody m_hitObjectRigidbody;
    private HealthManager currentHealt;
    string collisionType;


    // float damageMultiplier = 0.5f;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        currentHealt = GetComponent<HealthManager>();
    }

     void OnCollisionEnter(Collision collision)
    {
        HealthManager m_hitObjectHealt = collision.gameObject.GetComponent<HealthManager>();
        m_hitObjectRigidbody = collision.rigidbody;

        if (m_hitObjectRigidbody != null && m_hitObjectHealt != null)
        {
            ProcessCollision(currentHealt, m_hitObjectRigidbody);
            Debug.Log(collision.gameObject.name);
        }
    }

    void ProcessCollision(HealthManager m_hitObject, Rigidbody rigidbody)
    {
        Vector3 playerVec = m_rigidbody.velocity;
        Vector3 hitObjectVec = m_hitObjectRigidbody.velocity;
        
        float m_speedDiff = (playerVec - hitObjectVec).magnitude;
        
        Vector3 playerDir = playerVec.normalized;
        Vector3 hitObjectDir = hitObjectVec.normalized;
        Vector3 toOther = (m_hitObjectRigidbody.transform.position - transform.position).normalized;
       
        float dotForward = Vector3.Dot(transform.forward, toOther);
        float dotRight = Vector3.Dot(transform.right, toOther);
        float direction = Vector3.Dot(playerDir, -toOther);

        if (direction > 0.5f) collisionType = "head-on";
        else if (direction < 0.7f) collisionType = "rear-end";
        else if (Mathf.Abs(dotRight) > 0.7f) collisionType = "side-hit";

        m_baseDamage = m_speedDiff * m_damageMultiplier;

        switch (collisionType)

        {
            case "head-on":
                currentHealt.TakeDamage(m_baseDamage * 0.5f);
                // m_hitObjectHealt.TakeDamage(m_baseDamage * 0.5f);
                Debug.Log(gameObject.name + "head-on");
                break;

            case "rear-end":
                // m_hitObjectHealth.TakeDamage(m_baseDamage);
                Debug.Log(gameObject.name + "rear-end");
                break;

            case "side-hit":
                // m_hitObjectHealth.TakeDamage(m_baseDamage);
                Debug.Log(gameObject.name + "side-hit");

                break;

        }


    }
    /*private void OnCollisionEnter(Collision collision)
    {
        m_hitObject = collision.gameObject;
        Vector3 contactNormal = collision.contacts[0].normal;
        Vector3 relativeVelocity = collision.relativeVelocity;
        Vector3 localHitDir = transform.InverseTransformDirection(-contactNormal); //Converts global vector into local, relavant to cars rotation
       //Debug.Log("Normal of the first point: " + collision.contacts[0].normal);
        m_damage = collision.relativeVelocity.magnitude;
       

        foreach (var item in collision.contacts)
        {
            Debug.DrawRay(item.point, item.normal * 100, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
        }
        Debug.Log("Normal of the first point: " + contactNormal);



        if (Mathf.Abs(localHitDir.x) > Mathf.Abs(localHitDir.z))
        {
            GetComponent<VehicleExplosion>().TakeDamage(damage);
        }
        else
        {
            GetComponent<VehicleExplosion>().TakeDamage(damage);
            collision.gameObject.GetComponent<VehicleExplosion>().TakeDamage(damage);
        }
         */

    /* public void OnCollisionEnter(Collision collision)
     {
         HealthManager self = GetComponent<HealthManager>();
         HealthManager other = collision.gameObject.GetComponent<HealthManager>();

         if (self != null && other != null)
         {
            CollisionDamageManager.ResolveCollision(self, other, collision);
         }

     }
    */


}