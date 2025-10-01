using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeeduMeter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Bilobjekt med PrometeoCarController")]
    [SerializeField] private CarController m_car;

    [Tooltip("Pivot til nålen (roteres)")]
    [SerializeField] private RectTransform m_needle;

    [Tooltip("UI tekst til digital hastighed (km/t)")]
    [SerializeField] private TextMeshProUGUI m_speedTMP; // Brug denne hvis du foretrækker TMP

    [Header("Skala & visning")]
    [Tooltip("Min/max vinkel på nålen (grader). F.eks. -130 til +130")]
    [SerializeField] private float m_needleMinAngle = 99.555f;  // venstre (max hastighed)
    [SerializeField] private float m_needleMaxAngle = -100.485f;  // højre (0 km/t)

    [Tooltip("Visuel max-værdi på skiven (km/t). Typisk = bilens maxSpeed, men kan være højere for headroom.")]
    [SerializeField] private float m_gaugeMaxKmh = 200f;

    [Tooltip("Dæmpning/udjævning for nål og tal (0 = ingen smooth, højere = mere smooth)")]
    [Range(0f, 20f)]
    [SerializeField] private float m_smooth = 8f;

    float m_displayKmh; // glattet værdi til UI/viser

    void Reset()
    {
        // Auto-find bil hvis muligt
        if (!m_car) m_car = FindObjectOfType<CarController>();
    }

    void Update()
    {
        if (!m_car) return;

        // car.carSpeed er allerede i km/t i PrometeoCarController
        float kmh = Mathf.Abs(m_car.carSpeed);

        // Smooth visning
        if (m_smooth > 0f)
            m_displayKmh = Mathf.Lerp(m_displayKmh, kmh, Time.deltaTime * m_smooth);
        else
            m_displayKmh = kmh;

        UpdateNeedle(m_displayKmh);
        UpdateText(m_displayKmh);
    }

    void UpdateNeedle(float kmh)
    {
        if (!m_needle) return;

        // Clamp til skivens max
        float t = Mathf.InverseLerp(0f, Mathf.Max(1f, m_gaugeMaxKmh), Mathf.Min(kmh, m_gaugeMaxKmh));
        float angle = Mathf.Lerp(m_needleMaxAngle, m_needleMinAngle, t);
        m_needle.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    void UpdateText(float kmh)
    {
        if (m_speedTMP)
        {
            m_speedTMP.text = Mathf.RoundToInt(kmh).ToString();
        }
    }
}
