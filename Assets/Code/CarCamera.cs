using UnityEngine;
using Cinemachine;
using System.Linq;

public class CarCamera : MonoBehaviour
{
    [Header("CINEMACHINE FIELDS")]
    [SerializeField] private CinemachineFreeLook m_freeLookCam;
    [SerializeField] private CinemachineVirtualCamera m_ballCam;

    [Header("CAR FIELDS")]
    [SerializeField] private CarController m_carController;
    [SerializeField] private float m_lookAheadCarSpeed = 30.0f;
    [SerializeField] private float m_lookAheadRotationSpeed = 5f;

    [Header("ENEMY FIELDS")]
    [SerializeField] private string m_enemyTag = "Enemy"; // Tag your enemies
    [SerializeField] private float m_ballCamDistance = 10f;
    [SerializeField] private float m_ballCamHeight = 3f;

    private Rigidbody m_rb;
    private bool m_useBallCam = false;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Start in FreeLook
        ActivateFreeLook();
    }

    private void Update()
    {
        // Toggle camera with a key (example: C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            m_useBallCam = !m_useBallCam;
            if (m_useBallCam)
                ActivateBallCam();
            else
                ActivateFreeLook();
        }

        if (!m_useBallCam)
            HandleFreeLookCamera();
        else
            HandleBallCam();
    }

    private void HandleFreeLookCamera()
    {
        if (m_carController.carSpeed > m_lookAheadCarSpeed)
        {
            Vector3 moveDir = m_rb.velocity;
            moveDir.y = 0;

            if (moveDir.sqrMagnitude > m_lookAheadCarSpeed)
            {
                float moveYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

                float currentYaw = m_freeLookCam.m_XAxis.Value;
                float smoothYaw = Mathf.LerpAngle(currentYaw, moveYaw, Time.deltaTime * m_lookAheadRotationSpeed);

                m_freeLookCam.m_XAxis.Value = smoothYaw;
            }
        }
    }

    private void HandleBallCam()
    {
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy == null)
        {
            // No enemy found, fallback to FreeLook
            m_useBallCam = false;
            ActivateFreeLook();
            return;
        }

        // Direction on XZ plane only (ignore height difference)
        Vector3 directionToEnemy = (closestEnemy.position - transform.position);
        directionToEnemy.y = 0f; // flatten so height doesn't skew camera
        directionToEnemy.Normalize();

        // Opposite position relative to player
        Vector3 oppositePos = transform.position - directionToEnemy * m_ballCamDistance;

        // Lock camera height to player's height + offset
        oppositePos.y = transform.position.y + m_ballCamHeight;

        // Smooth camera move and always look at enemy
        m_ballCam.transform.position = Vector3.Lerp(m_ballCam.transform.position, oppositePos, Time.deltaTime * 15f);
        m_ballCam.LookAt = closestEnemy;
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(m_enemyTag);
        if (enemies.Length == 0) return null;

        return enemies
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .First().transform;
    }

    private void ActivateFreeLook()
    {
        m_freeLookCam.Priority = 10;
        m_ballCam.Priority = 0;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Time = 0f; // instant cut
    }

    private void ActivateBallCam()
    {
        m_freeLookCam.Priority = 0;
        m_ballCam.Priority = 10;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
        brain.m_DefaultBlend.m_Time = 0f; // instant cut
    }

}
