using UnityEngine;
using System.Collections.Generic;

/*public class CollisionDamage : MonoBehaviour
{
    [Header("Skadesindstillinger")]
    [Tooltip("Skade pr. 1 enhed hastighed (enheder/sek).")]
    [SerializeField] private float damagePerSpeed = 2f; // eksemepl ( 2 * bilenshastighed = x skade ) 

    [Tooltip("Mindste hastighed der kræves for at give skade.")]
    [SerializeField] private float minSpeedToDealDamage = 1f; // bestemer hvor hurtigt bilen skal køre for at kunne skade.... 

    [Tooltip("Valgfrit loft for skaden. Sæt <= 0 for at deaktivere.")]
    [SerializeField] private float maxDamage = 0f; // kun vigtigt hvis i ønsker en max skade.... 

    [Header("Kollisionsfiltrering")]
    [Tooltip("Kun objekter på disse lag modtager skade.")]
    [SerializeField] private LayerMask damageableLayers = ~0; // bestemer hvilken layer der kan tags skade... 

    [Header("Træf-kontrol")]
    [Tooltip("Sekunder der skal gå, før samme mål kan tage skade igen.")]
    [SerializeField] private float perTargetCooldown = 0.25f;

    [Tooltip("Hvis sand, giv også skade ved trigger-berøringer (for trigger-colliders).")]
    [SerializeField] private bool damageOnTrigger = true;

    // Reference til din bevægelsesscript (skal have public float speed)
    private PlayerMovementTest playerMovement;

    // Holder styr på sidste tidspunkt et mål fik skade (via instanceID) for at undgå spam
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();

    void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovementTest>();  
    } 
    void OnCollisionEnter(Collision collision)
    {
        TryApplyDamage(collision.collider); 
    }
    void OnTriggerEnter(Collider other)
    {
        if (damageOnTrigger)
            TryApplyDamage(other);
    }
    private void TryApplyDamage(Collider hitCollider)
    {
        
        if (((1 << hitCollider.gameObject.layer) & damageableLayers) == 0)
            return;
        
        // Afgør aktuel hastighed udelukkende fra PlayerMovement
        float currentSpeed = (playerMovement != null) ? playerMovement.speed : 0f;
        if (currentSpeed < minSpeedToDealDamage)
            return;
        
        // Beregn skade ud fra hastighed
        float damage = currentSpeed * damagePerSpeed;
        if (maxDamage > 0f) damage = Mathf.Min(damage, maxDamage);
        
        
        GameObject targetRoot = hitCollider.attachedRigidbody
            ? hitCollider.attachedRigidbody.gameObject
            : hitCollider.gameObject;

        int id = targetRoot.GetInstanceID();
        float now = Time.time;
        if (lastHitTime.TryGetValue(id, out float lastTime))
        {
            if (now - lastTime < perTargetCooldown) return;
        }
        lastHitTime[id] = now;
        
        targetRoot.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }
}
*/