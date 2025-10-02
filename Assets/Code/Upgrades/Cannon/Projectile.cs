using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionEffect;
    public float damage = 25f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if target has a HealthManager
        HealthManager targetHealth = collision.gameObject.GetComponent<HealthManager>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Destroy projectile after impact
        Destroy(gameObject);
    }
}
