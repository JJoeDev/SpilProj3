using UnityEngine;
using Cinemachine;
using System.Linq;

public class CarCamera : MonoBehaviour
{
    [Header("CINEMACHINE FIELDS")]
    [SerializeField] private CinemachineFreeLook m_freeLookCam;
    [SerializeField] private CinemachineVirtualCamera m_trackCam;

    [Header("CAR FIELDS")]
    [SerializeField] private CarController m_carController;
    [SerializeField] private float m_lookAheadCarSpeed = 30.0f;
    [SerializeField] private float m_lookAheadRotationSpeed = 5f;

    [Header("ENEMY FIELDS")]
    [SerializeField] private string m_enemyTag = "Enemy";
    [SerializeField] private float m_trackCamDistance = 8f;
    [SerializeField] private float m_trackCamHeight = 7f;

    private Rigidbody m_rb;
    private bool m_useTrackCam = false;

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
        // Toggle camera with a key (currently C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!m_useTrackCam) // Trying to switch *into* track cam
            {
                Transform closestEnemy = FindClosestEnemy();
                if (closestEnemy != null)
                {
                    m_useTrackCam = true;
                    ActivateTrackCam();
                }
                else
                {
                    // No enemy, stay in freelook
                    Debug.Log("No enemies found, staying in FreeLook.");
                    m_useTrackCam = false;
                    ActivateFreeLook();
                }
            }
            else // Already in track cam, switch back
            {
                m_useTrackCam = false;
                ActivateFreeLook();
            }
        }

        if (!m_useTrackCam)
            HandleFreeLookCamera();
        else
            HandleTrackCam();
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

    private void HandleTrackCam()
    {
        Transform closestEnemy = FindClosestEnemy();
        if (closestEnemy == null)
        {
            // No enemy found, fallback to FreeLook
            m_useTrackCam = false;
            ActivateFreeLook();
            return;
        }

        // Direction on XZ plane only 
        Vector3 directionToEnemy = (closestEnemy.position - transform.position);
        directionToEnemy.y = 0f; // flatten so height doesn't skew camera
        directionToEnemy.Normalize();

        // Opposite position relative to enemy
        Vector3 oppositePos = transform.position - directionToEnemy * m_trackCamDistance;

        // Lock camera height to player's height + offset
        oppositePos.y = transform.position.y + m_trackCamHeight;

        // Smooth camera move and always look at enemy
        m_trackCam.transform.position = Vector3.Lerp(m_trackCam.transform.position, oppositePos, Time.deltaTime * 15f);
        m_trackCam.LookAt = closestEnemy;
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
        m_trackCam.Priority = 0;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void ActivateTrackCam()
    {
        m_freeLookCam.Priority = 0;
        m_trackCam.Priority = 10;

        var brain = Camera.main.GetComponent<CinemachineBrain>();
    }
}
