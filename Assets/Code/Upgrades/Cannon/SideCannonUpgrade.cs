using UnityEngine;

public class SideCannonsUpgrade : Upgrade
{
    [Header("Visual Models")]
    [SerializeField] private GameObject m_sideCannonModel;

    [Header("Cannon Scripts")]
    [SerializeField] private LeftCannonUpgrade m_leftCannonScript;
    [SerializeField] private RightCannonUpgrade m_rightCannonScript;

    public override void EnableUpgrade()
    {
        if (m_sideCannonModel != null) m_sideCannonModel.SetActive(true);

        if (m_leftCannonScript != null) m_leftCannonScript.enabled = true;
    }

    public override void DisableUpgrade()
    {
        if (m_sideCannonModel != null) m_sideCannonModel.SetActive(false);

        if (m_leftCannonScript != null) m_leftCannonScript.enabled = false;
        if (m_rightCannonScript != null) m_rightCannonScript.enabled = false;
    }
}
