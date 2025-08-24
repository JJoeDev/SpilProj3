using UnityEngine;
using Cinemachine;

public class CarCamera : MonoBehaviour
{
    [Header("CINEMACHINE FIELDS")]
    [SerializeField] private CinemachineFreeLook m_freeLookCam;

    [Header("CAR FIELDS")]
    [SerializeField] private PrometeoCarController m_prometeoController;
    [Space(10)]
    [SerializeField] private float m_lookAheadCarSpeed = 30.0f;
    [SerializeField] private float m_lookAheadRotationSpeed;

    private Rigidbody m_rb;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(m_prometeoController.carSpeed > m_lookAheadCarSpeed)
        {
            Vector3 moveDir = m_rb.velocity;
            moveDir.y = 0.0f;

            if(moveDir.magnitude > 0.01f)
            {
                float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z);

                float currentYaw = m_freeLookCam.m_XAxis.Value;
                float smoothedYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * m_lookAheadRotationSpeed);

                m_freeLookCam.m_XAxis.Value = smoothedYaw;
            }
        }
    }
}
