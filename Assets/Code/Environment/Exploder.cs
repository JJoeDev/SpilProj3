using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploder : MonoBehaviour
{
    [SerializeField] float m_explodeRadius = 5;
    [SerializeField] float m_explosionForce;


    Collider[] m_affectedObjects;
    int m_affectedObjectsCount = 20;

    void Explode()
    {
        Debug.Log("EXPLODING!");

        m_affectedObjects = new Collider[m_affectedObjectsCount];
        Physics.OverlapSphereNonAlloc(transform.position, m_explodeRadius, m_affectedObjects);
        
        foreach (Collider collider in m_affectedObjects)
        {
            if (collider == null || collider.gameObject == gameObject) continue;

            if (collider.GetComponent<Explodable>() != null)
            {
                collider.GetComponent<Explodable>().Explode(transform.position);

                Debug.Log(collider.name);
                Debug.DrawLine(transform.position, collider.transform.position, Color.red, 5f);
            }

        }

        //Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Explode();
        }
    }
}
