// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionEffect;
    public float damage = 25f;

    [SerializeField] private float m_explotionRadius = 9.0f;

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_explotionRadius);

        HealthManager hm;

        foreach (var col in colliders)
        {
            Debug.DrawLine(col.transform.position, col.transform.position + Vector3.up * 100, Color.red);
            hm = col.GetComponent<HealthManager>();
            if (hm == null) continue;

            if (col == collision.collider) 
            {
                hm.TakeDamage(damage, gameObject);
                continue;
            }


            Vector3 distance = col.transform.position - transform.position;
            hm.TakeDamage(damage / distance.magnitude, gameObject); // Less damage based on distance
        }

        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
    }
}
