using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBar : MonoBehaviour
{
    public int enemiesKilled = 0; 
    [SerializeField] UpgradeManager m_manager;
    [SerializeField] Image[] roadMapPoints;

    public void UpdateRoadMap()
    {
        int i = 0;
        for (; i < m_manager.upgradeCount; i++)
        {
            roadMapPoints[i].color = Color.HSVToRGB(1, 1, 1);
        }
        for (; i < roadMapPoints.Length; i++)
        {
            roadMapPoints[i].color = Color.HSVToRGB(1, 1, 0.5f);
        } 
    }
}
