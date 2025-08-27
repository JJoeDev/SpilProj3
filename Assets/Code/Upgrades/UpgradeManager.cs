using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] UIMeter m_scoreMeter;

    private Upgrade[] m_upgrades;
    private int m_upgradeCount = 0;

    public void EnableUpgrade(int upgradeIndex)
    {
        m_upgrades[upgradeIndex].EnableUpgrade();
    }
    private void Start()
    {
        m_upgrades = GetComponentsInChildren<Upgrade>();
    }

    private void Update()
    {
        if(m_upgradeCount <= m_upgrades.Length)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                m_scoreMeter.value++;
            }
            if (m_scoreMeter.value >= m_scoreMeter.maxValue)
            {
                m_scoreMeter.value = 0;
                EnableUpgrade(m_upgradeCount);
                m_upgradeCount++;
            }
        }
        else
        {
            m_scoreMeter.value = m_scoreMeter.maxValue;
        }
    }

}
