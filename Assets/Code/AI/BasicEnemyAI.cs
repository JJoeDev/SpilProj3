using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyAI : MonoBehaviour
{
    [Header("Enemy vision")]
    [SerializeField] private float m_sightRadius;
    [SerializeField] private float m_sightAngle;
    [SerializeField] private Transform m_playerTransform;

    [Header("Enemy movement")]
    [SerializeField] WheelCollider m_frontRight;
    [SerializeField] WheelCollider m_frontLeft;
    [SerializeField] WheelCollider m_backRight;
    [SerializeField] WheelCollider m_backLeft;

    [SerializeField] private float m_speed;
    [SerializeField] private float m_turnRadius;
    [Tooltip("The distance the center of the enemy needs to be to the NavMesh corner before it moves to a new corner")]
    [SerializeField] private float m_distanceTillNewCorner = 1.0f;
    private bool m_playerVisible;
    private bool m_playerPathUpdating;

    // Nav mesh related variables
    private NavMeshPath m_path;

    //private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_targetDir = Vector3.zero;
    private int m_currentCorner = 0;

    private void Start()
    {
        m_path = new NavMeshPath();

        NavMesh.CalculatePath(transform.position, m_playerTransform.position, NavMesh.AllAreas, m_path);
    }

    private void Update()
    {
        Vector3 playerDir = m_playerTransform.position - transform.position;

        if(playerDir.magnitude <= m_sightRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, playerDir);
            if (angleToPlayer <= m_sightAngle / 2) m_playerVisible = true;
            else if (m_playerVisible) m_playerVisible = false;
        }
        else if(m_playerVisible) m_playerVisible = false;

        if(m_playerVisible && !m_playerPathUpdating) StartCoroutine(ieUpdatePlayerPath());
        else if(!m_playerVisible)
        {
            m_targetDir = m_path.corners[m_currentCorner] - transform.position;

            if(m_targetDir.magnitude <= m_distanceTillNewCorner)
            {
                m_currentCorner++;
                Debug.Log($"NEW CORNER: {m_path.corners[m_currentCorner]}");
            }

            if(m_currentCorner != m_path.corners.Length - 1)
            {
                float angleToCorner = Vector3.Angle(transform.forward, m_path.corners[m_currentCorner]);
            }
        }
    }

    IEnumerator ieUpdatePlayerPath()
    {
        m_playerPathUpdating = true;

        while(m_playerVisible)
        {
            yield return new WaitForSeconds(1);
            NavMesh.CalculatePath(transform.position, m_playerTransform.position, NavMesh.AllAreas, m_path);
        }

        m_playerPathUpdating = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_sightRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_distanceTillNewCorner);

        Gizmos.color = Color.yellow;

        float minAngle = -m_sightAngle / 2;
        Vector3 leftAngle = Quaternion.Euler(0.0f, minAngle, 0.0f) * transform.forward;
        Vector3 rightAngle = Quaternion.Euler(0.0f, -minAngle, 0.0f) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * m_sightRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightAngle * m_sightRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftAngle * m_sightRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + m_targetDir.normalized * 5);

        if(EditorApplication.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 prev = Vector3.zero;
            foreach (Vector3 pos in m_path.corners)
            {
                if (prev != Vector3.zero)
                {
                    Gizmos.DrawLine(prev, pos);
                }
                else
                {
                    Gizmos.DrawLine(transform.position, pos);
                }

                prev = pos;
            }

            if (m_currentCorner > m_path.corners.Length - 1) return;
            Gizmos.DrawSphere(m_path.corners[m_currentCorner], 0.3f);
        }
    }
}