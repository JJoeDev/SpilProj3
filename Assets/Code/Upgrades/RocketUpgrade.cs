using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketUpgrade : Upgrade
{
    [SerializeField] GameObject rocketObject;
    public override void EnableUpgrade()
    {
        base.EnableUpgrade();
        rocketObject.SetActive(true);
    }
}
