// UpgradeManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public UpgradeBar upgradeBar;
    [SerializeField] private UpgradeCard[] m_upgradeCards;
    [SerializeField] private GameObject m_upgradeMenu;
    private InputManager m_inputManager;

    public Upgrade[] upgrades;

    public int upgradeCount = 0;

    // track cannonball kills separately
    public int cannonballKills = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
    }

    private void Update()
    {
        if (m_inputManager != null && m_inputManager.OnOpenUpgradeRoadmap().triggered)
        {
            m_upgradeMenu.SetActive(!m_upgradeMenu.activeSelf);
            Cursor.lockState = m_upgradeMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = m_upgradeMenu.activeSelf ? true : false;
        }

        // Save current score every frame
        if (UpgradeSaving.Instance != null)
        {
            UpgradeSaving.Instance.SetScore((int)upgradeCount);
        }
    }

    // Called by HealthManager when an enemy is killed by normal means
    public void RegisterNormalKill()
    {
        if (upgradeBar != null)
        {
            upgradeBar.enemiesKilled++;
        }

        TryUnlockNext();
    }

    // Called by HealthManager when an enemy is killed by a cannonball
    public void RegisterCannonballKill()
    {
        cannonballKills++;
        TryUnlockNext();
    }

    private void TryUnlockNext()
    {
        if (upgradeCount >= m_upgradeCards.Length) return;

        var nextCard = m_upgradeCards[upgradeCount];

        if (nextCard == null) return;

        if (nextCard.requiresCannonballKill)
        {
            Debug.Log("Next upgrade requires " + nextCard.cannonballKillsRequired + " cannonball kills. Current: " + cannonballKills);

            if (cannonballKills >= nextCard.cannonballKillsRequired)
            {
                Debug.Log("Cannonball requirement met. Unlocking upgrade.");
                cannonballKills = 0;
                UnlockUpgrade(nextCard);
            }
        }
        else
        {
            Debug.Log("Next upgrade requires " + nextCard.killsRequired + " normal kills. Current: " + upgradeBar.enemiesKilled);

            if (upgradeBar.enemiesKilled >= nextCard.killsRequired)
            {
                Debug.Log("Normal requirement met. Unlocking upgrade.");
                upgradeBar.enemiesKilled = 0;
                UnlockUpgrade(nextCard);
            }
        }
    }


    private void UnlockUpgrade(UpgradeCard card)
    {
        if (upgradeBar != null)
            upgradeBar.UpdateRoadMap();
        else
            Debug.LogWarning("UpgradeBar reference is missing!");

        Debug.Log(">>> UNLOCKED UPGRADE: " + card.UpgradeID);

        card.isUnlocked = true;
        card.UpdateCard();

        if (card.LinkedUpgrade != null)
            card.LinkedUpgrade.EnableUpgrade();

        if (UpgradeSaving.Instance != null)
            UpgradeSaving.Instance.acquiredUpgrades.Add(card.UpgradeID);

        upgradeCount++;
    }

}
