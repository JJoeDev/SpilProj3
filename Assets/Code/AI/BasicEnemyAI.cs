using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class BasicEnemyAI : MonoBehaviour
{
    [Header("Enemy vision")]
    [SerializeField] private float m_sightRadius = 30;
    [SerializeField] private float m_sightAngle = 215;
    [SerializeField] private Transform m_playerTransform = null;

    [Header("Enemy movement")]
    [SerializeField] WheelCollider m_frontRight;
    [SerializeField] WheelCollider m_frontLeft;
    [SerializeField] WheelCollider m_backRight;
    [SerializeField] WheelCollider m_backLeft;

    [SerializeField] private float m_maxTorque = 300;
    [SerializeField] private float m_minSpeed = 10;
    [SerializeField] private float m_maxSpeed = 300;
    [SerializeField] private float m_breakForce = 15;
    [SerializeField] private float m_maxBreakForce = 1;
    [Tooltip("When the distance from the ai position to the target corner is lower than this distance, the ai will start to slow down")]
    [SerializeField] private float m_slowDownDistance = 30;
    //[SerializeField] private float m_speed;
    [SerializeField] private float m_turnRadius = 36;
    [Tooltip("The distance the center of the enemy needs to be to the NavMesh corner before it moves to a new corner")]
    [SerializeField] private float m_distanceToNewCorner = 1.0f;

    private bool m_playerPathUpdating;

    [Header("Roaming area")]
    [SerializeField] private Vector3 m_roamAreaCenter = Vector3.zero;
    [SerializeField] private Vector2 m_roamAreaSize = Vector2.one;

    // Nav mesh related variables
    private NavMeshPath m_path;

    private Vector3 m_targetDir = Vector3.zero;
    private int m_currentCorner = 0;

    enum ENEMY_STATE
    {
        PATROLE, // No player visible, just roaming
        PLAYER_TARGETING, // Player visible, going for the kill
        PLAYER_ESCAPE, // Player just hit, retrieve and try again
        REVERSE, // Enemy hit a wall, try to get free
    }

    private ENEMY_STATE m_state;

    private void Start()
    {
        m_path = new NavMeshPath();

        GetRandomWorldPath();

        m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Assert.AreNotEqual(m_playerTransform, null, "PLAYER TRANSFORM IS NULL");
    }

    private void FixedUpdate()
    {
        if (m_path == null)
        {
            Debug.LogWarning("M_PATH IS NULL");
        }

        Vector3 playerDir = m_playerTransform.position - transform.position;

        // Is the player inside the visible radius and inside the visible angle
        if(playerDir.sqrMagnitude <= m_sightRadius * m_sightRadius)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, playerDir);

            if (angleToPlayer <= m_sightAngle / 2)
            {
                m_state = ENEMY_STATE.PLAYER_TARGETING;
                m_currentCorner = 0;
            }
            else if (m_state != ENEMY_STATE.PATROLE)
            {
                m_state = ENEMY_STATE.PATROLE;
                m_currentCorner = 0;
            }
        }
        else if(m_state != ENEMY_STATE.PATROLE) m_state = ENEMY_STATE.PATROLE;

        // If the player is seen and we are not following the player, follow the player
        if (m_state == ENEMY_STATE.PLAYER_TARGETING && !m_playerPathUpdating) StartCoroutine(ieUpdatePlayerPath());

        switch (m_state)
        {
            case ENEMY_STATE.PATROLE: // No player visible, just roaming
                EnemyPatrole();
                break;
            case ENEMY_STATE.PLAYER_TARGETING: // Player visible, going for the kill
                EnemyTargeting();
                break;
            case ENEMY_STATE.PLAYER_ESCAPE: // Player just hit, retrieve and try again
                EnemyRetrieve();
                break;
            case ENEMY_STATE.REVERSE: // Enemy hit a wall, try to get free
                EnemyReverse();
                break;
            default:
                Debug.LogError("How did you end up here?");
                break;
        }
    }

    /// <summary>
    /// Function <c>EnemyPatrole</c> runs when the AI can't see the player
    /// The enemy will find a random point inside roam area, and will drive towards that point.
    /// Once close enough to the point the AI will attempt to slow down
    /// </summary>
    void EnemyPatrole()
    {
        if (m_path == null || m_path.corners.Length == 0)
        {
            GetRandomWorldPath();
            return;
        }

        m_targetDir = m_path.corners[m_currentCorner] - transform.position;

        float distanceToCorner = m_targetDir.sqrMagnitude;
        if (distanceToCorner <= m_distanceToNewCorner && m_currentCorner < m_path.corners.Length - 1) m_currentCorner++;

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

        float calculatedAcceleration = desiredSpeed / m_maxSpeed * m_maxTorque;

        m_frontLeft.steerAngle = steeringAngle;
        m_frontRight.steerAngle = steeringAngle;

        m_frontLeft.motorTorque = calculatedAcceleration;
        m_frontRight.motorTorque = calculatedAcceleration;
    }

    /// <summary>
    /// Function <c>EnemyTargeting</c> This function runs once the AI can see the player.
    /// The AI will attempt to get a NavMesh path to the player, and then follow that path to get to the player.
    /// The enemy will attempt to go full speed and ram the player.
    /// </summary>
    void EnemyTargeting()
    {
        if (m_path.corners.Length < 1)
        {
            m_targetDir = m_playerTransform.position - transform.position;
        }
        else
        {
            m_targetDir = m_path.corners[m_currentCorner] - transform.position;
            if (m_targetDir.sqrMagnitude <= m_distanceToNewCorner && m_currentCorner < m_path.corners.Length - 1) m_currentCorner++;
        }
        
        m_targetDir.y = 0;

        float angleToCorner = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);

        float steeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;    

        m_frontLeft.steerAngle = steeringAngle;
        m_frontRight.steerAngle = steeringAngle;

        m_frontLeft.motorTorque = m_maxTorque;
        m_frontRight.motorTorque = m_maxTorque;
    }

    /// <summary>
    /// Function <c>EnemyRetrieve</c> is supposed to run once the enemy has hit the player.
    /// Once a hit is registered the AI will attempt to escape the player, in order to come back for a second hit.
    /// </summary>
    void EnemyRetrieve()
    {
        
    }

    /// <summary>
    /// Function <c>EnemyReverse</c> is for when the enemy hits a wall, and can't escape again.
    /// This simply puts the enemy in the reverse gear and gives it an attempt to get free
    /// </summary>
    void EnemyReverse()
    {
        
    }

    /// <summary>
    /// Function <c>GetRandomWorldPosition</c> gets a random world position to go to in the roam area
    /// <example>
    /// Example:
    /// <code>
    /// Vector3 randomPoint = GetRandomWorldPosition();
    /// </code>
    /// This example gets and stores a new random position in the randomPoint variable
    /// </example>
    /// </summary>
    /// <returns>
    /// A Vector3 position at a random position inside the defined roam area
    /// </returns>
    Vector3 GetRandomWorldPosition()
    {
        Vector3 randomPos;
        randomPos.x = Random.Range(m_roamAreaCenter.x - m_roamAreaSize.x * 0.5f, m_roamAreaCenter.x + m_roamAreaSize.x * 0.5f);
        randomPos.z = Random.Range(m_roamAreaCenter.z - m_roamAreaSize.y * 0.5f, m_roamAreaCenter.z + m_roamAreaSize.y * 0.5f);
        randomPos.y = 0;

        return randomPos;
    }

    /// <summary>
    /// Function <c>GetRandomWorldPath()</c> uses the <c>GetRandomWorldPosition</c> function for a random target position
    /// and then creates a NavMesh path to that target position.
    /// The NavMesh path is stored in m_path
    /// </summary>
    void GetRandomWorldPath()
    {
        NavMesh.CalculatePath(transform.position, GetRandomWorldPosition(), NavMesh.AllAreas, m_path);
        m_currentCorner = 0;
    }

    /// <summary>
    /// IEnumerator <c>ieUpdatePlayerPath</c> updates the NavMesh path towards the Player every 0.5s
    /// as long as the AI state is PLAYER_TARGETING
    /// </summary>
    /// <returns>WaitForSeconds(0.5f)</returns>
    IEnumerator ieUpdatePlayerPath()
    {
        m_playerPathUpdating = true;

        while(m_state == ENEMY_STATE.PLAYER_TARGETING)
        {
            yield return new WaitForSeconds(0.5f);
            NavMesh.CalculatePath(transform.position, m_playerTransform.position, NavMesh.AllAreas, m_path);
        }

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

            if (m_currentCorner < m_path.corners.Length - 1)
            {
                Gizmos.DrawSphere(m_path.corners[m_currentCorner], 0.3f);

                // Visualize slow down distance
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(m_path.corners[m_currentCorner], m_slowDownDistance);
            }
        }

        // Visualize enemy roam area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(m_roamAreaCenter, new Vector3(m_roamAreaSize.x, 2.0f, m_roamAreaSize.y));
    }
}