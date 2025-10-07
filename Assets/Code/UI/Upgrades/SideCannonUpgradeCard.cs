using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SideCannonUpgradeCard: UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.enemiesKilledWithCannon.ToString() + "/" + 10;
    }

    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.enemiesKilledWithCannon >= 10;
        return isUnlocked;
    }
}
