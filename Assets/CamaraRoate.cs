using UnityEngine;
using Cinemachine;

/// Clamp Y, add a slight tilt, and (most important) rotate the camera's
/// orbit vector toward the target's Up in the Body stage when banked.
/// This makes Y feel "redefined" relative to the car on ramps even when
/// FreeLook Binding Mode = World Space.
[ExecuteAlways, RequireComponent(typeof(CinemachineFreeLook))]
public class FL_YClampTiltAlign : CinemachineExtension
{
    [Header("References")]
    public Transform target;   // leave empty to use vcam.Follow

    // ---------- Y Clamp ----------
    [Header("Clamp Y Axis (between bottom and top rig)")]
    [Range(0f, 1f)] public float minY = 0.35f;
    [Range(0f, 1f)] public float maxY = 0.95f;
    [Range(0.1f, 20f)] public float yReturnSpeed = 8f;

    // ---------- Pitch Bias ----------
    [Header("Small 'film-from-above' pitch")]
    [Range(0f, 25f)] public float pitchBiasDeg = 8f;   // 0 disables
    [Range(0.1f, 30f)] public float pitchSmooth = 12f;

    // ---------- Position orbit rotate (the important part) ----------
    [Header("Rotate orbit toward target Up when banked")]
    public bool rotateOrbitOnBank = true;
    [Tooltip("Start rotating the orbit when |bank| exceeds this (deg).")]
    [Range(0f, 90f)] public float alignStartDeg = 12f;
    [Tooltip("How much orbit rotation at 90° bank (0..1). 1 = fully use target.up")]
    [Range(0f, 1f)] public float alignStrengthAt90 = 1.0f;
    [Range(0.1f, 30f)] public float alignSmooth = 10f;

    CinemachineFreeLook _fl;
    float _pitchSmoothed;
    float _alignSmoothed; // 0..1

    protected override void Awake()
    {
        base.Awake();
        _fl = GetComponent<CinemachineFreeLook>();
        if (!target) target = _fl ? _fl.Follow : null;
        NormalizeY();
    }

    void OnValidate() => NormalizeY();
    void NormalizeY()
    {
        minY = Mathf.Clamp01(minY);
        maxY = Mathf.Clamp01(maxY);
        if (maxY < minY) { var t = minY; minY = maxY; maxY = t; }
    }

    void LateUpdate()
    {
        if (_fl == null) return;

        // Smooth Y clamp (no snap)
        float y = _fl.m_YAxis.Value;
        float targetY = Mathf.Clamp(y, minY, maxY);
        if (!Mathf.Approximately(y, targetY))
        {
            float k = 1f - Mathf.Exp(-yReturnSpeed * Time.deltaTime);
            _fl.m_YAxis.Value = Mathf.Lerp(y, targetY, k);
        }
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        Transform t = target ? target : vcam.Follow;
        if (!t) return;
        float dt = Mathf.Max(deltaTime, 1e-4f);

        // ---- BODY: rotate the orbit vector around the target so "up" tends toward target.up
        if (stage == CinemachineCore.Stage.Body && rotateOrbitOnBank)
        {
            // how banked are we?
            float bank = Mathf.Abs(Vector3.SignedAngle(Vector3.up, t.up, t.forward));
            float targetAlign =
                (bank <= alignStartDeg) ? 0f :
                Mathf.Lerp(0f, alignStrengthAt90, Mathf.InverseLerp(alignStartDeg, 90f, bank));
            float kAlign = Application.isPlaying ? 1f - Mathf.Exp(-alignSmooth * dt) : 1f;
            _alignSmoothed = Mathf.Lerp(_alignSmoothed, targetAlign, kAlign);

            if (_alignSmoothed > 0.0001f)
            {
                // rotate current orbit vector (camera pos relative to Follow) toward target.up
                Vector3 followPos = t.position;
                Vector3 camPos = state.RawPosition + state.PositionCorrection;
                Vector3 toCam = camPos - followPos;
                if (toCam.sqrMagnitude > 1e-6f)
                {
                    Quaternion toUp = Quaternion.FromToRotation(Vector3.up, t.up);
                    Quaternion q = Quaternion.Slerp(Quaternion.identity, toUp, _alignSmoothed);
                    Vector3 rotated = q * toCam;
                    state.PositionCorrection += (rotated - toCam);
                }
            }
        }

        // ---- AIM: small pitch bias so we look a bit down at the car
        if (stage == CinemachineCore.Stage.Aim && pitchBiasDeg > 0.01f)
        {
            float kPitch = Application.isPlaying ? 1f - Mathf.Exp(-pitchSmooth * dt) : 1f;
            _pitchSmoothed = Mathf.Lerp(_pitchSmoothed, pitchBiasDeg, kPitch);

            Quaternion current = state.RawOrientation * state.OrientationCorrection;
            Vector3 right = current * Vector3.right;
            Quaternion qPitch = Quaternion.AngleAxis(_pitchSmoothed, right);

            state.OrientationCorrection = state.OrientationCorrection * qPitch;
        }
    }
}
