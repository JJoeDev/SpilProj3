using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [Min(1f)] public float maxHealth = 100f;
    [SerializeField, Min(0f)]
    private float m_currentHealth = -1f; // -1 => init to max on Awake

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Events (optional for UI / other)")]
    public FloatEvent onDamaged;     // damage amount
    public FloatEvent onHealthRatio; // current/max (0..1)
    public UnityEvent onDied;

    [Header("Hit flash (optional)")]
    [Tooltip("Renderer that flashes red on hit (if null, first child Renderer is used).")]
    public Renderer flashRenderer;
    public float hitFlashDuration = 0.12f;

    [SerializeField] private ExplosionScript m_vehicleExplosion;

    private MaterialPropertyBlock m_propBlock;
    private WaitForSeconds m_cachedHitFlashYield;
    private Color m_originalRendererColor = Color.white;
    private bool m_isFlashing = false;

    public bool IsDead { get; private set; }
    public float Current => m_currentHealth;
    public float Max => maxHealth;

    void Awake()
    {
        if (m_currentHealth < 0f) m_currentHealth = maxHealth;
        m_currentHealth = Mathf.Clamp(m_currentHealth, 0f, maxHealth);
        IsDead = m_currentHealth <= 0f;

        if (flashRenderer == null)
            flashRenderer = GetComponentInChildren<Renderer>();

        if (flashRenderer != null && flashRenderer.sharedMaterial != null && flashRenderer.sharedMaterial.HasProperty("_Color"))
            m_originalRendererColor = flashRenderer.sharedMaterial.color;

        onHealthRatio?.Invoke(Current / Max);

        m_propBlock = new MaterialPropertyBlock();
        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);

        if (m_vehicleExplosion == null)
        {
            m_vehicleExplosion = GetComponent<ExplosionScript>()
                                ?? GetComponentInParent<ExplosionScript>()
                                ?? GetComponentInChildren<ExplosionScript>();

            // fallback: find nearest ExplosionScript in scene (only if none found above)
            if (m_vehicleExplosion == null)
            {
                var all = FindObjectsOfType<ExplosionScript>();
                if (all != null && all.Length > 0)
                {
                    float bestDist = float.MaxValue;
                    ExplosionScript best = null;
                    foreach (var ex in all)
                    {
                        float d = Vector3.Distance(transform.position, ex.transform.position);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            best = ex;
                        }
                    }
                    m_vehicleExplosion = best;
                }
            }
        }
    }

    void OnValidate()
    {
        if (m_currentHealth < 0f) m_currentHealth = -1f;
        m_currentHealth = Mathf.Clamp(m_currentHealth, -1f, Mathf.Max(1f, maxHealth));
        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        m_currentHealth = Mathf.Max(0f, m_currentHealth - amount);
        onDamaged?.Invoke(amount);
        onHealthRatio?.Invoke(Current / Max);

        if (!m_isFlashing && flashRenderer != null)
            StartCoroutine(FlashHit());

        if (m_currentHealth <= 0f && !IsDead)
        {
            IsDead = true;
            onDied?.Invoke();

            // Explosion on death (safe call)
            m_vehicleExplosion?.Explode();

            // Disable a specific controller by name if present (optional)
            var specific = GetComponent("PrometeoCarController") as Behaviour;
            if (specific != null) specific.enabled = false;

            // Freeze rigidbody
            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // disable all colliders on this object & children
            foreach (var col in GetComponentsInChildren<Collider>(true))
                if (col != null) col.enabled = false;
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        if (IsDead) return;

        m_currentHealth = Mathf.Min(maxHealth, m_currentHealth + amount);
        onHealthRatio?.Invoke(Current / Max);
    }

    public void ResetHealth(float? newMax = null)
    {
        if (newMax.HasValue) maxHealth = Mathf.Max(1f, newMax.Value);
        m_currentHealth = maxHealth;
        IsDead = false;
        onHealthRatio?.Invoke(1f);

        var specific = GetComponent("PrometeoCarController") as Behaviour;
        if (specific != null) specific.enabled = true;

        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
    }

    IEnumerator FlashHit()
    {
        m_isFlashing = true;
        if (flashRenderer != null && flashRenderer.sharedMaterial != null && flashRenderer.sharedMaterial.HasProperty("_Color"))
        {
            Color prior = m_originalRendererColor;
            flashRenderer.GetPropertyBlock(m_propBlock);
            m_propBlock.SetColor("_Color", Color.red);
            flashRenderer.SetPropertyBlock(m_propBlock);

            yield return m_cachedHitFlashYield;

            m_propBlock.SetColor("_Color", prior);
            flashRenderer.SetPropertyBlock(m_propBlock);
        }
        else
        {
            yield return m_cachedHitFlashYield;
        }
        m_isFlashing = false;
    }
}
