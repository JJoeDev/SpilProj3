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
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.AddMaxHealth(armorHealth);
        }
        // Activates the armor visually
        if (armorVisual != null)
        {
            armorVisual.SetActive(true);
        }
    }
}
