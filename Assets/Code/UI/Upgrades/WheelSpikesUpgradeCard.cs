using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WheelSpikesUpgradeCard : UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.totalEnemiesKilled.ToString() + "/" + 10;
    }
    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.totalEnemiesKilled >= 10;
        return isUnlocked;
    }
}
