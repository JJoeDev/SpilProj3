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

        foreach(var col in colliders)
        {
            Debug.DrawLine(col.transform.position, col.transform.position + Vector3.up * 100, Color.red);
            HealthManager hm = col.GetComponent<HealthManager>();
            if (hm == null) continue;

            hm.TakeDamage(damage, gameObject);

            if (explosionEffect == null) continue;
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        // // Check if target has a HealthManager
        // HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
        // if (targetHealth != null)
        // {
        //     // Pass this projectile as the damage source
        //     targetHealth.TakeDamage(damage, gameObject);
        // }

        // // Spawn explosion effect
        // if (explosionEffect != null)
        // {
        //     Instantiate(explosionEffect, transform.position, Quaternion.identity);
        // }

        // // Destroy projectile after impact
        // Destroy(gameObject);
    }
}
