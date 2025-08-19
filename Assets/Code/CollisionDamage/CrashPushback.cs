using UnityEngine;
using System.Collections;

public class CrashPushback : MonoBehaviour
{
    [Header("Mål & Lag")]
    [Tooltip("Hvilke lag udløser et tilbage-skub (sæt fx Fjende, Væg, etc.).")]
    [SerializeField] private LayerMask skubLag = ~0;
    [Tooltip("Skub også, hvis det er trigger-colliders (fx fjender med trigger).")]
    [SerializeField] private bool ogsåTriggers = true;

    [Header("Skub baseret på fart")]
    [Tooltip("Grundafstand, der altid skubbes (meter).")]
    [SerializeField] private float baseSkubAfstand = 0.5f; 
    [Tooltip("Ekstra skub pr. 1 enhed/sek. fart (meter pr. (enhed/sek)).")]
    [SerializeField] private float skubAfstandPrFart = 0.08f; // skubber 0.08f længere væk per fart/speed
    [Tooltip("Minimum og maksimum skub (meter).")]
    [SerializeField] private float minSkub = 0.2f; // min skub tilbage afstand...
    [SerializeField] private float maxSkub = 2.5f; // hvor langt kan bilen max blive skubet tilbage?

    [Tooltip("Varighed ved lav fart (sek).")]
    [SerializeField] private float varighedLavFart = 0.12f;
    [Tooltip("Varighed ved høj fart (sek).")]
    [SerializeField] private float varighedHøjFart = 0.22f;

    [Tooltip("Smooth-kurve for skub (0..1 tid → 0..1 distance).")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Retning & tærskler")]
    [Tooltip("Brug kollisionsfladens normal som retning (mere fysisk korrekt).")]
    [SerializeField] private bool brugKollisionsNormal = true;
    [Tooltip("Mindste fart for at udløse skub (for at undgå mikro-skub).")]
    [SerializeField] private float minHastighedForSkub = 0.1f;
    [Tooltip("Cooldown mellem skub, så det ikke spammer (sek).")]
    [SerializeField] private float cooldown = 0.25f;
    [Tooltip("Dæmp skub, hvis slaget ikke er frontalt (dot med -forward).")]
    [Range(0f, 1f)][SerializeField] private float vinkelDæmpning = 1f;

    [Header("Styring under skub")]
    [Tooltip("Deaktivér PlayerMovement i kort tid under skub for tydelig effekt.")]
    [SerializeField] private bool låsBevægelseUnderSkub = true;

    [Header("Hastighed efter kollision")]
    [Tooltip("Farten sættes ned til denne værdi (eller lavere).")]
    [SerializeField] private float maxSpeedEfterKollision = 1f;

    private PlayerMovementTest _pm;
    private Rigidbody _rb;
    private float _næsteSkubTid;
    private Coroutine _aktivSkub;

    void Awake()
    {
        _pm = GetComponent<PlayerMovementTest>();
        _rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time < _næsteSkubTid) return;
        if (!ErRelevant(collision.collider)) return;

        float fart = HentAktuelFart();
        if (fart < minHastighedForSkub) return;

        Vector3 dir = BestemSkubRetning(collision);
        StartSkub(dir, fart);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ogsåTriggers) return;
        if (Time.time < _næsteSkubTid) return;
        if (!ErRelevant(other)) return;

        float fart = HentAktuelFart();
        if (fart < minHastighedForSkub) return;

        
        Vector3 dir = -transform.forward;
        StartSkub(dir, fart);
    }

    private bool ErRelevant(Collider col)
    {
        return ((1 << col.gameObject.layer) & skubLag) != 0;
    }

    private float HentAktuelFart()
    {
        if (_pm != null) return _pm.speed;
        if (_rb != null) return _rb.velocity.magnitude; // fallback hvis ingen PlayerMovement
        return 0f;
    }

    private Vector3 BestemSkubRetning(Collision c)
    {
        if (brugKollisionsNormal && c.contactCount > 0)
        {
            Vector3 sum = Vector3.zero;
            int cnt = Mathf.Min(c.contactCount, 8);
            for (int i = 0; i < cnt; i++) sum += c.GetContact(i).normal;
            Vector3 n = sum.normalized;
            if (n.sqrMagnitude > 0.0001f) return n;
        }
        return -transform.forward; // fallback: skub tilbage i bilens modsatte kørselsretning
    }

    private void StartSkub(Vector3 retning, float fart)
    {
        // Sæt farten ned
        if (_pm != null)
            _pm.currentSpeed = Mathf.Min(_pm.currentSpeed, maxSpeedEfterKollision);

        // Beregn vinkel-dæmpning (frontalt slag → 1, skråt → mindre)
        float frontal = Mathf.Clamp01(Vector3.Dot(-transform.forward, retning.normalized));
        float vinkelFaktor = Mathf.Lerp(1f, frontal, vinkelDæmpning); // hvis vinkelDæmpning=1 → brug frontal helt

        // Dynamisk skub-afstand: base + fart*koefficient, clampet
        float dynamiskAfstand = Mathf.Clamp(baseSkubAfstand + fart * skubAfstandPrFart, minSkub, maxSkub);
        dynamiskAfstand *= vinkelFaktor;

        // Dynamisk varighed: lerp mellem lav/høj fart varighed
        float norm = 0f;
        if (_pm != null && _pm.MaxSpeed > 0f) norm = Mathf.Clamp01(fart / _pm.MaxSpeed);
        else norm = Mathf.Clamp01(fart / 20f); // konservativ fallback
        float dynamiskVarighed = Mathf.Lerp(varighedLavFart, varighedHøjFart, norm);

        // Start skub-rutine
        if (_aktivSkub != null) StopCoroutine(_aktivSkub);
        _aktivSkub = StartCoroutine(SkubRutine(retning.normalized, dynamiskAfstand, dynamiskVarighed));

        _næsteSkubTid = Time.time + cooldown;
    }

    private IEnumerator SkubRutine(Vector3 dir, float afstand, float varighed)
    {
        bool genAktiver = false;
        if (låsBevægelseUnderSkub && _pm != null && _pm.enabled)
        {
            _pm.enabled = false; // kort “stun” så skubbet kan mærkes
            genAktiver = true;
        }

        Vector3 start = _rb ? _rb.position : transform.position;
        float t = 0f;

        while (t < varighed)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / varighed);
            float k = ease != null ? Mathf.Clamp01(ease.Evaluate(u)) : Mathf.SmoothStep(0f, 1f, u);

            Vector3 target = start + dir * (afstand * k);

            if (_rb != null && !_rb.isKinematic)
                _rb.MovePosition(target);     // respekterer fysik-kollisioner
            else
                transform.position = target;  // simpel fallback

            yield return null;
        }

        if (genAktiver && _pm != null) _pm.enabled = true;
        _aktivSkub = null;
    }
}
