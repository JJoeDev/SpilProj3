using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public UIMeter scoreMeter;
    [SerializeField] private UpgradeCard[] m_upgradeCards;
    [SerializeField] private GameObject m_upgradeMenu;
    private InputManager m_inputManager;

    public Upgrade[] upgrades;

    public int upgradeCount = 0;

    private void Start()
    {
        m_inputManager = InputManager.Instance;
    }

    private void Update()
    {
        if (m_inputManager.OnOpenUpgradeRoadmap().triggered)
        {
            m_upgradeMenu.SetActive(!m_upgradeMenu.activeSelf);
            Cursor.lockState = m_upgradeMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if(upgradeCount < m_upgradeCards.Length)
        {
            if (scoreMeter.value >= (scoreMeter.maxValue / 3) * (upgradeCount + 1))
            {
                Debug.Log("Got upgrade: " + m_upgradeCards[upgradeCount].name);
                m_upgradeCards[upgradeCount].enabled = true;
                m_upgradeCards[upgradeCount].UpdateCard();
                upgradeCount++;
            }
        }
        else
        {
            scoreMeter.value = scoreMeter.maxValue;
        }
    }

}
