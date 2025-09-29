using System;
using System.Collections.Generic;
using UnityEngine;

public class SpikeHub : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float m_Spikeskade = 10f;
    [SerializeField] private float m_hitCooldown = 0.25f;

    [Header("Filter")]
    [SerializeField] private LayerMask m_damageableLayers;
    [SerializeField] private Collider[] m_spikeColliders; // antal af bilens spikes. 

    
    private readonly Dictionary<HealthManager, float> m_lastHitTime = new(); // bliver brugt til cooldown

    private Transform myRoot;

    private void Awake()
    {
        myRoot = transform.root;
    }

    public void HandleTrigger(Collider other, Collider whichSpike)
    {
        
        if (Array.IndexOf(m_spikeColliders, whichSpike) < 0) return;

        // Layer-filter
        if (((1 << other.gameObject.layer) & m_damageableLayers) == 0) return;

        // Find mål (HealthManager)
        HealthManager hp = null;
        if (other.attachedRigidbody != null)
            hp = other.attachedRigidbody.GetComponentInParent<HealthManager>();
        if (hp == null)
            hp = other.GetComponentInParent<HealthManager>();
        if (hp == null) return;

        // Undgå selvskade
        if (hp.transform.root == myRoot) return;

        // Cooldown
        if (m_lastHitTime.TryGetValue(hp, out var t) && Time.time - t < m_hitCooldown) return;

        // Giver skade
        hp.TakeDamage(m_Spikeskade);
        m_lastHitTime[hp] = Time.time;
    }
}
