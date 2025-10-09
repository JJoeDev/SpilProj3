using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorUpgrade : Upgrade
{
    public float armorHealth = 150f;
    public GameObject[] armorVisuals;
    [SerializeField] HealthManager m_healthManager;

    public override void EnableUpgrade()
    {
        Debug.Log("Enabling armir");
        
        if (m_healthManager != null)
        {
            m_healthManager.maxHealth = armorHealth;
            m_healthManager.currentHealth = armorHealth;
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
