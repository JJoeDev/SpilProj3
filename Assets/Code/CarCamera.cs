using UnityEngine;
using Cinemachine;

public class CarCamera : MonoBehaviour
{
    [Header("CINEMACHINE FIELDS")]
    [SerializeField] private CinemachineFreeLook m_freeLookCam;

    [Header("CAR FIELDS")]
    [SerializeField] private CarController m_prometeoController;
    [Space(10)]
    [SerializeField] private float m_lookAheadCarSpeed = 30.0f;
    [SerializeField] private float m_lookAheadRotationSpeed;

    private Rigidbody m_rb;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(m_prometeoController.carSpeed > m_lookAheadCarSpeed)
        {
            Vector3 moveDir = m_rb.velocity;
            moveDir.y = 0;
            
            if(moveDir.sqrMagnitude > m_lookAheadCarSpeed)
            {
                float moveYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

                float currentYaw = m_freeLookCam.m_XAxis.Value;
                float smoothYaw = Mathf.LerpAngle(currentYaw, moveYaw, Time.deltaTime * m_lookAheadRotationSpeed);

                m_freeLookCam.m_XAxis.Value = smoothYaw;

                Vector3 smoothDir = Quaternion.Euler(0.0f, smoothYaw, 0.0f) * Vector3.forward;
                Vector3 moveDirQ = Quaternion.Euler(0.0f, moveYaw, 0.0f) * Vector3.forward;

                Debug.DrawLine(transform.position, transform.position + smoothDir * 50 * 50, Color.green, 0.1f, false);
                Debug.DrawLine(transform.position, transform.forward + moveDirQ * 50, Color.red, 0.1f, false);
            }
        }
    }
}
