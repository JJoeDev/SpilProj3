using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SocialPlatforms;

public class SpikeTrigger : MonoBehaviour
{
    [Header("Refs")]
    public Transform carTf; // Bilens transform.Bruges til at måle retninger (left/right/forward) og om et hit ligger ved siden(lokal Z).
    public Rigidbody rb;
    public LayerMask damageableLayers; // Hvilke layers må tage skade. Alt på andre layers ignoreres.

    [Header("Tuning")]
    [Range(0f, 1f)] public float minContactDot = 0.75f; // Vinkelkrav for at det tæller som “side-hit”. 1 = perfekt parallelt med siden; 0 = 90°. Jo højere tal, jo mere “rent” sideswipe kræves.
    [Range(0f, 100f)] public float zThreshold = 1f; // (meter): Hvor langt fra bilens midterlinje i lokal Z et hit stadig betragtes som “side” (filtrerer front/bag).
    [Range(0f, 100f)] public float minSideSpeed = 3f; // (m/s): Minimum lateral (sideværts) relativ hastighed, før et hit tæller (forhindrer næsten-stillestående skrab).
    [Range(0f, 100f)] public float minRelativeSpeed = 2f; // minRelativeSpeed (m/s): Minimum total relativ hastighed mellem biler for at undgå “mikro-kontakt”.
    [Range(0f, 100f)] public float damagePerHit = 1f; // damagePerHit: Grundskade pr. gyldigt hit (før evt. fremtidige bonusser).

    [Header("Timing")]
    public float cooldown = 1.5f; // delay mellem hits pr. target (sek)

    private Health m_selfHealth;
    private readonly HashSet<int> m_scraping = new HashSet<int>(); // kan godt være i skal bruge jeres health script som i har lavet. 
    private readonly Dictionary<int, float> m_lastHitTime = new Dictionary<int, float>(); // Hvornår hver modstander sidst fik skade — håndterer cooldown pr. modstander.

    private void Start()
    {
        if (!carTf) carTf = transform.root;
        if (!rb) rb = carTf.GetComponent<Rigidbody>();
        m_selfHealth = carTf.GetComponent<Health>();

        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if ((damageableLayers.value & (1 << other.gameObject.layer)) == 0) return;

        Vector3 P = other.ClosestPoint(carTf.position);
        Vector3 localP = carTf.InverseTransformPoint(P);

        bool isSide = Mathf.Abs(localP.z) <= zThreshold;
        Vector3 sideDir = (localP.x >= 0f) ? -carTf.right : carTf.right;

        Vector3 approxNormalIn = (carTf.position - P).normalized;
        float dot = Vector3.Dot(approxNormalIn, sideDir);

        Vector3 vSelf = rb ? rb.GetPointVelocity(P) : Vector3.zero;
        Rigidbody otherRb = other.attachedRigidbody;
        Vector3 vOther = otherRb ? otherRb.GetPointVelocity(P) : Vector3.zero;

        Vector3 rel = vSelf - vOther;
        float sideSpeed = Mathf.Abs(Vector3.Dot(rel, sideDir));
        float forwardCmp = Mathf.Abs(Vector3.Dot(rel, carTf.forward));
        float relMag = rel.magnitude;

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
        if (wasScraping) return;

        float now = Time.time;
        if (m_lastHitTime.TryGetValue(id, out float lastTime))
        {
            if (now - lastTime < cooldown) return;
        }

        m_scraping.Add(id);
        m_lastHitTime[id] = now;

        Health target = FindTargetHealth(other);
        if (target && target != m_selfHealth) // sikker at vi ikke rasiker at vi skader oselv. 
        {
            target.ApplyDamage(damagePerHit);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        int id = other.attachedRigidbody ? other.attachedRigidbody.GetInstanceID()
                                         : other.GetInstanceID();
        m_scraping.Remove(id);
    }
    private Health FindTargetHealth(Collider otherCol)
    {
        if (!otherCol) return null;

        var otherRb = otherCol.attachedRigidbody;
        if (otherRb)
        {
            return otherRb.GetComponent<Health>()
                ?? otherRb.GetComponentInParent<Health>()
                ?? otherRb.GetComponentInChildren<Health>();
        }

        var t = otherCol.transform;
        return t.GetComponent<Health>()
            ?? t.GetComponentInParent<Health>()
            ?? t.GetComponentInChildren<Health>();
    }
}
