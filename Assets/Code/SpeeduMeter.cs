using UnityEngine;
using UnityEngine.UI;

// Hvis du bruger TextMeshPro, så fjern kommentaren nedenfor og brug TMP-feltet.
// using TMPro;

public class SpeeduMeter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Bilobjekt med PrometeoCarController")]
    public PrometeoCarController car;

    [Tooltip("Pivot til nålen (roteres)")]
    public RectTransform needle;

    [Tooltip("UI tekst til digital hastighed (km/t)")]
    public Text speedText;
    // public TextMeshProUGUI speedTMP; // Brug denne hvis du foretrækker TMP

    [Header("Skala & visning")]
    [Tooltip("Min/max vinkel på nålen (grader). F.eks. -130 til +130")]
    public float needleMinAngle = 99.555f;  // venstre (max hastighed)
    public float needleMaxAngle = -100.485f;  // højre (0 km/t)

    [Tooltip("Visuel max-værdi på skiven (km/t). Typisk = bilens maxSpeed, men kan være højere for headroom.")]
    public float gaugeMaxKmh = 200f;

    [Tooltip("Dæmpning/udjævning for nål og tal (0 = ingen smooth, højere = mere smooth)")]
    [Range(0f, 20f)]
    public float smooth = 8f;

    [Header("Enheder")]
    public bool showMphToo = false;          // kun kosmetisk til teksten
    [Tooltip("Konverteringsfaktor km/t -> mph")]
    public float kmhToMph = 0.621371f;

    float _displayKmh; // glattet værdi til UI/viser

    void Reset()
    {
        // Auto-find bil hvis muligt
        if (!car) car = FindObjectOfType<PrometeoCarController>();
    }

    void Update()
    {
        if (!car) return;

        // car.carSpeed er allerede i km/t i PrometeoCarController
        float kmh = Mathf.Abs(car.carSpeed);
        /*
        var rb = car.GetComponent<Rigidbody>();
        float kmh = rb ? rb.velocity.magnitude * 3.6f : Mathf.Abs(car.carSpeed);
        */
        // Smooth visning
        if (smooth > 0f)
            _displayKmh = Mathf.Lerp(_displayKmh, kmh, Time.deltaTime * smooth);
        else
            _displayKmh = kmh;

        UpdateNeedle(_displayKmh);
        UpdateText(_displayKmh);
    }

    void UpdateNeedle(float kmh)
    {
        if (!needle) return;

        // Clamp til skivens max
        float t = Mathf.InverseLerp(0f, Mathf.Max(1f, gaugeMaxKmh), Mathf.Min(kmh, gaugeMaxKmh));
        float angle = Mathf.Lerp(needleMaxAngle, needleMinAngle, t);
        needle.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    void UpdateText(float kmh)
    {
        // Brug enten UnityEngine.UI.Text eller TMP – ikke begge
        if (speedText)
        {
            if (showMphToo)
            {
                float mph = kmh * kmhToMph;
                speedText.text = $"{Mathf.RoundToInt(kmh)} km/h\n{Mathf.RoundToInt(mph)} mph";
            }
            else
            {
                speedText.text = Mathf.RoundToInt(kmh).ToString();
            }
        }

        // Hvis du bruger TMP:
        // if (speedTMP)
        // {
        //     if (showMphToo)
        //     {
        //         float mph = kmh * kmhToMph;
        //         speedTMP.text = $"{Mathf.RoundToInt(kmh)} km/h\n{Mathf.RoundToInt(mph)} mph";
        //     }
        //     else
        //     {
        //         speedTMP.text = Mathf.RoundToInt(kmh).ToString();
        //     }
        // }
    }
}
