using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploder : Explodable
{
    [SerializeField] float m_explodeRadius = 5;
    [SerializeField] float m_explosionForce = 15;
    private HealthManager m_healthManager;

    private void Awake()
    {
        m_healthManager = GetComponent<HealthManager>();
    }

    Collider[] m_affectedObjects;
    int m_affectedObjectsCount = 20;

    public override void Explode()
    {
       
        m_affectedObjects = new Collider[m_affectedObjectsCount];
        Physics.OverlapSphereNonAlloc(transform.position, m_explodeRadius, m_affectedObjects);
        
        foreach (Collider collider in m_affectedObjects)
        {
            if (collider == null || collider.gameObject == gameObject) continue;

            if (collider.GetComponent<Explodable>() != null)
            {
                collider.GetComponent<Explodable>().Explode(transform.position, m_explosionForce);
               // Debug.DrawLine(transform.position, collider.transform.position, Color.red, 5f);
            }
        }
    }
}
