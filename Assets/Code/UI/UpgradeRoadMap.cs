using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRoadMap : MonoBehaviour
{
    [SerializeField] RawImage[] m_roadMapPoints;
    [SerializeField] float m_scrollSpeed;

    [SerializeField] UpgradeManager m_upgradeManager;
    InputManager m_inputManager;

    public RectTransform rectTransform;

    public float maxScrollDist;
    public float minScrollDist;


    private void Start()
    {
        m_inputManager = InputManager.Instance;
        m_upgradeManager = UpgradeManager.Instance;

        rectTransform = GetComponent<RectTransform>();

        m_roadMapPoints = GetComponentsInChildren<RawImage>();

        //m_minScrollDist = m_roadMapPoints[m_roadMapPoints.Length - 1].transform.position.x;
        //m_maxScrollDist = m_roadMapPoints[0].transform.position.x;
        //UpdateRoadMap();
    }

    private void Update()
    {
        // Scrolling the roadmap.
        if (m_inputManager.OnRoadMapScroll() != 0)
        {
            if (rectTransform.anchoredPosition.x > maxScrollDist && m_inputManager.OnRoadMapScroll() > 0) return;
            if (rectTransform.anchoredPosition.x < minScrollDist && m_inputManager.OnRoadMapScroll() < 0) return;
            rectTransform.anchoredPosition += new Vector2(1, 0) * m_inputManager.OnRoadMapScroll() * m_scrollSpeed * Time.deltaTime;
        }
    }

    public void UpdateRoadMap()
    {
        m_roadMapPoints[m_upgradeManager.upgradeCount].color = Color.HSVToRGB(0, 0, 1);
    }
}
