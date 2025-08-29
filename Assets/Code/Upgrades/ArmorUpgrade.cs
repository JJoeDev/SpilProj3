using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorUpgrade : Upgrade
{
    public float armorHealth = 50f;
    public GameObject armorVisual;

    bool applied;

    private void Start()
    {
        // Enables the ability at the start of the game
        if (!applied) { EnableUpgrade(); applied = true; }
    }
    public override void EnableUpgrade()
    {
        // Adds health
        HealthManager health = GetComponent<HealthManager>();
        if (health != null)
        {
            health.maxHealth = armorHealth;
        }
        // Activates the armor visually
        if (armorVisual != null)
        {
            armorVisual.SetActive(true);
        }
    }
}
