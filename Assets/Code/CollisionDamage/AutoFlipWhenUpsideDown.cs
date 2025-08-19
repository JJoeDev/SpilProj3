using UnityEngine;

/// <summary>
/// Forhindrer k�rsel n�r k�ret�jet ligger p� hovedet og vender det automatisk om efter en periode.
/// </summary>
public class AutoFlipWhenUpsideDown : MonoBehaviour
{
    [Header("Opsporing af 'p� hovedet'")]
    [Tooltip("Vinkel fra lodret (0 = helt oprejst, 180 = helt p� hovedet), hvor vi anser bilen for at v�re 'p� hovedet'.")]
    [Range(90f, 179f)]
    public float vinkelT�rskel = 120f;

    [Tooltip("Hvor l�nge den skal v�re p� hovedet, f�r vi automatisk vender den (sekunder).")]
    public float tidF�rAutoFlip = 2.0f;

    [Header("Flip-indstillinger")]
    [Tooltip("Hvor meget vi l�fter bilen op f�r vi reorienterer (for at undg� at sidde fast i jorden).")]
    public float l�ftH�jde = 1.0f;

    [Tooltip("Cooldown efter flip (sekunder) f�r k�rsel igen tillades, s� fysikken kan falde til ro.")]
    public float efterFlipCooldown = 0.5f;

    [Tooltip("Nulstil Rigidbody-hastigheder ved flip (anbefalet hvis du bruger Rigidbody).")]
    public bool nulstilRigidbodyHastighed = true;

    [Header("Manuelt flip (valgfrit)")]
    [Tooltip("Tillad manuelt flip med en tast (nyttigt til test).")]
    public bool tilladManueltFlip = true;

    [Tooltip("Tast til manuelt flip.")]
    public KeyCode manuelFlipKey = KeyCode.R;

    [Header("Debug")]
    public bool logTilKonsol = false;

    private PlayerMovementTest _pm;
    private Rigidbody _rb;

    private bool _erL�st;            // k�rsel l�st pga. p� hovedet
    private float _p�HovedetTimer;   // hvor l�nge vi har v�ret p� hovedet
    private float _genAktiverTid;    // tidspunkt hvor vi m� genaktivere k�rsel

    void Awake()
    {
        _pm = GetComponent<PlayerMovementTest>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Manuel flip til test
        if (tilladManueltFlip && Input.GetKeyDown(manuelFlipKey))
        {
            L�sK�rsel();
            Udf�rFlip();
            return;
        }

        // Tjek om vi er p� hovedet
        bool p�Hovedet = ErP�Hovedet();

        if (p�Hovedet)
        {
            _p�HovedetTimer += Time.deltaTime;

            // L�s k�rsel med det samme
            if (!_erL�st)
                L�sK�rsel();

            // Flip n�r timeren er g�et
            if (_p�HovedetTimer >= tidF�rAutoFlip)
            {
                Udf�rFlip();
            }
        }
        else
        {
            // Ikke p� hovedet -> nulstil timer
            _p�HovedetTimer = 0f;

            // Hvis vi tidligere var l�st, s� gen-aktiver n�r cooldown er ovre
            if (_erL�st && Time.time >= _genAktiverTid)
            {
                FrigivK�rsel();
            }
        }
    }
    private bool ErP�Hovedet()
    {
        // 0� = oprejst, 180� = p� hovedet
        float vinkel = Vector3.Angle(transform.up, Vector3.up);
        return vinkel >= vinkelT�rskel;
    }

    private void L�sK�rsel()
    {
        if (_erL�st) return;
        _erL�st = true;

        // Stop hastighed i PlayerMovement og deaktiv�r styring
        if (_pm != null)
        {
            _pm.currentSpeed = 0f;
            _pm.enabled = false;
        }
    }

    private void FrigivK�rsel()
    {
        _erL�st = false;

        if (_pm != null)
            _pm.enabled = true;
    }

    private void Udf�rFlip()
    {
        // L�ft lidt op
        transform.position += Vector3.up * l�ftH�jde;

        // Bevar yaw, nulstil roll/pitch
        float yaw = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Nulstil rigidbody-hastigheder for stabilitet
        if (_rb != null && nulstilRigidbodyHastighed)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // Start cooldown f�r vi m� k�re igen
        _genAktiverTid = Time.time + efterFlipCooldown;
        _p�HovedetTimer = 0f;
    }

    // Hj�lpe-gizmo s� du kan se world-up vs din up i Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.up * 2f);
    }
}

