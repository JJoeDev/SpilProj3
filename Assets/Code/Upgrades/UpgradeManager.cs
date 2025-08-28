using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private Upgrade[] m_upgrades;

    private void Start()
    {
        m_upgrades = GetComponentsInChildren<Upgrade>();
    }

}
