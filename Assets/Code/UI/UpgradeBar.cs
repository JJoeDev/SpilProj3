using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBar : MonoBehaviour
{
    RectMask2D m_mask;
    public int upgradeIndex = 0;
    public int enemiesKilled = 0;
    [SerializeField] float[] m_progressStages;

    private void Start()
    {
        m_mask = GetComponent<RectMask2D>();
    }

    public void UpdateBar()
    {

        Mathf.Clamp(upgradeIndex, 0, m_progressStages.Length);
        m_mask.padding = new Vector4(0,0,m_progressStages[upgradeIndex],0);
    }
}
