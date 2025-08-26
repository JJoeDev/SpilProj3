using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
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

    [Tooltip("If true the GameObject will be destroyed after destroyDelay seconds when it dies.")]
    public bool destroyOnDeath = true;

    [Tooltip("Seconds to wait before destroying the GameObject after death.")]
    public float destroyDelay = 10f;

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

    // NY: toggle pr instance: skal denne instans bruge den indbyggede world/OnGUI healthbar?
    [Tooltip("If false: this instance will NOT draw the built-in world OnGUI health bar.")]
    public bool enableWorldHealthBar = true;

    [Header("Hit flash (optional)")]
    [Tooltip("Renderer that flashes red on hit (if null, first child Renderer is used).")]
    public Renderer flashRenderer;
    public float hitFlashDuration = 0.12f;

    [Header("Global display")]
    [Tooltip("Requested max simultaneous bars for this instance. Global cap will be the maximum requested by awakened Healths (default 3).")]
    public int maxVisibleBars = 3;

    // --- Internal / static for 'recent attacked' logic ---
    private class RecentEntry { public Health h; public float time; }
    private static readonly List<RecentEntry> s_recentAttacks = new List<RecentEntry>();
    private static int s_globalMaxVisibleBars = 3;
    private bool m_destroyScheduled = false;

    // cache
    private Camera m_mainCam;
    private Transform m_playerTransform;
    private Color m_originalRendererColor = Color.white;
    private bool m_isFlashing = false;

    // per-instance last hit time
    private float m_lastHitTime = -999f;

    // Exposed state
    public bool IsDead { get; private set; }
    public float Current => m_currentHealth;
    public float Max => maxHealth;

    // Cached component arrays (lazy)
    private Renderer[] m_cachedRenderers;
    private Canvas[] m_cachedCanvases;
    private ParticleSystem[] m_cachedParticleSystems;
    private AudioSource[] m_cachedAudioSources;
    private Collider[] m_cachedColliders;
    private bool m_cachedChildrenCollected = false;

    // MaterialPropertyBlock for flash color changes (no material instancing)
    private MaterialPropertyBlock m_propBlock;

    // Cached WaitForSeconds for flash to avoid allocations
    private WaitForSeconds m_cachedHitFlashYield;

    void Awake()
    {
        m_mainCam = Camera.main;
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) m_playerTransform = playerGO.transform;
        else if (m_mainCam != null) m_playerTransform = m_mainCam.transform;

        if (m_currentHealth < 0f) m_currentHealth = maxHealth;
        m_currentHealth = Mathf.Clamp(m_currentHealth, 0f, maxHealth);
        IsDead = m_currentHealth <= 0f;

        // default flash renderer
        if (flashRenderer == null)
            flashRenderer = GetComponentInChildren<Renderer>();

        if (flashRenderer != null && flashRenderer.sharedMaterial != null && flashRenderer.sharedMaterial.HasProperty("_Color"))
            m_originalRendererColor = flashRenderer.sharedMaterial.color;

        onHealthRatio?.Invoke(Current / Max);

        // update global max visible bars to respect inspector requests
        s_globalMaxVisibleBars = Mathf.Max(s_globalMaxVisibleBars, Mathf.Max(1, maxVisibleBars));

        // prepare property block
        m_propBlock = new MaterialPropertyBlock();

        // cache WaitForSeconds for flash to reduce allocations
        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);

        // NOTE: heavy GetComponentsInChildren(...) calls removed from Awake.
        // Instead we use EnsureCachedChildren() lazily when we actually need child arrays (e.g. on death/ScheduleDestroy).
    }

    void OnValidate()
    {
        // keep currentHealth within sensible bounds in editor
        if (m_currentHealth < 0f) m_currentHealth = -1f;
        m_currentHealth = Mathf.Clamp(m_currentHealth, -1f, Mathf.Max(1f, maxHealth));
        maxVisibleBars = Mathf.Max(1, maxVisibleBars);
        destroyDelay = Mathf.Max(0f, destroyDelay);

        // Keep cached WaitForSeconds in sync in editor (if you change hitFlashDuration)
        m_cachedHitFlashYield = new WaitForSeconds(hitFlashDuration);
    }

    /// <summary>
    /// Apply damage. Amount must be > 0.
    /// Updates recent list and last-hit timestamp.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        m_currentHealth = Mathf.Max(0f, m_currentHealth - amount);
        onDamaged?.Invoke(amount);
        onHealthRatio?.Invoke(Current / Max);

        m_lastHitTime = Time.time;

        // only mark in global recent list if player is nearby (mirrors earlier behaviour)
        bool markGlobally = false;
        if (m_playerTransform != null)
        {
            float d = Vector3.Distance(m_playerTransform.position, transform.position);
            if (d <= showDistance) markGlobally = true;
        }
        else
        {
            // fallback: mark anyway
            markGlobally = true;
        }

        if (markGlobally && enableWorldHealthBar)
        {
            AddOrUpdateRecent(this, m_lastHitTime);
        }

        // flash renderer
        if (!m_isFlashing && flashRenderer != null)
            StartCoroutine(FlashHit());

        if (m_currentHealth <= 0f && !IsDead)
        {
            IsDead = true;
            onDied?.Invoke();

            // try to disable PrometeoCarController if present (best-effort)
            var specific = GetComponent("PrometeoCarController") as Behaviour;
            if (specific != null) specific.enabled = false;

            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                // stop motion and make kinematic so it no longer interacts physically
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // disable all colliders to avoid further collisions after death
            EnsureCachedChildren(); // only now we gather children arrays if needed
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

            // schedule destruction after delay if requested (default 10s)
            if (destroyOnDeath && !m_destroyScheduled)
            {
                StartCoroutine(ScheduleDestroy());
            }
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead)
            return;

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

        // allow re-scheduling destruction in future if needed
        m_destroyScheduled = false;
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
            // Use MaterialPropertyBlock so we DON'T instantiate a new Material
            Color prior = m_originalRendererColor;

            flashRenderer.GetPropertyBlock(m_propBlock);
            m_propBlock.SetColor("_Color", Color.red);
            flashRenderer.SetPropertyBlock(m_propBlock);

            // use cached WaitForSeconds to avoid allocations
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

    // coroutine that hides visual/audio parts immediately and destroys after delay
    private IEnumerator ScheduleDestroy()
    {
        m_destroyScheduled = true;

        // Ensure we have cached children before use
        EnsureCachedChildren();

        // 1) Hide visual components immediately (keep GameObject active so coroutine continues)
        if (m_cachedRenderers != null)
        {
            foreach (var r in m_cachedRenderers)
                if (r != null) r.enabled = false;
        }
        else
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) if (r != null) r.enabled = false;
        }

        if (m_cachedCanvases != null)
        {
            foreach (var c in m_cachedCanvases)
                if (c != null) c.enabled = false;
        }
        else
        {
            var canvases = GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases) if (c != null) c.enabled = false;
        }

        if (m_cachedParticleSystems != null)
        {
            foreach (var ps in m_cachedParticleSystems)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Clear(true);
                }
            }
        }
        else
        {
            var particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Clear(true);
                }
            }
        }

        if (m_cachedAudioSources != null)
        {
            foreach (var a in m_cachedAudioSources)
                if (a != null) a.Stop();
        }
        else
        {
            var audioSources = GetComponentsInChildren<AudioSource>(true);
            foreach (var a in audioSources) if (a != null) a.Stop();
        }

        // Ensure colliders remain disabled (defensive)
        if (m_cachedColliders != null)
        {
            foreach (var col in m_cachedColliders)
                if (col != null) col.enabled = false;
        }
        else
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders) if (col != null) col.enabled = false;
        }

        // 2) Wait for the requested delay
        if (destroyDelay > 0f)
            yield return new WaitForSeconds(destroyDelay);
        else
            yield return null; // at least one frame

        // 3) Destroy the GameObject
        Destroy(gameObject);
    }

    // Add or update this Health in the global recent list, then prune/trim
    private static void AddOrUpdateRecent(Health h, float time)
    {
        // remove null entries first (non-allocating)
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

        // sort newest first (use static compare to avoid lambda allocation)
        s_recentAttacks.Sort(CompareRecent);

        // prune entries older than their showDuration
        float now = Time.time;
        float maxShow = 0f;
        for (int i = 0; i < s_recentAttacks.Count; i++)
        {
            var e = s_recentAttacks[i];
            if (e.h != null) maxShow = Mathf.Max(maxShow, e.h.showDuration);
        }
        if (maxShow > 0f)
        {
            // Remove entries older than maxShow - use for-loop removal
            for (int i = s_recentAttacks.Count - 1; i >= 0; i--)
            {
                if (now - s_recentAttacks[i].time > maxShow)
                    s_recentAttacks.RemoveAt(i);
            }
        }

        // Trim to global cap
        if (s_recentAttacks.Count > s_globalMaxVisibleBars)
            s_recentAttacks.RemoveRange(s_globalMaxVisibleBars, s_recentAttacks.Count - s_globalMaxVisibleBars);
    }

    // non-alloc remove-null helper
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

    // Simple screen-space world bar using OnGUI (good for quick testing / prototypes).
    void OnGUI()
    {
        // NY: hvis denne instans ikke ønsker at bruge world-bar, så gør vi ingenting.
        if (!enableWorldHealthBar) return;

        if (m_currentHealth <= 0f) return;
        if (m_mainCam == null) m_mainCam = Camera.main;
        if (m_mainCam == null) return;

        float now = Time.time;

        // Remove dead/null entries occasionally
        RemoveNullEntries();

        // Determine whether this instance is eligible to draw:
        bool timeOk = (now - m_lastHitTime) <= showDuration;
        // find index in recent list (newest=0)
        int idx = s_recentAttacks.FindIndex(e => e.h == this);
        bool inRecentTop = idx >= 0 && idx < s_globalMaxVisibleBars;

        // Distance check
        if (m_playerTransform != null)
        {
            if (Vector3.Distance(m_playerTransform.position, transform.position) > showDistance) return;
        }

        // Decide visibility:
        bool shouldShow = false;
        if (onlyShowIfLastAttacked)
        {
            // only show if we're among the recent top-N and within that entry's duration
            if (inRecentTop)
            {
                var entry = s_recentAttacks[idx];
                if (now - entry.time <= showDuration) shouldShow = true;
            }
        }
        else
        {
            // show if we were hit recently AND we're among the top-N recent entries (to cap total bars)
            if (timeOk && inRecentTop) shouldShow = true;
        }

        if (!shouldShow) return;

        Vector3 worldPos = transform.position + healthbarWorldOffset;
        Vector3 screenPos = m_mainCam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return; // behind camera

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

    // Public accessor to change global max bars at runtime if preferred
    public static void SetGlobalMaxVisibleBars(int n)
    {
        s_globalMaxVisibleBars = Mathf.Max(1, n);
    }

    // NY: public metode så andre scripts kan slå denne instans' world-bar til eller fra
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
