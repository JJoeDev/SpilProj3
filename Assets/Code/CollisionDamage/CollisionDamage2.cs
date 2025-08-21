using UnityEngine;

// Denne komponent sidder på et objekts hierarki (fx Spiller eller Fjende) :) 
// og beregner skade ved en fysisk kollision. Kun den part, der er hurtigst
// (ud over en "buffer"), giver skade til modparten.
public class DamageCollision2 : MonoBehaviour
{
    [Header("Lag & Filter")]
    [SerializeField] private LayerMask m_damageableLayers = ~0;

    [Header("Skadeberegning")]
    // En tærskel/buffer som skal overvindes før der gives skade.
    // Formlen er: (min_fart - din_fart - buffer). Hvis resultat <= 0 → ingen skade.
    [SerializeField] private float m_buffer = 2f;
    
    [SerializeField] private float m_damageMultiplier = 1f;

   
    [SerializeField] private bool m_roundDamageToInt = true;

    [Header("Afhængigheder")]
    [SerializeField] private Rigidbody m_rb;

    [Header("Debug")]
    // Slå log på/af. Når den er true, skriver vi tal i Console ved kollision.
    [SerializeField] private bool m_log = false;
    private Vector3 m_lastVelocity; // egen pre-collision velocity
    private float m_lastSpeed; // egen pre-collision speed (|v|)
    public float PreSpeed => m_lastSpeed;

    void Awake()
    {
        if (m_rb == null)
        {
            m_rb = GetComponent<Rigidbody>();
            if (m_rb == null) m_rb = GetComponentInParent<Rigidbody>();
            if (m_rb == null) m_rb = GetComponentInChildren<Rigidbody>();
        }
    }
    void FixedUpdate()
    {
        if (m_rb != null)
        {
            m_lastVelocity = m_rb.velocity; // vektor-retning + størrelse
            m_lastSpeed = m_lastVelocity.magnitude; // kun størrelsen (|v|)
        }
        else
        {
            m_lastVelocity = Vector3.zero;
            m_lastSpeed = 0f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Sikkerhed: hvis Unity af en eller anden grund sender en null, gør ingenting.
        if (collision == null) return;

        GameObject hitGO = collision.collider != null ? collision.collider.gameObject : collision.gameObject;
        GameObject target = ResolveDamageTarget(hitGO);
        if (target == null) return; // Intet at skade

        if (((1 << target.layer) & m_damageableLayers) == 0) // Må vi skade target? Ja hvis target.layer er inkluderet i m_damageableLayers; ellers nej.
        {
            if (m_log) Debug.Log($"[{name}] Skip: target layer '{LayerMask.LayerToName(target.layer)}' ikke i maske."); 
            return;
        }

        // Hent begge parters pre-hastighed:
        // selfPreSpeed kommer fra vores egen cache (sat i FixedUpdate).
        float selfPreSpeed = m_lastSpeed;

        // otherPreSpeed: vi prøver først at finde modpartens DamageCollision2,
        // så vi kan læse dens PreSpeed direkte (det er den bedste kilde).
        float otherPreSpeed;
        DamageCollision2 otherDC = hitGO.GetComponentInParent<DamageCollision2>();
        if (otherDC != null)
        {
            otherPreSpeed = otherDC.PreSpeed;
        }
        else if (collision.rigidbody != null)
        {
            otherPreSpeed = collision.rigidbody.velocity.magnitude;
        }
        else
        {
            // Hvis der slet ingen RB er, antager vi 0.
            otherPreSpeed = 0f;
        }
    
        // Kun hvis råSkade > 0 giver vi skade (dvs. vi var reelt hurtigere).
        float raw = (selfPreSpeed - otherPreSpeed) - m_buffer;

        
        if (m_log) // fjen denne linje eller set m_log = false. får at ungå denne debug. 
        {
            string tName = target != null ? target.name : "null";
            string tLayer = target != null ? LayerMask.LayerToName(target.layer) : "?"; // du kan fjernde dette vis du ønsker det. er kun til debugs.
            Debug.Log($"[{name}] selfPre={selfPreSpeed:F2}, otherPre={otherPreSpeed:F2}, buffer={m_buffer:F2}, raw={raw:F2} → target={tName}({tLayer})");
        }
        if (raw <= 0f) return;   

        float dmg = raw * m_damageMultiplier;
        if (m_roundDamageToInt) dmg = Mathf.Round(dmg);

        ApplyDamage(target, dmg);
    }
    GameObject ResolveDamageTarget(GameObject hit)
    {
        // Find IDamageable opad i hierarkiet
        var parents = hit.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var mb in parents)
            if (mb is IDamageable) return mb.gameObject;

        // Fallback: brug root (typisk hvor health sidder)
        return hit.transform.root != null ? hit.transform.root.gameObject : hit;
    }
    void ApplyDamage(GameObject target, float amount)
    {
        if (target == null || amount <= 0f) return;

        // Interface (foretrukket og mest robuste løsning)
        foreach (var mb in target.GetComponentsInParent<MonoBehaviour>(true))
        {
            if (mb is IDamageable idmg)
            {
                // Kalder jeres egen health-implementations TakeDamage(amount, source)
                idmg.TakeDamage(amount, gameObject);
                return;
            }
        }

        // Fjern denne linje, hvis I ikke vil have mere konsol-output.
        target.SendMessage("TakeDamage", amount, SendMessageOptions.DontRequireReceiver);
        target.SendMessage("ApplyDamage", amount, SendMessageOptions.DontRequireReceiver);
    }
}
