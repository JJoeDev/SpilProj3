using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrophyUpgradeCard : UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.totalEnemiesKilled.ToString() + "/" + 50;
    }
    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.totalEnemiesKilled >= 50;
        return isUnlocked;
    }
}
