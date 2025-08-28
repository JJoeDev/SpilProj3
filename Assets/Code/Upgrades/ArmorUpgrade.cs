using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorUpgrade : Upgrade
{
    public float armorHealth = 50f;

    bool applied;

    private void Start()
    {
        // Enables the ability at the start of the game
        if (!applied) { EnableUpgrade(); applied = true; }
    }
    public override void EnableUpgrade()
    {
        // Gets the Health component and adds health to it
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.AddMaxHealth(armorHealth);
        }
    }
}
