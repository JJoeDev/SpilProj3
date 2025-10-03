using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRoadMap : MonoBehaviour
{
    public int enemiesKilled = 0; 
    UpgradeManager m_upgradeManager;
    [SerializeField] RawImage[] m_roadMapPoints;
    [SerializeField] float m_scrollSpeed;

    InputManager m_inputManager;

    [SerializeField] float m_maxScrollDist;
    [SerializeField] float m_minScrollDist;

    private void Start()
    {
        m_inputManager = InputManager.Instance;
        m_upgradeManager = UpgradeManager.Instance;
        //m_minScrollDist = m_roadMapPoints[m_roadMapPoints.Length - 1].transform.position.x;
        //m_maxScrollDist = m_roadMapPoints[0].transform.position.x;
        UpdateRoadMap();
    }

    private void Update()
    {
        // Scrolling the roadmap.
        if (m_inputManager.OnRoadMapScroll() != 0)
        {
            if (transform.localPosition.x > m_maxScrollDist && m_inputManager.OnRoadMapScroll() > 0) return;
            if (transform.localPosition.x < m_minScrollDist && m_inputManager.OnRoadMapScroll() < 0) return;
            transform.localPosition += new Vector3(1, 0, 0) * m_inputManager.OnRoadMapScroll() * m_scrollSpeed * Time.deltaTime;
        }
    }

    public void UpdateRoadMap()
    {
        m_roadMapPoints = GetComponentsInChildren<RawImage>();

        int i = 0;
        for (; i < m_upgradeManager.upgradeCount; i++)
        {
            m_roadMapPoints[i].color = Color.HSVToRGB(0, 0, 1);
        }
        for (; i < m_roadMapPoints.Length; i++)
        {
            m_roadMapPoints[i].color = Color.HSVToRGB(0, 0, 0.5f);
        } 
    }
}
