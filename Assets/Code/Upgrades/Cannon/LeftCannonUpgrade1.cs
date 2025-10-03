using System.Collections;
using UnityEngine;

public class LeftCannonUpgrade : Upgrade
{
    [Header("Cannon Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileForce = 100f;
    public float fireCooldown = 5f;

    [Header("Knockback Settings")]
    public Rigidbody carRigidbody;
    public float knockbackForce = 10000f;

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
        if (Input.GetMouseButtonDown(0) && canFire)
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
                rb.velocity = -firePoint.right * projectileForce; // shoots forward
            }

            // Destroy after 15 seconds
            Destroy(projectile, 15f);
        }

        if (carRigidbody != null)
        {
            // sideways + upward force
            Vector3 recoilDirection = (firePoint.right + Vector3.up * 0.35f).normalized;
            carRigidbody.AddForce(recoilDirection * knockbackForce, ForceMode.Impulse);
        }

        StartCoroutine(FireCooldown());
    }

    IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireCooldown);
        canFire = true;
    }
}
