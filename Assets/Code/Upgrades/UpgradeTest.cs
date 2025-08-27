using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTest : Upgrade
{
    [SerializeField] GameObject m_UpdgradeGameObject;
    public override void EnableUpgrade()
    {
        m_UpdgradeGameObject.SetActive(true);
    }

    public override void DisableUpgrade()
    {
        m_UpdgradeGameObject.SetActive(false);
    }
}
