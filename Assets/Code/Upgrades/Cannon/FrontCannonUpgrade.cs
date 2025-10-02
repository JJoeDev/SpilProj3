using System.Collections;
using UnityEngine;

public class CarCannon : MonoBehaviour
{
    [Header("Cannon Settings")]
    public GameObject projectilePrefab;   
    public Transform firePoint;           
    public float projectileForce = 100f;   
    public float fireCooldown = 5f;     

    [Header("Knockback Settings")]
    public Rigidbody carRigidbody;        
    public float knockbackForce = 30000f;    

    private bool canFire = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canFire)
        {
            FireCannon();
        }
    }

    void FireCannon()
    {
        canFire = false;

        if (projectilePrefab != null && firePoint != null)
        {
            // Spawn projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.velocity = firePoint.forward * projectileForce; // direct velocity forward
            }

            // Destroy after 15 seconds
            Destroy(projectile, 15f);
        }

        // Apply recoil with upward kick
        if (carRigidbody != null)
        {
            // backward + upward force
            Vector3 recoilDirection = (-firePoint.forward + Vector3.up * 0.35f).normalized;
            carRigidbody.AddForce(recoilDirection * knockbackForce, ForceMode.Impulse);
        }
        // Apply torque to tilt the car upwards
        if (carRigidbody != null)
        {
            Vector3 torque = -firePoint.right * knockbackForce * 0.1f;
            carRigidbody.AddTorque(torque, ForceMode.Impulse);
        }


        StartCoroutine(FireCooldown());
    }

    IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }
}
