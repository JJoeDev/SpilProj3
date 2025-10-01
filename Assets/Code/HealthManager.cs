using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleExplosion))]
public class HealthManager : MonoBehaviour
{
    [SerializeField] public float maxHealth;
    public float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            GetComponent<VehicleExplosion>().Explodes();
            if (gameObject.CompareTag("Enemy"))
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<UpgradeManager>().scoreMeter.value += 3.5f;
            }
        }
    }
}
