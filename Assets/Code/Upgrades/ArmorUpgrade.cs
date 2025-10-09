using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorUpgrade : Upgrade
{
    public float armorHealth = 150f;
    public GameObject[] armorVisuals;

    public override void EnableUpgrade()
    {
        // Adds health
        HealthManager health = GetComponent<HealthManager>();
        
        if (health != null)
        {
            health.maxHealth = armorHealth;
        }
        // Activates the armor visually
        if (armorVisuals.Length != 0)
        {
            foreach (GameObject armorPart in armorVisuals)
            {
                armorPart.SetActive(true);
            }
        }
    }
}
