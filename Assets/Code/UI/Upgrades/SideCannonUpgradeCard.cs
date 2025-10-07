using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCannonUpgradeCard: UpgradeCard
{
    StatTracker m_statTracker;

    private void Start()
    {
        m_statTracker = StatTracker.Instance;
    }

    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = m_statTracker.enemiesKilledWithCannon >= 10;
        return isUnlocked;
    }
}
