using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DamageCollision : MonoBehaviour
{
    // virker både til fjende og player. 
    public Rigidbody rb; // filled in Awake if left empty in inspector

    public float minSpeedBeforeDamable = 5f; // m/s
    [Range(0f, 1f)]
    public float contactDotThreshold = 0.5f;

    [Header("Damage Settings")]
    public LayerMask damageableLayers = ~0;

    [Header("Optional: assign your car's body/mesh colliders here (auto-filled if empty)")]
    public Collider[] myColliders;

    [Header("Facing rules")]
    [Tooltip("Dot threshold to consider a vehicle 'facing' the other. Range -1..1.")]
    [Range(-1f, 1f)]
    public float facingDotThreshold = 0.1f;

    [Header("Speed multiplier (multiplicative mode)")]
    [Tooltip("Fraction per km/h. Example: 0.01 = +1% damage per km/h.")]
    public float extraDamagePerKmh = 0.01f;

    void Awake()
    {
        if (rb == null)
            gameObject.TryGetComponent<Rigidbody>(out rb);

        if (myColliders == null || myColliders.Length == 0)
            myColliders = GetComponentsInChildren<Collider>(includeInactive: true);

        foreach (var c in myColliders)
        {
            if (c == null) continue;
            if (c.gameObject.GetComponentInParent<DamageCollision>() != this) continue;

            if (!c.gameObject.TryGetComponent<CollisionForwarder>(out CollisionForwarder forwarder))
            {
                forwarder = c.gameObject.AddComponent<CollisionForwarder>();
            }
            forwarder.owner = this;
        }
    }
    // root fallback
    void OnCollisionEnter(Collision collision)
    {
        HandleCollisionFromChild(collision);
    }
    private void HandleCollisionFromChild(Collision collision)
    {
        if (rb == null) return;
        if (collision == null || collision.contactCount == 0) return;
        ContactPoint contactPoint = collision.GetContact(0); 

        Collider otherCollider = (contactPoint.otherCollider != null) ? contactPoint.otherCollider : collision.collider;
        if (otherCollider == null) return;
        if (myColliders != null && System.Array.IndexOf(myColliders, otherCollider) >= 0) return;

        Rigidbody otherRb = otherCollider.attachedRigidbody;
        if (otherRb == null || otherRb == rb) return;

        int otherLayer = otherRb.gameObject.layer;
        if ((damageableLayers.value & (1 << otherLayer)) == 0) return;

        Transform otherT = otherRb.transform;
        Transform ourT = rb.transform;

        Vector3 ourForwardXZ = Vector3.ProjectOnPlane(ourT.forward, Vector3.up);
        Vector3 otherForwardXZ = Vector3.ProjectOnPlane(otherT.forward, Vector3.up);
        if (ourForwardXZ.sqrMagnitude < 1e-6f) ourForwardXZ = Vector3.forward;
        if (otherForwardXZ.sqrMagnitude < 1e-6f) otherForwardXZ = Vector3.forward;
        ourForwardXZ.Normalize();
        otherForwardXZ.Normalize();

        Vector3 ourCom = rb.worldCenterOfMass;
        Vector3 otherCom = otherRb.worldCenterOfMass;
        Vector3 dirToOtherFromThis = otherCom - ourCom;
        dirToOtherFromThis.y = 0f;
        if (dirToOtherFromThis.sqrMagnitude < 1e-6f) dirToOtherFromThis = ourForwardXZ;
        dirToOtherFromThis.Normalize();

        float dotThisCenter = Vector3.Dot(ourForwardXZ, dirToOtherFromThis);
        float dotOtherCenter = Vector3.Dot(otherForwardXZ, -dirToOtherFromThis);

        Vector3 toContactThis = contactPoint.point - ourCom; toContactThis.y = 0f;
        Vector3 toContactOther = contactPoint.point - otherCom; toContactOther.y = 0f;
        float contactDotThis = (toContactThis.sqrMagnitude > 1e-6f) ? Vector3.Dot(ourForwardXZ, toContactThis.normalized) : 0f;
        float contactDotOther = (toContactOther.sqrMagnitude > 1e-6f) ? Vector3.Dot(otherForwardXZ, toContactOther.normalized) : 0f;

        Vector3 normalXZ = Vector3.ProjectOnPlane(contactPoint.normal, Vector3.up);
        if (normalXZ.sqrMagnitude < 1e-6f) normalXZ = Vector3.forward;
        normalXZ.Normalize();
        float alignThisToNormal = Vector3.Dot(ourForwardXZ, -normalXZ);
        float alignOtherToNormal = Vector3.Dot(otherForwardXZ, normalXZ);

        bool attackerThis = (dotThisCenter >= facingDotThreshold) && (contactDotThis >= contactDotThreshold);
        bool attackerOther = (dotOtherCenter >= facingDotThreshold) && (contactDotOther >= contactDotThreshold);

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
                return;
            }
        }
        Vector3 vA = rb.GetPointVelocity(contactPoint.point);
        Vector3 vB = otherRb.GetPointVelocity(contactPoint.point);
        Vector3 vRel = vA - vB;
        float closing = -Vector3.Dot(vRel, contactPoint.normal);

        float usedClosing = 0f;
        if (closing >= minSpeedBeforeDamable)
        {
            usedClosing = closing;
        }
        else
        {
            Vector3 impulse = collision.impulse;
            float deltaV = impulse.magnitude / Mathf.Max(1e-6f, rb.mass);
            if (deltaV >= minSpeedBeforeDamable)
                usedClosing = deltaV;
        }

        if (usedClosing <= 0f) return;

        bool shouldApplyThis = false;
        if (attackerThis && !attackerOther) shouldApplyThis = true;
        else if (attackerThis && attackerOther) shouldApplyThis = true;
        else if (!attackerThis && attackerOther) shouldApplyThis = false;

        if (!shouldApplyThis) return;

        float baseDamage = usedClosing;
        float speedKmh = baseDamage * 3.6f;
        float multiplier = 1f + extraDamagePerKmh * speedKmh;

        float finalDamage = baseDamage * multiplier;

        if (otherRb.TryGetComponent<DamageCollision>(out var otherDC))
        {
            otherDC.ReceiveHit(finalDamage);
            return;
        }
        if (!otherRb.TryGetComponent<HealthManager>(out var hp))
        {
            hp = otherRb.GetComponentInParent<HealthManager>();
            if (hp == null) hp = otherRb.GetComponentInChildren<HealthManager>();
        }
        if (hp != null)
        {
            hp.TakeDamage(finalDamage);
            return;
        }
    }

    public void ReceiveHit(float damage)
    {
        if (!TryGetComponent<HealthManager>(out var hp))
        {
            hp = GetComponentInParent<HealthManager>();
            if (hp == null) hp = GetComponentInChildren<HealthManager>();
        }

        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
    }
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
