using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeUpgrade : Upgrade
{
    [SerializeField] GameObject m_spikeModel;

    public override void EnableUpgrade()
    {
        m_spikeModel.SetActive(true);
    }
}
