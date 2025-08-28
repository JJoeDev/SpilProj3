using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UIMeter m_scoreMeter;
    [SerializeField] private UpgradeCard[] upgradeCards;
    public Upgrade[] upgrades;

    public int upgradeCount = 0;

    private void Update()
    {
        if(upgradeCount < upgradeCards.Length)
        {
            if (m_scoreMeter.value >= (m_scoreMeter.maxValue / 3) * (upgradeCount + 1))
            {
                Debug.Log("Got upgrade: " + upgradeCards[upgradeCount].name);
                m_scoreMeter.value = 0;
                upgradeCards[upgradeCount].enabled = true;
                upgradeCards[upgradeCount].UpdateCard();
                upgradeCount++;
            }
        }
        else
        {
            m_scoreMeter.value = m_scoreMeter.maxValue;
        }
    }

}
