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
    float damage;
   // float damageMultiplier = 0.5f;
    private void OnCollisionEnter(Collision collision)
    {
        m_hitObject = collision.gameObject;
        Vector3 contactNormal = collision.contacts[0].normal;
        Vector3 relativeVelocity = collision.relativeVelocity;
        Vector3 localHitDir = transform.InverseTransformDirection(-contactNormal); //Converts global vector into local, relavant to cars rotation
       //Debug.Log("Normal of the first point: " + collision.contacts[0].normal);
        damage = collision.relativeVelocity.magnitude;

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
         
    }
    // Update is called once per frame
    void Update()
    {
            
    }
}
