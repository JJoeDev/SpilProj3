using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCrashDamage : MonoBehaviour
{
    // Brug på spilleren skade afhænger af egen fart ved kollision.

    [Header("Layers")]
    [Tooltip("Hvilke lag skal udløse crash-skade (fx kun 'Wall').")]
    [SerializeField] private LayerMask m_wallLayers = 0;

    [Header("Damage Fra Fart")]
    [Tooltip("Minimum hastighed før crash kan give skade.")]
    [SerializeField] private float m_minSpeedForDamage = 5f;
    [Tooltip("Skade pr. 1 enhed/sek. over min-hastighed.")]
    [SerializeField] private float m_damagePerSpeed = 1f;
    [Tooltip("Afrund skade til heltal.")]
    [SerializeField] private bool m_roundDamageToInt = true;
    [Tooltip("Lille cooldown så samme crash ikke rammer 100 gange i træk.")]
    [SerializeField] private float m_cooldownSeconds = 0.05f;

    [Header("Afhængigheder")]
    [Tooltip("Spillerens Rigidbody. Findes automatisk hvis tom.")]
    [SerializeField] private Rigidbody m_rb;

    [Header("Debug")]
    [SerializeField] private bool m_log = false;


    private float m_lastPreSpeed = 0f;
    private float m_lastHitTime = -999f;

    void Awake()
    {
        // Find en Rigidbody i hierarkiet hvis ikke sat i Inspector
        if (m_rb == null)
        {
            m_rb = GetComponent<Rigidbody>();
            if (m_rb == null) m_rb = GetComponentInParent<Rigidbody>();
            if (m_rb == null) m_rb = GetComponentInChildren<Rigidbody>();
        }
    }


    void FixedUpdate()
    {
        // Gem pre kollisions hastighed hver fysik opdatering
        if (m_rb != null) m_lastPreSpeed = m_rb.velocity.magnitude;
        else m_lastPreSpeed = 0f; // Ingen RB → ingen fart
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return; // Sikkerhed

        // Kun reagér hvis vi rammer et objekt på et af de tilladte "væg"-lag
        GameObject other = collision.collider != null ? collision.collider.gameObject : collision.gameObject;
        if (((1 << other.layer) & m_wallLayers) == 0) return;

    
        if (Time.time - m_lastHitTime < m_cooldownSeconds) return; // Respektér cooldown så vi ikke påfører skade alt for tit

        float over = m_lastPreSpeed - m_minSpeedForDamage;
     

        if (over <= 0f) return;

        // Beregn skade ud fra overskydende fart
        float dmg = over * m_damagePerSpeed;
        if (m_roundDamageToInt) dmg = Mathf.Round(dmg);

      
        ApplySelfDamage(dmg, other);
        // Start cooldown
        m_lastHitTime = Time.time;
    }

    void ApplySelfDamage(float amount, GameObject source)
    {
        if (amount <= 0f) return;
        // find en IDamageable i forældre (typisk på roden) og kald TakeDamage
        foreach (var mb in GetComponentsInParent<MonoBehaviour>(true))
        {
            if (mb is IDamageable idmg)
            {
                idmg.TakeDamage(amount, source);
                return;
            }
        }

        gameObject.SendMessage("TakeDamage", amount, SendMessageOptions.DontRequireReceiver);
        gameObject.SendMessage("ApplyDamage", amount, SendMessageOptions.DontRequireReceiver);
    }

    void OnValidate()
    {
        //sørger for at tallet ikke kan blive negativ. 
        if (m_minSpeedForDamage < 0f) m_minSpeedForDamage = 0f;
        if (m_damagePerSpeed < 0f) m_damagePerSpeed = 0f;
        if (m_cooldownSeconds < 0f) m_cooldownSeconds = 0f;
    }
}
