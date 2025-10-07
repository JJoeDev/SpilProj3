using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrontCannonUpgradeCard: UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.bigHoopsJumpedThrough.ToString() + "/" + 1;
    }
    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.bigHoopsJumpedThrough >= 1;
        return isUnlocked;
    }
}
