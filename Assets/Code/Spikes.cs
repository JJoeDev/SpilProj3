using UnityEngine;
using System.Collections.Generic;

public class SpikeTrigger : MonoBehaviour
{
    [Header("Refs")]
    public Transform carTf;
    public Rigidbody rb;
    public LayerMask damageableLayers;

    [Header("Tuning")]
    [Range(0f, 1f)] public float minContactDot = 0.75f;
    [Range(0f, 100f)] public float zThreshold = 1f;
    [Range(0f, 100f)] public float minSideSpeed = 3f; // m/s
    [Range(0f, 100f)] public float minRelativeSpeed = 2f; // m/s
    [Range(0f, 100f)] public float damagePerHit = 1f; // Min. skade pr. hit

    [Header("Damage scaling")]
    [Tooltip("Ekstra skade pr. km/t relativ hastighed.")]
    public float damagePerKmh = 0.01f; // fx 0.01 => +0.01 dmg pr km/t
    [Tooltip("Maks vinkelbonus som faktor over 1 (0.5 = op til +50%).")]
    public float angleBonusMax = 0.5f; // 0.5 => op til +50% ved dot=1

    [Header("Timing")]
    public float cooldown = 1.5f; // sek pr. target mellem hits

    [Header("Filtering")]
    public string spikeTag = "Spike";
    public float selfSpikeProbeRadius = 0.02f; // radius (i meter) på en lille "OverlapSphere"-probe, der tjekker om kontaktpunktet rører en collider med tag "Spike"
    public bool requireSelfSpikeContact = true;

    private HealthManager m_selfHealth;
    private readonly HashSet<int> m_scraping = new HashSet<int>();
    private readonly Dictionary<int, float> m_lastHitTime = new Dictionary<int, float>();

    private void Start()
    {
        if (!carTf) carTf = transform.root;
        if (!rb) rb = carTf.GetComponent<Rigidbody>();
        m_selfHealth = carTf.GetComponent<HealthManager>();
    }
    private void OnTriggerStay(Collider other)
    {
        // Layer-gate
        if ((damageableLayers.value & (1 << other.gameObject.layer)) == 0) return;

        // Geometri
        Vector3 P = other.ClosestPoint(carTf.position);
        if (requireSelfSpikeContact)
        {
            var hits = Physics.OverlapSphere(P, Mathf.Max(0.001f, selfSpikeProbeRadius), ~0, QueryTriggerInteraction.Collide);
            bool spikeTouch = false;
            for (int i = 0; i < hits.Length; i++)
            {
                var c = hits[i];
                if (!c || !c.enabled) continue;
                if (!c.CompareTag(spikeTag)) continue;
                var t = c.transform;
                if (t == carTf || t.IsChildOf(carTf)) { spikeTouch = true; break; }
            }
            if (!spikeTouch) return;
        }
        Vector3 localP = carTf.InverseTransformPoint(P);

        bool isSide = Mathf.Abs(localP.z) <= zThreshold;
        Vector3 sideDir = (localP.x >= 0f) ? -carTf.right : carTf.right;

        // vinkel
        Vector3 approxNormalIn = (carTf.position - P).normalized;
        float dot = Vector3.Dot(approxNormalIn, sideDir);

        // Hastighed
        Vector3 vSelf = rb ? rb.GetPointVelocity(P) : Vector3.zero;
        Rigidbody otherRb = other.attachedRigidbody;
        Vector3 vOther = otherRb ? otherRb.GetPointVelocity(P) : Vector3.zero;

        Vector3 rel = vSelf - vOther;
        float sideSpeed = Mathf.Abs(Vector3.Dot(rel, sideDir)); // m/s
        float forwardCmp = Mathf.Abs(Vector3.Dot(rel, carTf.forward));
        float relMag = rel.magnitude; // m/s

        // Gates
        bool allowByDot = (dot >= 0.90f);
        bool validHit =
            isSide &&
            dot >= minContactDot &&
            relMag >= minRelativeSpeed &&
            (sideSpeed >= minSideSpeed || allowByDot) &&
            (allowByDot || sideSpeed >= forwardCmp * 0.10f);

        int id = otherRb ? otherRb.GetInstanceID() : other.GetInstanceID();
        bool wasScraping = m_scraping.Contains(id);

        if (!validHit) { if (wasScraping) m_scraping.Remove(id); return; }
        if (wasScraping) return; // én skade pr. “scrape”

        float now = Time.time;
        if (m_lastHitTime.TryGetValue(id, out float lastTime))
            if (now - lastTime < cooldown) return;

        m_scraping.Add(id);
        m_lastHitTime[id] = now;

        // Find health
        HealthManager target = FindTargetHealth(other);
        if (target && target != m_selfHealth)
        {
            // Skadeberegning
            float relKmh = relMag * 3.6f; // m/s -> km/t
            float damage = damagePerHit;

            // bonus pr. km/t
            if (damagePerKmh > 0f)
                damage += damagePerKmh * relKmh;

            // vinkelbonus
            if (angleBonusMax > 0f)
                damage *= Mathf.Lerp(1f, 1f + angleBonusMax, Mathf.Clamp01(dot));

            target.TakeDamage(damage);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        int id = other.attachedRigidbody ? other.attachedRigidbody.GetInstanceID()
                                         : other.GetInstanceID();
        m_scraping.Remove(id);
    }
    private HealthManager FindTargetHealth(Collider otherCol)
    {
        if (!otherCol) return null;

        var otherRb = otherCol.attachedRigidbody;
        if (otherRb)
        {
            return otherRb.GetComponent<HealthManager>()
                ?? otherRb.GetComponentInParent<HealthManager>()
                ?? otherRb.GetComponentInChildren<HealthManager>();
        }

        var t = otherCol.transform;
        return t.GetComponent<HealthManager>()
            ?? t.GetComponentInParent<HealthManager>()
            ?? t.GetComponentInChildren<HealthManager>();
    }
}
