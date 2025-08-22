using UnityEngine;
using System.Collections.Generic;

/*public class CollisionDamage : MonoBehaviour
{
    [Header("Skadesindstillinger")]
    [Tooltip("Skade pr. 1 enhed hastighed (enheder/sek).")]
    [SerializeField] private float damagePerSpeed = 2f; // eksemepl ( 2 * bilenshastighed = x skade ) 

    [Tooltip("Mindste hastighed der kr�ves for at give skade.")]
    [SerializeField] private float minSpeedToDealDamage = 1f; // bestemer hvor hurtigt bilen skal k�re for at kunne skade.... 

    [Tooltip("Valgfrit loft for skaden. S�t <= 0 for at deaktivere.")]
    [SerializeField] private float maxDamage = 0f; // kun vigtigt hvis i �nsker en max skade.... 

    [Header("Kollisionsfiltrering")]
    [Tooltip("Kun objekter p� disse lag modtager skade.")]
    [SerializeField] private LayerMask damageableLayers = ~0; // bestemer hvilken layer der kan tags skade... 

    [Header("Tr�f-kontrol")]
    [Tooltip("Sekunder der skal g�, f�r samme m�l kan tage skade igen.")]
    [SerializeField] private float perTargetCooldown = 0.25f;

    [Tooltip("Hvis sand, giv ogs� skade ved trigger-ber�ringer (for trigger-colliders).")]
    [SerializeField] private bool damageOnTrigger = true;

    // Reference til din bev�gelsesscript (skal have public float speed)
    private PlayerMovementTest playerMovement;

    // Holder styr p� sidste tidspunkt et m�l fik skade (via instanceID) for at undg� spam
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
        
        // Afg�r aktuel hastighed udelukkende fra PlayerMovement
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