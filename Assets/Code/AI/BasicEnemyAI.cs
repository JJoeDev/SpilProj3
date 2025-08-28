using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;

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

    [SerializeField] private float m_maxTorque;
    [SerializeField] private float m_minSpeed;
    [SerializeField] private float m_maxSpeed;
    [SerializeField] private float m_breakForce;
    [SerializeField] private float m_maxBreakForce;
    [Tooltip("When the distance from the ai position to the target corner is lower than this distance, the ai will start to slow down")]
    [SerializeField] private float m_slowDownDistance;
    //[SerializeField] private float m_speed;
    [SerializeField] private float m_turnRadius;
    [Tooltip("The distance the center of the enemy needs to be to the NavMesh corner before it moves to a new corner")]
    [SerializeField] private float m_distanceToNewCorner = 1.0f;

    private bool m_playerVisible;
    private bool m_playerPathUpdating;

    [Header("Roaming area")]
    [SerializeField] private Vector3 m_roamAreaCenter = Vector3.zero;
    [SerializeField] private Vector2 m_roamAreaSize = Vector2.one;

    // Nav mesh related variables
    private NavMeshPath m_path;

    private Rigidbody m_rb;

    //private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_targetDir = Vector3.zero;
    private int m_currentCorner = 0;

    enum ENEMY_STATE 
    {
        PATROLE,
        PLAYER_TARGETING,
    }

    private ENEMY_STATE m_state;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_path = new NavMeshPath();

        GetRandomWorldPath();
    }

    private void FixedUpdate()
    {
        Vector3 playerDir = m_playerTransform.position - transform.position;

        // Is the player inside the visible radius and inside the visible angle
        if(playerDir.sqrMagnitude <= m_sightRadius * m_sightRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, playerDir);

            if (angleToPlayer <= m_sightAngle / 2) m_state = ENEMY_STATE.PLAYER_TARGETING; 
            else if (m_state != ENEMY_STATE.PATROLE) m_state = ENEMY_STATE.PATROLE;
        }
        else if(m_state != ENEMY_STATE.PATROLE) m_state= ENEMY_STATE.PATROLE;

        // If the player is seen and we are not following the player, follow the player
        if (m_state == ENEMY_STATE.PLAYER_TARGETING && !m_playerPathUpdating) StartCoroutine(ieUpdatePlayerPath());

        switch(m_state)
        {
            case ENEMY_STATE.PATROLE:
                EnemyPatrole();
                break;
            case ENEMY_STATE.PLAYER_TARGETING:
                EnemyTargeting();
                break;
            default:
                Debug.LogError("How did you end up here?");
                break;
        }
    }

    // Slows down and drives smoother
    void EnemyPatrole()
    {
        m_targetDir = m_path.corners[m_currentCorner] - transform.position;

        float distanceToCorner = m_targetDir.sqrMagnitude;
        if (distanceToCorner <= m_distanceToNewCorner) m_currentCorner++;

        if (m_currentCorner >= m_path.corners.Length)
        {
            GetRandomWorldPath();
            m_frontLeft.brakeTorque = 0;
            m_frontRight.brakeTorque = 0;
        }

        m_targetDir = m_path.corners[m_currentCorner] - transform.position;
        m_targetDir.y = 0;
        distanceToCorner = m_targetDir.magnitude;

        float angleToCorner = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);
        float steeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;

        float maxAllowedSpeed = Mathf.Sqrt(2 * m_breakForce * distanceToCorner);
        float desiredSpeed = m_maxSpeed;

        if (distanceToCorner <= m_slowDownDistance)
        {
            desiredSpeed = Mathf.Min(Mathf.Lerp(m_minSpeed, m_maxSpeed, Mathf.Clamp01(distanceToCorner / m_slowDownDistance)), maxAllowedSpeed);
        }

        float calculatedAcceleration = (desiredSpeed / m_maxSpeed) * m_maxTorque;

        m_frontLeft.steerAngle = steeringAngle;
        m_frontRight.steerAngle = steeringAngle;

        m_frontLeft.motorTorque = calculatedAcceleration;
        m_frontRight.motorTorque = calculatedAcceleration;

        Debug.Log($"CALC ACCELERATION: {calculatedAcceleration} - DESIRED SPEED: {desiredSpeed}");
    }

    // Just full throttle towards the player
    void EnemyTargeting()
    {
        m_targetDir = m_path.corners[m_currentCorner] - transform.position;
        if (m_targetDir.sqrMagnitude <= m_distanceToNewCorner && m_currentCorner < m_path.corners.Length) m_currentCorner++;

        m_targetDir = m_path.corners[m_currentCorner] - transform.position;
        m_targetDir.y = 0;

        float angleToCorner = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);

        float steeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;

        m_frontLeft.steerAngle = steeringAngle;
        m_frontRight.steerAngle = steeringAngle;

        m_frontLeft.motorTorque = m_maxTorque;
        m_frontRight.motorTorque = m_maxTorque;
    }

    Vector3 GetRandomWorldPosition()
    {
        Vector3 randomPos;
        randomPos.x = Random.Range(m_roamAreaCenter.x - m_roamAreaSize.x * 0.5f, m_roamAreaCenter.x + m_roamAreaSize.x * 0.5f);
        randomPos.z = Random.Range(m_roamAreaCenter.z - m_roamAreaSize.y * 0.5f, m_roamAreaCenter.z + m_roamAreaSize.y * 0.5f);
        randomPos.y = 0;

        return randomPos;
    }

    void GetRandomWorldPath()
    {
        NavMesh.CalculatePath(transform.position, GetRandomWorldPosition(), NavMesh.AllAreas, m_path);
        m_currentCorner = 0;
    }

    IEnumerator ieUpdatePlayerPath()
    {
        m_playerPathUpdating = true;

        Debug.Log("PLAYER VISIBLE");

        while(m_state == ENEMY_STATE.PLAYER_TARGETING)
        {
            yield return new WaitForSeconds(0.5f);
            NavMesh.CalculatePath(transform.position, m_playerTransform.position, NavMesh.AllAreas, m_path);
        }

        Debug.Log("PLAYER NOT VISIBLE");

        m_playerPathUpdating = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize sight radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_sightRadius);

        // Visualize radius for detecting corners on NavMesh
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, m_distanceToNewCorner);

        // Visualize the sight angle
        Gizmos.color = Color.yellow;
        float minAngle = -m_sightAngle / 2;
        Vector3 leftAngle = Quaternion.Euler(0.0f, minAngle, 0.0f) * transform.forward;
        Vector3 rightAngle = Quaternion.Euler(0.0f, -minAngle, 0.0f) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * m_sightRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightAngle * m_sightRadius);
        Gizmos.DrawLine(transform.position, transform.position + leftAngle * m_sightRadius);

        // Visualize the target move direction
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + m_targetDir.normalized * 5);

        // Visualize the complete NavMesh path to desiret position
        if (EditorApplication.isPlaying)
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

            // Visualize slow down distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(m_path.corners[m_currentCorner], m_slowDownDistance);
        }

        // Visualize enemy roam area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(m_roamAreaCenter, new Vector3(m_roamAreaSize.x, 2.0f, m_roamAreaSize.y));
    }
}