using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] HealthManager m_healthManager;
    [SerializeField] Transform m_bar;

    Vector3 m_startScale;
    Camera m_mainCam;

    private void Start()
    {
        m_startScale = m_bar.transform.localScale;
        m_mainCam = Camera.main;
    }

    void Update()
    {
        transform.LookAt(m_mainCam.transform);
        m_bar.transform.localScale = new Vector3(m_startScale.x * (m_healthManager.currentHealth / m_healthManager.maxHealth), m_startScale.y, m_startScale.z);
    }
}
