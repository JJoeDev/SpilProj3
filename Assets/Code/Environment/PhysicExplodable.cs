using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhysicExplodable : Explodable
{
    Rigidbody m_rigidBody;
    private void Start()
    {
        if (GetComponent<Rigidbody>())
        {
            Debug.LogWarning("Physics explodable object: " + name + ", requires a rigidbody!");
        }
        else
        {
            m_rigidBody = GetComponent<Rigidbody>();
        }
    }

    public override void Explode(Vector3 exploderPos, float explosionforce)
    {
        m_rigidBody.velocity = (transform.position - exploderPos).normalized * explosionforce + Vector3.up;
    }
}
