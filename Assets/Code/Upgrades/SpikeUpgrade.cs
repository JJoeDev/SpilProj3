using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeUpgrade : Upgrade
{
    [SerializeField] private GameObject[] m_spikes;
    public override void EnableUpgrade()
    {
        base.EnableUpgrade();

        foreach (GameObject spike in m_spikes)
        {
            spike.SetActive(true);
        }
    }
}
