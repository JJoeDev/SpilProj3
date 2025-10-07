// HealthManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    [SerializeField] UIMeter healthBar;
    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Backwards-compatible overload (in case other code calls old signature)
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, null);
    }

    // New signature with source
    public void TakeDamage(float amount, GameObject source)
    {
        currentHealth -= amount;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0f)
        {
            GetComponent<VehicleExplosion>().Explodes();

            if (gameObject.CompareTag("Enemy"))
            {
                StatTracker m_statTracker = StatTracker.Instance;
                if (m_statTracker != null)
                {
                    m_statTracker.totalEnemiesKilled++;
                    if (source != null && source.GetComponent<CannonballMarker>() != null)
                    {
                        Debug.Log("Enemy killed by CANNONBALL!");
                        m_statTracker.enemiesKilledWithCannon++;
                    }
                }
            }
        }

    }
}
