using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public static class CollisionDamageManager
{
    public static void ResolveCollision(HealthManager carA, HealthManager carB, Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 hitDir = -contact.normal.normalized;
            
        float dotForwardA = Vector3.Dot(hitDir, carA.transform.forward);
        float dotRightA = Vector3.Dot(hitDir, carA.transform.right);

        float damage = collision.relativeVelocity.magnitude;

        if (Mathf.Abs(dotRightA) > Mathf.Abs(dotForwardA))
            {
                carA.TakeDamage(damage);
            }
            else
            {

                carA.TakeDamage(damage);
                carB.TakeDamage(damage);
            }
    } 
} */
