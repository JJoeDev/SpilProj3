using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArmorUpgradeCard : UpgradeCard
{
    [SerializeField] TMP_Text stattracker_Text;

    public override void Update()
    {
        base.Update();
        stattracker_Text.text = statTracker.enemiesPushedIntoBarrels.ToString() + "/" + 1;
    }
    public override bool CheckUpgradeUnlocked()
    {
        isUnlocked = statTracker.enemiesPushedIntoBarrels >= 1;
        return isUnlocked;
    }
}
