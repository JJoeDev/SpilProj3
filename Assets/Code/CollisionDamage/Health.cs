using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    // virker både til player og enemy 
    [Header("Health")]
    [Min(1f)] public float maxHealth = 100f;
    [SerializeField, Min(0f)]
    private float m_currentHealth = -1f; // -1 => init to max on Awake

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Events (optional for UI / other)")]
    public FloatEvent onDamaged; // damage amount
    public FloatEvent onHealthRatio; // current/max (0..1)
    public UnityEvent onDied;

    // --- World-healthbar / visual feedback (optional) ---
    [Header("World Healthbar (OnGUI)")]
    [Tooltip("Max distance player->this for healthbar to be allowed to show.")]
    public float showDistance = 12f;
    [Tooltip("Seconds the bar stays visible after last attack.")]
    public float showDuration = 3f;
    public Vector3 healthbarWorldOffset = new Vector3(0f, 2f, 0f);
    public float barWidth = 100f;
    public float barHeight = 12f;
    public Color bgColor = Color.black;
    public Color fullColor = Color.green;
    public Color emptyColor = Color.red;
    [Tooltip("If true: only show this bar when this instance was recently attacked and is within the global top-N recent list.")]
    public bool onlyShowIfLastAttacked = true;

    [Tooltip("If false: this instance will NOT draw the built-in world OnGUI health bar.")]
    public bool enableWorldHealthBar = true;

    [Header("Hit flash (optional)")]
    [Tooltip("Renderer that flashes red on hit (if null, first child Renderer is used).")]
    public Renderer flashRenderer;
    public float hitFlashDuration = 0.12f;

    [Header("Global display")]
    [Tooltip("Requested max simultaneous bars for this instance. Global cap will be the maximum requested by awakened Healths (default 3).")]
    public int maxVisibleBars = 3;

    private class RecentEntry { public Health h; public float time; }
    private static readonly List<RecentEntry> s_recentAttacks = new List<RecentEntry>();
    private static int s_globalMaxVisibleBars = 3;


    private Camera m_mainCam;
    private Transform m_playerTransform;
    private Color m_originalRendererColor = Color.white;
    private bool m_isFlashing = false;


    private float m_lastHitTime = -999f;

    
    public bool IsDead { get; private set; }
    public float Current => m_currentHealth;
    public float Max => maxHealth;

    
    private Renderer[] m_cachedRenderers;
    private Canvas[] m_cachedCanvases;
    private ParticleSystem[] m_cachedParticleSystems;
    private AudioSource[] m_cachedAudioSources;
    private Collider[] m_cachedColliders;
    private bool m_cachedChildrenCollected = false;

   
    private MaterialPropertyBlock m_propBlock;

 
    private WaitForSeconds m_cachedHitFlashYield;

    [SerializeField] private ExplosionScript m_vehicleExplosion;


    void Awake()
    {
        m_mainCam = Camera.main;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) m_playerTransform = playerGO.transform;
        else if (m_mainCam != null) m_playerTransform = m_mainCam.transform;

        if (m_currentHealth < 0f) m_currentHealth = maxHealth;
        m_currentHealth = Mathf.Clamp(m_currentHealth, 0f, maxHealth);
        IsDead = m_currentHealth <= 0f;

        if (flashRenderer == null)
            flashRenderer = GetComponentInChildren<Renderer>();

        if (flashRenderer != null && flashRenderer.sharedMaterial != null && flashRenderer.sharedMaterial.HasProperty("_Color"))
            m_originalRendererColor = flashRenderer.sharedMaterial.color;

        onHealthRatio?.Invoke(Current / Max);

  
        s_globalMaxVisibleBars = Mathf.Max(s_globalMaxVisibleBars, Mathf.Max(1, maxVisibleBars));

  
        m_propBlock = new MaterialPropertyBlock();


        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);

        if (m_vehicleExplosion == null)
        {
            m_vehicleExplosion = GetComponent<ExplosionScript>()
                                ?? GetComponentInParent<ExplosionScript>()
                                ?? GetComponentInChildren<ExplosionScript>();
        }

        // --- fallback: find nærmeste ExplosionScript i scenen (hvis ingen fundet endnu) ---
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

    void OnValidate()
    {
        if (m_currentHealth < 0f) m_currentHealth = -1f;
        m_currentHealth = Mathf.Clamp(m_currentHealth, -1f, Mathf.Max(1f, maxHealth));
        maxVisibleBars = Mathf.Max(1, maxVisibleBars);

        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        m_currentHealth = Mathf.Max(0f, m_currentHealth - amount);
        onDamaged?.Invoke(amount);
        onHealthRatio?.Invoke(Current / Max);

        m_lastHitTime = Time.time;

        bool markGlobally = false;
        if (m_playerTransform != null)
        {
            float d = Vector3.Distance(m_playerTransform.position, transform.position);
            if (d <= showDistance) markGlobally = true;
        }
        else
        {
            markGlobally = true;
        }

        if (markGlobally && enableWorldHealthBar)
        {
            AddOrUpdateRecent(this, m_lastHitTime);
        }

        if (!m_isFlashing && flashRenderer != null)
            StartCoroutine(FlashHit());

        if (m_currentHealth <= 0f && !IsDead)
        {
            IsDead = true;
            onDied?.Invoke();

            // Kald explosion på death (sikker null-check)
            m_vehicleExplosion?.Explode();

            var specific = GetComponent("PrometeoCarController") as Behaviour;
            if (specific != null) specific.enabled = false;

            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            EnsureCachedChildren();
            if (m_cachedColliders != null)
            {
                foreach (var col in m_cachedColliders)
                    if (col != null) col.enabled = false;
            }
            else
            {
                foreach (var col in GetComponentsInChildren<Collider>(true))
                    if (col != null) col.enabled = false;
            }
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        if (IsDead)
        {
            // Hvis du vil genoplive i stedet for at ignorere, kald ResetHealth() her før Heal
            return;
        }

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

    public void MarkAsAttackedExternally()
    {
        m_lastHitTime = Time.time;
        if (enableWorldHealthBar) AddOrUpdateRecent(this, m_lastHitTime);
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

    private static void AddOrUpdateRecent(Health h, float time)
    {

        RemoveNullEntries();

        var existing = s_recentAttacks.Find(e => e.h == h);
        if (existing != null)
        {
            existing.time = time;
        }
        else
        {
            s_recentAttacks.Insert(0, new RecentEntry { h = h, time = time });
        }


        s_recentAttacks.Sort(CompareRecent);

        float now = Time.time;
        float maxShow = 0f;
        for (int i = 0; i < s_recentAttacks.Count; i++)
        {
            var e = s_recentAttacks[i];
            if (e.h != null) maxShow = Mathf.Max(maxShow, e.h.showDuration);
        }
        if (maxShow > 0f)
        {
  
            for (int i = s_recentAttacks.Count - 1; i >= 0; i--)
            {
                if (now - s_recentAttacks[i].time > maxShow)
                    s_recentAttacks.RemoveAt(i);
            }
        }


        if (s_recentAttacks.Count > s_globalMaxVisibleBars)
            s_recentAttacks.RemoveRange(s_globalMaxVisibleBars, s_recentAttacks.Count - s_globalMaxVisibleBars);
    }

    private static void RemoveNullEntries()
    {
        int write = 0;
        for (int i = 0; i < s_recentAttacks.Count; i++)
        {
            if (s_recentAttacks[i].h != null)
            {
                if (write != i) s_recentAttacks[write] = s_recentAttacks[i];
                write++;
            }
        }
        if (write < s_recentAttacks.Count)
            s_recentAttacks.RemoveRange(write, s_recentAttacks.Count - write);
    }

    private static int CompareRecent(RecentEntry a, RecentEntry b) => b.time.CompareTo(a.time);

    void OnGUI()
    {

        if (!enableWorldHealthBar) return;

        if (m_currentHealth <= 0f) return;
        if (m_mainCam == null) m_mainCam = Camera.main;
        if (m_mainCam == null) return;

        float now = Time.time;

        RemoveNullEntries();


        bool timeOk = (now - m_lastHitTime) <= showDuration;

        int idx = s_recentAttacks.FindIndex(e => e.h == this);
        bool inRecentTop = idx >= 0 && idx < s_globalMaxVisibleBars;


        if (m_playerTransform != null)
        {
            if (Vector3.Distance(m_playerTransform.position, transform.position) > showDistance) return;
        }


        bool shouldShow = false;
        if (onlyShowIfLastAttacked)
        {

            if (inRecentTop)
            {
                var entry = s_recentAttacks[idx];
                if (now - entry.time <= showDuration) shouldShow = true;
            }
        }
        else
        {

            if (timeOk && inRecentTop) shouldShow = true;
        }

        if (!shouldShow) return;

        Vector3 worldPos = transform.position + healthbarWorldOffset;
        Vector3 screenPos = m_mainCam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return; 

        float x = screenPos.x - barWidth * 0.5f;
        float y = Screen.height - screenPos.y - barHeight;

        // background
        var bgRect = new Rect(x, y, barWidth, barHeight);
        GUI.color = bgColor;
        GUI.DrawTexture(bgRect, Texture2D.whiteTexture);

        // fill
        float t = Mathf.Clamp01(m_currentHealth / Mathf.Max(1f, maxHealth));
        var fgRect = new Rect(x + 1f, y + 1f, (barWidth - 2f) * t, barHeight - 2f);
        GUI.color = Color.Lerp(emptyColor, fullColor, t);
        GUI.DrawTexture(fgRect, Texture2D.whiteTexture);

        GUI.color = Color.white;
    }


    public static void SetGlobalMaxVisibleBars(int n)
    {
        s_globalMaxVisibleBars = Mathf.Max(1, n);
    }

    public void SetWorldBarEnabled(bool enabled)
    {
        enableWorldHealthBar = enabled;

        // hvis vi slår off: fjern instansen fra recent-listen så den ikke længere tager en plads i top-N
        if (!enabled)
        {
            s_recentAttacks.RemoveAll(e => e.h == this);
        }
        else
        {
            // hvis vi tænder den, og den blev ramt for nylig, gen-tilføj så den kan vises:
            if (m_lastHitTime > 0f)
                AddOrUpdateRecent(this, m_lastHitTime);
        }
    }

    // Lazy gather of child arrays (only when needed)
    private void EnsureCachedChildren()
    {
        if (m_cachedChildrenCollected) return;
        m_cachedRenderers = GetComponentsInChildren<Renderer>(true);
        m_cachedCanvases = GetComponentsInChildren<Canvas>(true);
        m_cachedParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        m_cachedAudioSources = GetComponentsInChildren<AudioSource>(true);
        m_cachedColliders = GetComponentsInChildren<Collider>(true);
        m_cachedChildrenCollected = true;
    }
}
