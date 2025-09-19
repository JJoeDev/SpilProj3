using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentExplosion : Explodable
{
   private HealthManager m_healthManager;
    private Explodable m_isExplodable;
    [SerializeField] float m_explodeRadius = 5;
    [SerializeField] float m_explosionForce = 15;
    Collider[] m_affectedObjects;
    int m_affectedObjectsCount;

    private void Awake()
    {
        m_healthManager = GetComponent<HealthManager>();
        m_isExplodable = GetComponent<Explodable>();
    }
   public void OnCollisionEnter(Collision collision)
    {
        
        
    }

    public override void Explode()
    {

        m_affectedObjects = new Collider[m_affectedObjectsCount];
        Physics.OverlapSphereNonAlloc(transform.position, m_explodeRadius, m_affectedObjects);



        foreach (Collider collider in m_affectedObjects)
        {
            if (collider == null || collider.gameObject == gameObject) return;

            if (collider.GetComponent<PhysicExplodable>() != null || collider.CompareTag("Player"))
            {
                collider.GetComponent<PhysicExplodable>().Explode(transform.position, m_explosionForce);

                Debug.DrawLine(transform.position, collider.transform.position, Color.red, 5f);
                GetComponent<VehicleExplosion>().Explodes();
            }
        }
        
    }
}
