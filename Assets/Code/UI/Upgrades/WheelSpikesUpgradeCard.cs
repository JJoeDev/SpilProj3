using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSpikesUpgradeCard : UpgradeCard
{
    StatTracker m_statTracker;

    private void Start()
    {
        m_statTracker = StatTracker.Instance;
    }

    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = m_statTracker.totalEnemiesKilled >= 10;
        return isUnlocked;
    }
}
