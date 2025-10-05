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
                StatTracker.Instance.totalEnemiesKilled++;
                var upgradeManager = UpgradeManager.Instance;
                if (upgradeManager != null)
                {
                    if (source != null && source.GetComponent<CannonballMarker>() != null)
                    {
                        Debug.Log("Enemy killed by CANNONBALL!"); 
                        upgradeManager.RegisterCannonballKill();
                    }
                    else
                    {
                        Debug.Log("Enemy killed by NORMAL means."); 
                        upgradeManager.RegisterNormalKill();
                    }
                }
            }
        }

    }
}
