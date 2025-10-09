using System.Collections.Generic;
using UnityEngine;

public class HoopCompletion : MonoBehaviour
{
    [SerializeField] private List<Collider> m_hoopsToHitOriginal;
    [SerializeField] private GameObject m_bigHoop;
    private List<Collider> m_hoopsToHitRuntime;
    public bool smallHoopsCompleted = false;
    [SerializeField] StatTracker m_statTracker;
    private void Awake()
    {
        m_hoopsToHitRuntime = new List<Collider>(m_hoopsToHitOriginal);
        if (!smallHoopsCompleted)
        {
            m_bigHoop.SetActive(false);
        }
        //m_statTracker = StatTracker.Instance;
    } 

    private void OnTriggerEnter(Collider other)
    {
        if (m_hoopsToHitRuntime.Contains(other))
        {
            m_hoopsToHitRuntime.Remove(other);
            Destroy(other.gameObject);
            m_statTracker.smallHoopsJumpedThrough++;
        }
        if (!smallHoopsCompleted && m_hoopsToHitRuntime.Count == 0)
        {
            smallHoopsCompleted = true;
            if (m_bigHoop != null) m_bigHoop.SetActive(true);
        }

        if (smallHoopsCompleted && m_bigHoop != null && other.gameObject == m_bigHoop)
        {
            Destroy(m_bigHoop);
            m_bigHoop = null;
            m_statTracker.bigHoopsJumpedThrough++;
        }
    }
}
