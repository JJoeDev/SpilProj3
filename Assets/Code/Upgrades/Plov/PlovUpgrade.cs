using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plov : Upgrade
{
    [SerializeField] GameObject m_plovModel;
    [SerializeField] float m_knockbackForce;

    public override void EnableUpgrade()
    {
        m_plovModel.SetActive(true);
    }

    public override void DisableUpgrade()
    {
        m_plovModel.SetActive(false);
    }


    private void OnTriggerEnter(Collider m_collision)
    {

        if (m_collision.gameObject.CompareTag("Enemy"))
        {
            
            if (m_collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody m_enemyRB))
            {
                m_enemyRB.velocity = (transform.position - m_collision.transform.position).normalized * m_knockbackForce;
            }

            // TODO: Make it deal more damage
        }
    }
}
