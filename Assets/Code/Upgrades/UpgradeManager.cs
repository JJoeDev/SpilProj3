using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    public UIMeter scoreMeter;
    [SerializeField] private UpgradeCard[] m_upgradeCards;
    [SerializeField] private GameObject m_upgradeMenu;
    private InputManager m_inputManager;

    public Upgrade[] upgrades;

    public int upgradeCount = 0;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        m_inputManager = InputManager.Instance;
        ReapplySavedUpgrades();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReapplySavedUpgrades();
    }

    private void ReapplySavedUpgrades()
    {
        if (UpgradeSaving.Instance == null) return;

        // Restore upgrades
        foreach (var savedID in UpgradeSaving.Instance.acquiredUpgrades)
        {
            foreach (var card in m_upgradeCards)
            {
                if (card.UpgradeID == savedID)
                {
                    card.isUnlocked = true;
                    card.UpdateCard();

                    if (card.LinkedUpgrade != null)
                        card.LinkedUpgrade.EnableUpgrade();
                }
            }
        }

        upgradeCount = UpgradeSaving.Instance.acquiredUpgrades.Count;

        // Restore score
        if (scoreMeter != null)
        {
            scoreMeter.value = UpgradeSaving.Instance.savedScore;
        }
    }



    private void Update()
    {
        if (m_inputManager.OnOpenUpgradeRoadmap().triggered)
        {
            m_upgradeMenu.SetActive(!m_upgradeMenu.activeSelf);
            Cursor.lockState = m_upgradeMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (upgradeCount < m_upgradeCards.Length)
        {
            if (scoreMeter.value >= (scoreMeter.maxValue / 3) * (upgradeCount + 1))
            {
                var unlockedCard = m_upgradeCards[upgradeCount];
                Debug.Log("Got upgrade: " + unlockedCard.UpgradeID);

                unlockedCard.isUnlocked = true;
                unlockedCard.UpdateCard();

                if (UpgradeSaving.Instance != null)
                {
                    UpgradeSaving.Instance.acquiredUpgrades.Add(unlockedCard.UpgradeID);
                }

                upgradeCount++;
            }
        }
        else
        {
            scoreMeter.value = scoreMeter.maxValue;
        }

        // Save current score every frame
        if (UpgradeSaving.Instance != null)
        {
            UpgradeSaving.Instance.SetScore((int)scoreMeter.value);
        }
    }

}
