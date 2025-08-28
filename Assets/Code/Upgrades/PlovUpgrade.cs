using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlovUpgrade : Upgrade
{
    [SerializeField] GameObject m_plovModel;
    public override void EnableUpgrade()
    {
        m_plovModel.SetActive(true);
    }

    public override void DisableUpgrade()
    {
        m_plovModel.SetActive(false);
    }
}
