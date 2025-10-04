// FrontCarCannon.cs
using System.Collections;
using UnityEngine;

public class FrontCarCannon : Upgrade
{
    [Header("Cannon Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileForce = 100f;
    public float fireCooldown = 5f;

    [Header("Knockback Settings")]
    public Rigidbody carRigidbody;
    public float knockbackForce = 30000f;

    [SerializeField] GameObject m_cannonModel;

    private bool canFire = true;

    public override void EnableUpgrade()
    {
        m_cannonModel.SetActive(true);
    }

    public override void DisableUpgrade()
    {
        m_cannonModel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && canFire)
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
                rb.velocity = firePoint.forward * projectileForce; // shoots forward
            }

            // Ensure it has a CannonballMarker so enemies can detect it
            if (projectile.GetComponent<CannonballMarker>() == null)
            {
                projectile.AddComponent<CannonballMarker>();
            }

            // Destroy after 15 seconds
            Destroy(projectile, 15f);
        }

        // Apply recoil with upward kick
        if (carRigidbody != null)
        {
            Vector3 recoilDirection = (-firePoint.forward + Vector3.up * 0.35f).normalized;
            carRigidbody.AddForce(recoilDirection * knockbackForce, ForceMode.Impulse);
        }
        // tilts the car upwards
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
