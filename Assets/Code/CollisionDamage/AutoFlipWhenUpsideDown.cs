using UnityEngine;

/// <summary>
/// Forhindrer kørsel når køretøjet ligger på hovedet og vender det automatisk om efter en periode.
/// </summary>
public class AutoFlipWhenUpsideDown : MonoBehaviour
{
    [Header("Opsporing af 'på hovedet'")]
    [Tooltip("Vinkel fra lodret (0 = helt oprejst, 180 = helt på hovedet), hvor vi anser bilen for at være 'på hovedet'.")]
    [Range(90f, 179f)]
    public float vinkelTærskel = 120f;

    [Tooltip("Hvor længe den skal være på hovedet, før vi automatisk vender den (sekunder).")]
    public float tidFørAutoFlip = 2.0f;

    [Header("Flip-indstillinger")]
    [Tooltip("Hvor meget vi løfter bilen op før vi reorienterer (for at undgå at sidde fast i jorden).")]
    public float løftHøjde = 1.0f;

    [Tooltip("Cooldown efter flip (sekunder) før kørsel igen tillades, så fysikken kan falde til ro.")]
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

    private bool _erLåst;            // kørsel låst pga. på hovedet
    private float _påHovedetTimer;   // hvor længe vi har været på hovedet
    private float _genAktiverTid;    // tidspunkt hvor vi må genaktivere kørsel

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
            LåsKørsel();
            UdførFlip();
            return;
        }

        // Tjek om vi er på hovedet
        bool påHovedet = ErPåHovedet();

        if (påHovedet)
        {
            _påHovedetTimer += Time.deltaTime;

            // Lås kørsel med det samme
            if (!_erLåst)
                LåsKørsel();

            // Flip når timeren er gået
            if (_påHovedetTimer >= tidFørAutoFlip)
            {
                UdførFlip();
            }
        }
        else
        {
            // Ikke på hovedet -> nulstil timer
            _påHovedetTimer = 0f;

            // Hvis vi tidligere var låst, så gen-aktiver når cooldown er ovre
            if (_erLåst && Time.time >= _genAktiverTid)
            {
                FrigivKørsel();
            }
        }
    }
    private bool ErPåHovedet()
    {
        // 0° = oprejst, 180° = på hovedet
        float vinkel = Vector3.Angle(transform.up, Vector3.up);
        return vinkel >= vinkelTærskel;
    }

    private void LåsKørsel()
    {
        if (_erLåst) return;
        _erLåst = true;

        // Stop hastighed i PlayerMovement og deaktivér styring
        if (_pm != null)
        {
            _pm.currentSpeed = 0f;
            _pm.enabled = false;
        }
    }

    private void FrigivKørsel()
    {
        _erLåst = false;

        if (_pm != null)
            _pm.enabled = true;
    }

    private void UdførFlip()
    {
        // Løft lidt op
        transform.position += Vector3.up * løftHøjde;

        // Bevar yaw, nulstil roll/pitch
        float yaw = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Nulstil rigidbody-hastigheder for stabilitet
        if (_rb != null && nulstilRigidbodyHastighed)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        // Start cooldown før vi må køre igen
        _genAktiverTid = Time.time + efterFlipCooldown;
        _påHovedetTimer = 0f;
    }

    // Hjælpe-gizmo så du kan se world-up vs din up i Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * 2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.up * 2f);
    }
}

