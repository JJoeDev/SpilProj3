using UnityEngine;
using Cinemachine;

#region 1) Roll -> Dutch (uændret)
[ExecuteAlways]
public class CM_TargetRollOnly : CinemachineExtension
{
    public Transform target;                             // tom = vcam.Follow
    [Range(0.1f, 30f)] public float rollSmooth = 10f;
    [Range(0f, 90f)] public float maxRoll = 60f;
    public float rollOffset = 0f;
    [Range(0f, 10f)] public float rollDeadZone = 0.25f;

    float _rollSmoothed;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage,
        ref CameraState state, float dt)
    {
        if (stage != CinemachineCore.Stage.Aim) return;

        var t = target ? target : vcam.Follow;
        if (!t) return;

        float desired = Vector3.SignedAngle(Vector3.up, t.up, t.forward) + rollOffset;
        if (Mathf.Abs(desired) < rollDeadZone) desired = 0f;
        desired = Mathf.Clamp(desired, -maxRoll, maxRoll);

        float a = Application.isPlaying ? 1f - Mathf.Exp(-rollSmooth * Mathf.Max(dt, 1e-4f)) : 1f;
        _rollSmoothed = Mathf.LerpAngle(_rollSmoothed, desired, a);

        var lens = state.Lens;
        if (Mathf.Abs(lens.Dutch - _rollSmoothed) > 0.01f)
        { lens.Dutch = _rollSmoothed; state.Lens = lens; }

        if (vcam is CinemachineFreeLook fl)
        {
            var l = fl.m_Lens;
            if (Mathf.Abs(l.Dutch - _rollSmoothed) > 0.01f)
            { l.Dutch = _rollSmoothed; fl.m_Lens = l; }
        }
    }
}
#endregion

#region 2) Auto-binding med heading-match + (valgfri) yaw-assist
[RequireComponent(typeof(CinemachineFreeLook))]
public class FL_AutoBindingOnBank : MonoBehaviour
{
    [Header("References")]
    public Transform bankSource; // normalt bilen (tom = FreeLook.Follow)
    public Rigidbody targetRb;   // valgfri – til at pause assist ved lav fart

    [Header("Bank thresholds (deg)")]
    public float enterBankDeg = 25f;   // > skift til LockToTarget
    public float exitBankDeg = 18f;   // < tilbage til WorldSpace
    public float enterHold = 0.10f; // kræv bank > enter i så mange sek
    public float minSwitchInterval = 0.30f; // undgå flimmer

    [Header("Modes")]
    public CinemachineTransposer.BindingMode flatMode = CinemachineTransposer.BindingMode.WorldSpace;
    public CinemachineTransposer.BindingMode bankedMode = CinemachineTransposer.BindingMode.LockToTarget;

    [Header("Yaw assist når banked (valgfri)")]
    public bool yawAssist = true;
    public float yawStrength = 1.2f;        // 0.8–2.0
    public float minSpeedToAssist = 0.5f;   // m/s (kræver targetRb)
    public int mouseButton = 1;           // hold RMB = ingen assist
    public float mouseDeltaThreshold = 0.02f;

    CinemachineFreeLook fl;
    CinemachineOrbitalTransposer[] bodies = new CinemachineOrbitalTransposer[3];
    bool isBanked;
    float bankTimer;
    float lastSwitch;

    void Awake()
    {
        fl = GetComponent<CinemachineFreeLook>();
        CacheBodies();
        if (!bankSource) bankSource = fl ? fl.Follow : null;
        ApplyMode(flatMode); // start i World Space (din favorit)
    }

    void CacheBodies()
    {
        for (int i = 0; i < 3; i++)
        {
            var rig = fl.GetRig(i);
            bodies[i] = rig ? rig.GetCinemachineComponent<CinemachineOrbitalTransposer>() : null;
        }
    }

    void LateUpdate()
    {
        if (!bankSource) return;

        float bank = Mathf.Abs(Vector3.SignedAngle(Vector3.up, bankSource.up, bankSource.forward));
        float now = Time.time;

        // ——— BANK STATE ———
        if (!isBanked)
        {
            if (bank > enterBankDeg) bankTimer += Time.deltaTime; else bankTimer = 0f;

            if (bankTimer >= enterHold && now - lastSwitch > minSwitchInterval)
            { SwitchMode(true); }
        }
        else
        {
            if (bank < exitBankDeg && now - lastSwitch > minSwitchInterval)
            { SwitchMode(false); }
        }

        // ——— YAW ASSIST KUN NÅR BANKED ———
        if (isBanked && yawAssist)
        {
            // pause når spilleren styrer
            bool userMoving =
                Input.GetMouseButton(mouseButton) ||
                Mathf.Abs(Input.GetAxisRaw("Mouse X")) > mouseDeltaThreshold ||
                Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > mouseDeltaThreshold;

            if (!userMoving)
            {
                // pause ved meget lav fart (hvis RB findes)
                if (targetRb && targetRb.velocity.magnitude < minSpeedToAssist) { /*do nothing*/ }
                else
                {
                    // drej FreeLook X en smule mod bilens heading omkring target.up
                    Vector3 up = bankSource.up;
                    Vector3 camF = Vector3.ProjectOnPlane(transform.forward, up).normalized;
                    Vector3 tarF = Vector3.ProjectOnPlane(bankSource.forward, up).normalized;
                    if (camF.sqrMagnitude > 1e-6f && tarF.sqrMagnitude > 1e-6f)
                    {
                        float err = Vector3.SignedAngle(camF, tarF, up); // [-180..180]
                        float step = err * Mathf.Clamp(yawStrength, 0f, 10f) * Time.deltaTime;
                        fl.m_XAxis.Value += step; // blidt “nudging”, stadig fuld FreeLook
                    }
                }
            }
        }
    }

    void SwitchMode(bool toBanked)
    {
        isBanked = toBanked;
        lastSwitch = Time.time;

        // match X-axis heading så kameraet IKKE "popper" ved mode-skift
        Vector3 upRef = toBanked ? bankSource.up : Vector3.up;
        float newHeading = ComputeHeadingAroundUp(bankSource, transform.position, upRef);
        fl.m_XAxis.Value = newHeading;

        ApplyMode(toBanked ? bankedMode : flatMode);
    }

    void ApplyMode(CinemachineTransposer.BindingMode m)
    {
        for (int i = 0; i < bodies.Length; i++)
            if (bodies[i] != null) bodies[i].m_BindingMode = m;
    }

    // Vinkel mellem target.forward og vektoren mod kameraet, målt omkring upRef
    float ComputeHeadingAroundUp(Transform tgt, Vector3 camPos, Vector3 upRef)
    {
        Vector3 toCam = Vector3.ProjectOnPlane(camPos - tgt.position, upRef).normalized;
        Vector3 fwd = Vector3.ProjectOnPlane(tgt.forward, upRef).normalized;
        if (toCam.sqrMagnitude < 1e-6f || fwd.sqrMagnitude < 1e-6f)
            return fl.m_XAxis.Value;

        float ang = Vector3.SignedAngle(fwd, toCam, upRef); // [-180..180]
        // FreeLook tager grader direkte i XAxis.Value
        return ang;
    }
}
#endregion
