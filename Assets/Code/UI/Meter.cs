using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMeter : MonoBehaviour
{
    public float value = 100f;
    public float maxValue = 100f;

    RectTransform m_rectTransform;
    float m_startingWidth;

    private void Start()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_startingWidth = m_rectTransform.sizeDelta.x;
    }

    private void FixedUpdate()
    {
        UpdateMeter();
    }

    public void UpdateMeter()
    {
        m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_startingWidth * (value / maxValue));
    }
}
