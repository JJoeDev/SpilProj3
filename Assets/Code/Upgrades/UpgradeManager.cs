// UpgradeManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    private static UpgradeManager _instance;
    public static UpgradeManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Two upgrademanagers exist - Deleting the duplicate!");
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public UpgradeRoadMap upgradeRoadMap;
    [SerializeField] private UpgradeCard[] m_upgradeCards;
    [SerializeField] private GameObject m_upgradeMenu;

    private InputManager m_inputManager;
    private StatTracker m_statTracker;
    
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
        m_statTracker = StatTracker.Instance;
        ReapplySavedUpgrades();
    }

    private void Update()
    {
        if (m_inputManager != null && m_inputManager.OnOpenUpgradeRoadmap().triggered)
        {
            m_upgradeMenu.SetActive(!m_upgradeMenu.activeSelf);
            Cursor.lockState = m_upgradeMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = m_upgradeMenu.activeSelf ? true : false;
        }

        if (upgradeCount < m_upgradeCards.Length)
        {
            if (m_upgradeCards[upgradeCount].CheckUpgradeUnlocked())
            {
                upgradeRoadMap.UpdateRoadMap();
                var unlockedCard = m_upgradeCards[upgradeCount];
                Debug.Log("Got upgrade: " + unlockedCard.UpgradeID);
                unlockedCard.UpdateCard();

                if (UpgradeSaving.Instance != null)
                {
                    UpgradeSaving.Instance.acquiredUpgrades.Add(unlockedCard.UpgradeID);
                }

                upgradeCount++;
            }
        }

        // Save current score every frame
        if (UpgradeSaving.Instance != null)
        {
            UpgradeSaving.Instance.SetScore((int)upgradeCount);
        }
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

}
