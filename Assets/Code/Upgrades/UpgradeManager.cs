using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] UIMeter m_scoreMeter;

    private Upgrade[] m_upgrades;
    private int m_upgradeCount = 0;


    private void Start()
    {
        m_upgrades = GetComponentsInChildren<Upgrade>();
    }

    private void Update()
    {
        if(m_upgradeCount <= m_upgrades.Length)
        {
            if (m_scoreMeter.value >= m_scoreMeter.maxValue)
            {
                m_scoreMeter.value = 0;
                m_upgrades[m_upgradeCount].EnableUpgrade();
                m_upgradeCount++;
            }
        }
        else
        {
            m_scoreMeter.value = m_scoreMeter.maxValue;
        }
    }

}
