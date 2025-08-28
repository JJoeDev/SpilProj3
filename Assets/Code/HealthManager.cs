using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log($"{gameObject.name} took {amount} damage. health remaining: {currentHealth}");
        
            if (currentHealth <= 0f)
        {
            gameObject.GetComponent<VehicleExplosion>().Explode();
        }
    }
}
