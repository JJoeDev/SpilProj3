using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] HealthManager m_healthManager;
    [SerializeField] Transform m_bar;
    [SerializeField] float m_offsetPos; // How much to move the bar left as the health scales

    Vector3 m_startScale;
    Vector3 m_startPos;
    Camera m_mainCam;

    private void Start()
    {
        m_startScale = m_bar.transform.localScale;
        m_startPos = m_bar.transform.localPosition;
        m_mainCam = Camera.main;
    }

    void Update()
    {
        float m_t = (m_healthManager.currentHealth / m_healthManager.maxHealth);
        if (m_mainCam != null) transform.LookAt(m_mainCam.transform);
        m_bar.transform.localScale = new Vector3(m_startScale.x * m_t, m_startScale.y, m_startScale.z);
        m_bar.localPosition = new Vector3(m_startPos.x - m_offsetPos * (1f - m_t), m_startPos.y, m_startPos.z);
    }
}
