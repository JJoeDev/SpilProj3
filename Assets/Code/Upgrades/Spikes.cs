using System.Collections.Generic;
using UnityEngine;

public class SpikeHub : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float m_Spikeskade = 10f;
    [SerializeField] private float m_hitCooldown = 0.25f;   // sek. pr. mål

    [Header("Filter")]
    [SerializeField] private LayerMask m_damageableLayers;   // fx "Enemy"

    // Cooldown
    private readonly Dictionary<HealthManager, float> m_lastHitTime = new();

    private Transform myRoot;

    private void Awake()
    {
        myRoot = transform.root;

        InvokeRepeating("ClearDict", 60, 30);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Layer-filter: kun ram mål på de valgte lag
        if (((1 << other.gameObject.layer) & m_damageableLayers) == 0)
            return;

        Debug.Log("ON SPIKE ENTER");

        // Find HealthManager på det ramte (eller dets Rigidbody/parent)
        HealthManager hp = null;
        if (other.attachedRigidbody != null)
            hp = other.attachedRigidbody.GetComponentInParent<HealthManager>();
        if (hp == null)
            hp = other.GetComponentInParent<HealthManager>();
        if (hp == null)
            return;

        // Undgå selvskade (samme root som din egen bil)
        if (hp.transform.root == myRoot)
            return;

        // Cooldown pr. fjende: hver gang vi rammer en fjende, går der lidt tid,
        // før vi kan skade den samme fjende igen. Vi bruger en Dictionary til at
        // huske, hvornår vi sidst ramte netop denne fjende.
        if (m_lastHitTime.TryGetValue(hp, out var t) && Time.time - t < m_hitCooldown)
            return;

        // 5) Giver skade til fjenden når vi rammer. 
        hp.TakeDamage(m_Spikeskade);
        Debug.Log("SpikesRammer");
        m_lastHitTime[hp] = Time.time; // Gemmer tidspunktet for dette hit og nulstil cooldown for denne fjende (forhindrer spam-skade).
    }

    void ClearDict()
    {
        

        for (int i = 0; i < m_lastHitTime.Count; i++)
        {
            
        }

        // Cheaky work around, every 30 sec clear all cooldown for the demo. FIX ME LETER
        m_lastHitTime.Clear();
        Debug.LogWarning("(spikes.cs) CLEAR DICT");
    }
}
