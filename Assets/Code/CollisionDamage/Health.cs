using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField]private float _currentHealth = 0; 
    [SerializeField]private float _maxHealth = 100;

    [SerializeField] VehicleExplosion vehicleExplosion;
    public void Awake()
    {
        GetComponent<VehicleExplosion>();
        _currentHealth = _maxHealth;
    }
    public void TakeDamage(float amount, GameObject source)
    {
        _currentHealth -=amount;

        if (_currentHealth <= 0 )
        {
            vehicleExplosion.Explode();
        }
    }

}
