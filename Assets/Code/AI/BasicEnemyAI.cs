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
    [Tooltip("The dead angle is the angle for roaming where the ai will just find a new path, instead of trying to follow the current path")]
    [SerializeField] private float m_deadAngle = 75;
    [SerializeField] private Transform m_playerTransform = null;

    [Header("Enemy movement")]
    [SerializeField] WheelCollider m_frontRight;
    [SerializeField] WheelCollider m_frontLeft;
    [SerializeField] WheelCollider m_backRight;
    [SerializeField] WheelCollider m_backLeft;

    [SerializeField] private float m_maxTorque = 300;
    [SerializeField] private float m_minSpeed = 10;
    [SerializeField] private float m_maxSpeed = 300;
    [SerializeField] private float m_brakeForce = 15;
    [Tooltip("When the distance from the ai position to the target corner is lower than this distance, the ai will start to slow down")]
    [SerializeField] private float m_slowDownDistance = 30;
    [SerializeField] private float m_turnRadius = 36;
    [SerializeField] private float m_smoothSteering = 5.0f;
    [Tooltip("The distance the center of the enemy needs to be to the NavMesh corner before it moves to a new corner")]
    [SerializeField] private float m_distanceToNewCorner = 1.0f;

    private bool m_playerPathUpdating;

    [Header("Roaming area")]
    [SerializeField] private Vector3 m_roamAreaCenter = Vector3.zero;
    [SerializeField] private Vector2 m_roamAreaSize = Vector2.one;

    private Rigidbody m_rb;

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
        m_rb = GetComponent<Rigidbody>();
        m_path = new NavMeshPath();

        GetRandomWorldPath();

        m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        Assert.AreNotEqual(m_playerTransform, null, "PLAYER TRANSFORM IS NULL");
    }

    private void FixedUpdate()
    {
        Assert.AreNotEqual(m_path, null, "NAV MESH PATH IS NULL");
        Vector3 playerDir = m_playerTransform.position - transform.position;

        // Is the player inside the visible radius and inside the visible angle
        if (playerDir.sqrMagnitude <= m_sightRadius * m_sightRadius)
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
        else if (m_state != ENEMY_STATE.PATROLE)
        {
            m_state = ENEMY_STATE.PATROLE;
        }

        // If the player is seen and we are not following the player, follow the player
        if (m_state == ENEMY_STATE.PLAYER_TARGETING && !m_playerPathUpdating) StartCoroutine(ieUpdatePlayerPath());

        switch (m_state)
        {
            case ENEMY_STATE.PATROLE: // No player visible, just roaming
                //EnemyPatrole();
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

        float distanceToCorner = 0;
        SetDirectionToTarget(ref distanceToCorner);

        float angleToCorner = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);

        if (m_currentCorner >= m_path.corners.Length - 1 || angleToCorner > m_deadAngle || angleToCorner < -m_deadAngle)
        {
            GetRandomWorldPath();
            m_frontLeft.brakeTorque = 0;
            m_frontRight.brakeTorque = 0;
        }

        distanceToCorner = Vector3.Distance(transform.position, m_path.corners[m_currentCorner]);
        float steeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;

        float maxAllowedSpeed = Mathf.Sqrt(2 * m_brakeForce * distanceToCorner);
        float desiredSpeed = Mathf.Min(m_maxSpeed, maxAllowedSpeed);

        float currentSpeed = m_rb.velocity.magnitude;

        float accelRatio = desiredSpeed / m_maxSpeed;
        float torque = accelRatio * m_maxTorque;

        if (distanceToCorner < m_slowDownDistance)
        {
            desiredSpeed = Mathf.Lerp(m_minSpeed, m_maxSpeed, distanceToCorner / m_slowDownDistance);
        }

        if (currentSpeed > desiredSpeed)
        {
            float brakeForce = Mathf.Clamp(m_brakeForce * (currentSpeed - desiredSpeed), 0, m_brakeForce);
            m_frontLeft.brakeTorque = brakeForce;
            m_frontRight.brakeTorque = brakeForce;
            m_backLeft.motorTorque = 0;
            m_backRight.motorTorque = 0;
            Debug.Log($"BRAKING: {brakeForce} - DESIRED SPEED: {desiredSpeed} - SPEED {currentSpeed}");
        }
        else
        {
            m_frontLeft.brakeTorque = 0;
            m_frontRight.brakeTorque = 0;
            m_backLeft.motorTorque = torque;
            m_backRight.motorTorque = torque;
        }

        m_frontLeft.steerAngle = steeringAngle;
        m_frontRight.steerAngle = steeringAngle;
    }

    /// <summary>
    /// Function <c>EnemyTargeting</c> This function runs once the AI can see the player.
    /// The AI will attempt to get a NavMesh path to the player, and then follow that path to get to the player.
    /// The enemy will attempt to go full speed and ram the player.
    /// </summary>
    void EnemyTargeting()
    {
        float distanceToTarget = 0;
        SetDirectionToTarget(ref distanceToTarget, true);

        float angleToCorner = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);

        float targetSteeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;
        float currentSteeringAngleL = m_frontLeft.steerAngle;
        float currentSteeringAngleR = m_frontRight.steerAngle;
        float newSteeringAngleL = Mathf.Lerp(currentSteeringAngleL, targetSteeringAngle, Time.fixedDeltaTime * m_smoothSteering);
        float newSteeringAngleR = Mathf.Lerp(currentSteeringAngleR, targetSteeringAngle, Time.fixedDeltaTime * m_smoothSteering);

        m_frontLeft.steerAngle = newSteeringAngleL;
        m_frontRight.steerAngle = newSteeringAngleR;

        //float distanceToTarget = m_targetDir.magnitude;
        float desiredSpeed = m_maxSpeed;
        if (distanceToTarget < m_slowDownDistance)
        {
            desiredSpeed = Mathf.Lerp(m_minSpeed, m_maxSpeed, distanceToTarget / m_slowDownDistance);
            Debug.Log($"SLOWING DOWN! DESIRED SPEED -> {desiredSpeed}");
        }

        float currentSpeed = m_rb.velocity.magnitude;
        float targetTorque = (desiredSpeed / m_maxSpeed) * m_maxTorque;

        if (currentSpeed > desiredSpeed)
        {
            float brakeForce = Mathf.Clamp(m_brakeForce * (currentSpeed - desiredSpeed), 0, m_brakeForce);
            m_frontLeft.brakeTorque = brakeForce;
            m_frontRight.brakeTorque = brakeForce;
            m_frontLeft.motorTorque = 0;
            m_frontRight.motorTorque = 0;
        }
        else
        {
            m_frontLeft.brakeTorque = 0;
            m_frontRight.brakeTorque = 0;
            m_frontLeft.motorTorque = targetTorque;
            m_frontRight.motorTorque = targetTorque;
        }
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
    /// Function <c>SetDirectionToCorner</c> sets the <c>m_targetDir</c> to the correct direction towards the new corner
    /// </summary>
    /// <param name="distanceToCorner">A reference to get the distance to next corner</param>
    /// <param name="allowPathToPlayer">Allow making a path directly to the player if possible</param>
    void SetDirectionToTarget(ref float distanceToCorner, bool allowPathToPlayer = false)
    {
        if (allowPathToPlayer)
        {
            // If the path towards the player is less than 1 then set the target direction towards player position
            if (m_path.corners.Length < 1) m_targetDir = m_playerTransform.position - transform.position;
        }
        else
        {
            if (distanceToCorner <= m_distanceToNewCorner && m_currentCorner < m_path.corners.Length - 1) m_currentCorner++;
            m_targetDir = m_path.corners[m_currentCorner] - transform.position;
        }

        m_targetDir.y = 0;
        distanceToCorner = m_targetDir.sqrMagnitude;
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

        // Visualize the angles where patrol wil generate a new path
        Gizmos.color = Color.red;
        leftAngle = Quaternion.Euler(0.0f, m_deadAngle, 0.0f) * transform.forward;
        rightAngle = Quaternion.Euler(0.0f, -m_deadAngle, 0.0f) * transform.forward;

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