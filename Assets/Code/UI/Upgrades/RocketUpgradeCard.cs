using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketUpgradeCard : UpgradeCard
{
    StatTracker m_statTracker;

    private void Start()
    {
        m_statTracker = StatTracker.Instance;
    }

    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = m_statTracker.smallHoopsJumpedThrough >= 3;
        return isUnlocked;
    }
}
