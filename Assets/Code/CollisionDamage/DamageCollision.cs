using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class DamageCollision : MonoBehaviour
{
    public Rigidbody rb; // Reference til dette objekts Rigidbody. Hvis den er tom i inspector, bliver den typisk udfyldt i Awake().
    public float currentDamage = 0f; // Den sidst beregnede skadeværdi (fx ved en kollision) — bruges til debugging/visning.
    public float maxDamage = 100f; // Maksimum skade der kan påføres i et enkelt hit (skade-cappen).
    public float minSpeedBeforeDamable = 5f; // Mindste hastighed (m/s) før et hit kan gøre skade. (5 m/s ≈ 18 km/t)

    [Range(0f, 1f)]
    public float contactDotThreshold = 0.5f; // Tærskel (0..1) for om kontaktpunktet ligger "foran" bilen. Højere = strengere front-check.

    [Header("Damage Settings")]
    public LayerMask damageableLayers = ~0; // Hvilke layers kan tage skade. ~0 betyder "alle layers" som standard.

    [Header("Optional: assign your car's body/mesh colliders here (auto-filled if empty)")]
    public Collider[] myColliders; // Liste over colliders der tilhører køretøjet (bruges til at undgå at registrere egne collider-kollisioner).

    [Header("Facing rules")]
    [Tooltip("Dot threshold to consider a vehicle 'facing' the other. Range -1..1. 0.1 is mild.")]
    [Range(-1f, 1f)]
    public float facingDotThreshold = 0.1f; // Tærskel for om to fremad-vektorer betragtes som "vendt mod" hinanden (dot-product). 0.1 = ret mild.

    [Header("Speed multiplier (multiplicative mode)")]
    [Tooltip("Fraction per km/h. Example: 0.01 = +1% damage per km/h (at 100 km/h -> 2x).")]
    public float extraDamagePerKmh = 0.01f; // Hvor meget skaden skalerer per km/t. Fx 0.01 = +1% damage pr. km/t (brug m/s*3.6 for km/t).
    void Awake()
    {
        // ---------- INIT / CACHING ----------
        // Hvis rb ikke er sat i inspector, prøv at finde Rigidbody på GameObject
        if (rb == null)
        {
            gameObject.TryGetComponent<Rigidbody>(out rb);
        }
        // Hvis myColliders er tom, find alle colliders i objekthierarkiet og gem dem
        if (myColliders == null || myColliders.Length == 0)
            myColliders = GetComponentsInChildren<Collider>(includeInactive: true);
        // For hver collider: sørg for at der er en CollisionForwarder, og peg den tilbage til dette script (owner)
        foreach (var c in myColliders)
        {
            if (c == null) continue;
            if (c.gameObject.GetComponentInParent<DamageCollision>() != this) continue;

            if (!c.gameObject.TryGetComponent<CollisionForwarder>(out CollisionForwarder forwarder))
            {
                forwarder = c.gameObject.AddComponent<CollisionForwarder>();
                forwarder.hideFlags = HideFlags.None;
            }
            forwarder.owner = this;
        }
    }
    // Root fallback - hvis kollisionsbesked kommer til denne component direkte
    // Root fallback - forwards to handler
    void OnCollisionEnter(Collision collision)
    {
        HandleCollisionFromChild(collision);
    }
    // HOVEDLOGIK: håndter kollisioner der kommer via en child collider
    private void HandleCollisionFromChild(Collision collision)
    {
        // 1) Grundlæggende checks
        if (rb == null) return;

        // contact point: hvis der ikke er nogen kontakter, afbryd
        if (collision == null || collision.contactCount == 0) return;
        ContactPoint contactPoint = collision.GetContact(0); // Vi ser på det første kontaktpunkt

        // Hvad ramte os? Nogle gange kan contactPoint.otherCollider være null, så fallback til collision.collider
        Collider otherCollider = (contactPoint.otherCollider != null) ? contactPoint.otherCollider : collision.collider;
        if (otherCollider == null) return;

        // Ignorer hvis det er en af vores egne colliders
        if (myColliders != null && System.Array.IndexOf(myColliders, otherCollider) >= 0) return;

        // Find rigidbody på den anden part (hvis ingen rigidbody -> ingen skadeberegning)
        Rigidbody otherRb = otherCollider.attachedRigidbody;
        if (otherRb == null) return;

        // Ignorer hvis det er samme rigidbody (sikkerhedscheck)
        if (otherRb == rb) return;

        // Tjek at objektet ligger på et layer som kan tage skade (filter)
        int otherLayer = otherRb.gameObject.layer;
        if ((damageableLayers.value & (1 << otherLayer)) == 0) return;

        // ---------- Beregn geometri / retninger ----------
        // Cacher transforms for læsbarhed
        // transforms
        Transform otherT = otherRb.transform;
        Transform ourT = rb.transform;

        // 2D/XZ-forwards: ignorer pitch (lodret rotation) ved at projicere fremad-vektorer ned på XZ-plan
        // --- XZ flattened forwards (ignore pitch) ---
        Vector3 ourForwardXZ = Vector3.ProjectOnPlane(ourT.forward, Vector3.up);
        Vector3 otherForwardXZ = Vector3.ProjectOnPlane(otherT.forward, Vector3.up);
        if (ourForwardXZ.sqrMagnitude < 1e-6f) ourForwardXZ = Vector3.forward;
        if (otherForwardXZ.sqrMagnitude < 1e-6f) otherForwardXZ = Vector3.forward;
        ourForwardXZ.Normalize();
        otherForwardXZ.Normalize();

        // Center-to-center retning (på XZ-planet)
        // center-to-center direction (XZ)
        Vector3 ourCom = rb.worldCenterOfMass;
        Vector3 otherCom = otherRb.worldCenterOfMass;
        Vector3 dirToOtherFromThis = otherCom - ourCom;
        dirToOtherFromThis.y = 0f;
        if (dirToOtherFromThis.sqrMagnitude < 1e-6f) dirToOtherFromThis = ourForwardXZ;
        dirToOtherFromThis.Normalize();

        // Brug dot-products til at se om centrene "kigger" mod hinanden
        // center-facing dots
        float dotThisCenter = Vector3.Dot(ourForwardXZ, dirToOtherFromThis); // >0 => we generally face the other
        float dotOtherCenter = Vector3.Dot(otherForwardXZ, -dirToOtherFromThis); // >0 => other faces us

        // Kontaktpunkt-baseret check: er kontaktpunktet foran vores front?
        // contact-based front check: is contact point in front hemisphere of each vehicle?
        Vector3 toContactThis = contactPoint.point - ourCom; toContactThis.y = 0f;
        Vector3 toContactOther = contactPoint.point - otherCom; toContactOther.y = 0f;
        float contactDotThis = (toContactThis.sqrMagnitude > 1e-6f) ? Vector3.Dot(ourForwardXZ, toContactThis.normalized) : 0f;
        float contactDotOther = (toContactOther.sqrMagnitude > 1e-6f) ? Vector3.Dot(otherForwardXZ, toContactOther.normalized) : 0f;

        // Se også hvor godt "front" peger mod normals (fallback indikator)
        // Also compute how well each front aligns with contact normal (useful for fallback)
        Vector3 normalXZ = Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up);
        if (normalXZ.sqrMagnitude < 1e-6f) normalXZ = Vector3.forward;
        normalXZ.Normalize();
        float alignThisToNormal = Vector3.Dot(ourForwardXZ, -normalXZ); // >0 => our front points against normal (i.e. toward other) - // >0 => vores front peger imod den anden
        float alignOtherToNormal = Vector3.Dot(otherForwardXZ, normalXZ); // >0 => other's front points against normal - // >0 => deres front peger imod os

        // Kontakt-tærskel (0..1)
        // Contact threshold to ensure contact point is actually "in front" (tune this in inspector)
        float contactThreshold = Mathf.Clamp(contactDotThreshold, 0f, 1f);

        // Beslut hvem der "attacker" (ser ud som angriber) baseret på center-dot og contact-dot
        // attacker initial flags require both center-facing and contact-in-front above threshold
        bool attackerThis = (dotThisCenter >= facingDotThreshold) && (contactDotThis >= contactThreshold);
        bool attackerOther = (dotOtherCenter >= facingDotThreshold) && (contactDotOther >= contactThreshold);

        // Hvis ingen af parterne opfylder de strenge front-checks, brug en konservativ fallback:
        // vælg den side hvis "front"-vektor stemmer bedst overens med kontakt-normalen,
        // men kun hvis denne alignment er meningsfuldt positiv (større end minAlignToConsider).
        if (!attackerThis && !attackerOther)
        {
            float minAlignToConsider = 0.25f;
            if (alignThisToNormal > alignOtherToNormal && alignThisToNormal > minAlignToConsider)
            {
                attackerThis = true;
                attackerOther = false;
            }
            else if (alignOtherToNormal > alignThisToNormal && alignOtherToNormal > minAlignToConsider)
            {
                attackerOther = true;
                attackerThis = false;
            }
            else
            {
                // Hvis vi ikke trygt kan beslutte en angriber, afbryd (ingen skade)
                return;
            }
        }
        // -------------------------
        // 2) Compute closing / impulse (as before)
        // -------------------------
        // vA og vB er punkt-hastigheder (inkluderer rotation), vRel = vA - vB er relativ hastighed
        Vector3 vA = rb.GetPointVelocity(contactPoint.point);
        Vector3 vB = otherRb.GetPointVelocity(contactPoint.point);
        Vector3 vRel = vA - vB;
        // closing måler hvor hurtigt de nærmer sig langs kontakt-normalen (m/s)
        float closing = -Vector3.Dot(vRel, contactPoint.normal); // >0 => we approach along normal

        float usedClosing = 0f;

        // Hvis closing over tærskel, brug den. Ellers brug impulse-fallback (deltaV estimeret fra impulse / masse)
        if (closing >= minSpeedBeforeDamable)
        {
            usedClosing = closing;
        }
        else
        {
            Vector3 impulse = collision.impulse; // fysisk impuls vektor fra Unity collision
            float deltaV = impulse.magnitude / Mathf.Max(1e-6f, rb.mass); // estimer ændring i hastighed (m/s)
            if (deltaV >= minSpeedBeforeDamable)
            {
                usedClosing = deltaV;
            }
        }

        // Hvis stadig for lille, ignorer
        if (usedClosing <= 0f) return;

        // -------------------------
        // 3) Hvem skal anvende skaden?
        // -------------------------
        // Beslut om "denne" side (this) skal påføre skade. Typisk: angriberen gør skade på modparten.
        bool shouldApplyThis = false;

        if (attackerThis && !attackerOther)
        {
            shouldApplyThis = true;
        }
        else if (attackerThis && attackerOther)
        {
            shouldApplyThis = true;
        }
        else if (!attackerThis && attackerOther)
        {
            shouldApplyThis = false;
        }

        if (!shouldApplyThis) return;

        // -------------------------
        // 4) Beregn og påfør skade
        // -------------------------
        // Fra usedClosing (m/s) lav en skadeværdi, evt. multipliceret med speed -> km/h skaleringsfaktor
        float baseDamage = usedClosing;
        float speedKmh = baseDamage * 3.6f;
        float multiplier = 1f + extraDamagePerKmh * speedKmh;
        if (multiplier < 0f) multiplier = 0f;

        // Hvis den anden part også er et DamageCollision-objekt (samme system), kald ReceiveHit direkte
        float finalDamage = baseDamage * multiplier;
        finalDamage = Mathf.Min(maxDamage, finalDamage);
        currentDamage = finalDamage;

        // Ellers prøv at finde en Health-komponent og påfør skade der
        if (otherRb.TryGetComponent<DamageCollision>(out var otherDC))
        {
            otherDC.ReceiveHit(finalDamage);
            return;
        }
        if (!otherRb.TryGetComponent<Health>(out var hp))
        {
            hp = otherRb.GetComponentInParent<Health>();
            if (hp == null) hp = otherRb.GetComponentInChildren<Health>();
        }
        if (hp != null)
        {
            hp.ApplyDamage(finalDamage);
            return;
        }

    }
    // Metode som andre DamageCollision objekter kan kalde for at give skade direkte til dette objekt
    public void ReceiveHit(float damage)
    {
        // Forsøg først at hente Health direkte på dette GameObject (mest sandsynligt).
        // Hvis ikke fundet, fallback til parent og så children.
        if (!TryGetComponent<Health>(out var hp))
        {
            hp = GetComponentInParent<Health>();
            if (hp == null) hp = GetComponentInChildren<Health>();
        }

        if (hp != null)
        {
            hp.ApplyDamage(damage);
        }
    }

    // Enkel helper-class der videresender OnCollisionEnter fra child-collider til owner (dette script)
    [DisallowMultipleComponent]
    public class CollisionForwarder : MonoBehaviour
    {
        [System.NonSerialized] public DamageCollision owner;
        void OnCollisionEnter(Collision collision)
        {
            if (owner == null) return;
            owner.HandleCollisionFromChild(collision);
        }
    }
}
