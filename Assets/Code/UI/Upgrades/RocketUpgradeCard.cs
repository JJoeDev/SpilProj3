using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RocketUpgradeCard : UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.smallHoopsJumpedThrough.ToString() + "/" + 3;
    }
    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.smallHoopsJumpedThrough >= 3;
        return isUnlocked;
    }
}
