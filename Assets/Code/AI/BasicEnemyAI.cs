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
    private float m_previouseSteeringAngle; // For smooth steering
    [Tooltip("The distance the center of the enemy needs to be to the NavMesh corner before it moves to a new corner")]
    [SerializeField] private float m_distanceToNewCorner = 1.0f;

    private bool m_playerPathUpdating;
    private bool m_directPlayerPath;

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
        Debug.Log($"ANGLE: {angleToCorner}");

        if (distanceToCorner < 4 && (angleToCorner > m_deadAngle || angleToCorner < -m_deadAngle))
        {
            m_currentCorner = 0;
            GetRandomWorldPath();
        }

        float steeringAngle = Mathf.Clamp(angleToCorner / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;

        float maxAllowedSpeed = Mathf.Sqrt(2 * m_brakeForce * distanceToCorner);
        float desiredSpeed = Mathf.Min(m_maxSpeed, maxAllowedSpeed);

        float currentSpeed = m_rb.velocity.magnitude;

        float accelerationRatio = desiredSpeed / m_maxSpeed;
        float torque = accelerationRatio * m_maxTorque;

        if (distanceToCorner < m_slowDownDistance)
        {
            desiredSpeed = Mathf.Lerp(m_minSpeed, m_maxSpeed, distanceToCorner / m_slowDownDistance);
        }

        if (currentSpeed > desiredSpeed)
        {
            float brakeForce = Mathf.Clamp(m_brakeForce * (currentSpeed - desiredSpeed), 0, m_brakeForce);

            SetBackWheelTorque(0); // Why are we setting to 0 multiple times
            SetAllWheelBrakes(brakeForce);
        }
        else
        {
            SetAllWheelBrakes(0); // Why are we setting to 0 multiple times
            SetBackWheelTorque(torque);
        }

        SetFrontWheelSteeringAngle(steeringAngle);
    }

    /// <summary>
    /// Function <c>EnemyTargeting</c> This function runs once the AI can see the player.
    /// The AI will attempt to get a NavMesh path to the player, and then follow that path to get to the player.
    /// The enemy will attempt to go full speed and ram the player.
    /// </summary>
    void EnemyTargeting()
    {
        float distanceToTarget = 10.0f;
        SetDirectionToTarget(ref distanceToTarget, true); // Allow direct targeting to player

        // Get the angle to the target, and make sure the steering angle can be a max of m_turnRadius in both + & -
        float angleToTarget = Vector3.SignedAngle(transform.forward, m_targetDir, Vector3.up);
        float targetSteeringAngle = Mathf.Clamp(angleToTarget / m_turnRadius, -1.0f, 1.0f) * m_turnRadius;

        // Smoothly set the steering angle
        // float steeringAngle = Mathf.Lerp(m_previouseSteeringAngle, targetSteeringAngle, Time.deltaTime * m_smoothSteering);
        // m_previouseSteeringAngle = steeringAngle;

        SetFrontWheelSteeringAngle(targetSteeringAngle);

        if (distanceToTarget > 1)
        {
            SetBackWheelTorque(m_maxTorque);
        }

        // float maxAllowedSpeed = Mathf.Sqrt(2 * (m_brakeForce / 2) * (distanceToTarget / 2));
        // float desiredSpeed = Mathf.Min(m_maxSpeed, maxAllowedSpeed);

        // float currentSpeed = m_rb.velocity.magnitude;

        // float accelerationRatio = desiredSpeed / m_maxSpeed;
        // float torque = accelerationRatio * m_maxTorque;

        // if (!m_directPlayerPath)
        // {
        //     if (distanceToTarget < m_slowDownDistance)
        //     {
        //         desiredSpeed = Mathf.Lerp(m_minSpeed, m_maxSpeed, distanceToTarget / m_slowDownDistance);
        //     }

        //     if (currentSpeed > desiredSpeed)
        //     {
        //         float brakeForce = Mathf.Clamp(m_brakeForce * (currentSpeed - desiredSpeed), 0, m_brakeForce);

        //         SetBackWheelTorque(0); // Why are we setting to 0 multiple times
        //         SetAllWheelBrakes(brakeForce);

        //         Debug.Log($"(targeting) BRAKING: {brakeForce} - DESIRED SPEED: {desiredSpeed} - SPEED {currentSpeed}");
        //     }
        //     else
        //     {
        //         SetAllWheelBrakes(0);
        //         SetBackWheelTorque(torque);
        //     }
        // }
        // else
        // {
        //     SetBackWheelTorque(torque);
        // }
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


    void SetControlledTorque(in float distanceToTarget)
    {
        
    }

    /// <summary>
    /// Function <c>SetFrontWheelSteeringAngle</c> sets the two front wheel colliders <c>steerAngle</c> to the desired angle
    /// </summary>
    /// <param name="angle">The desired angle to set the front wheels to</param>
    void SetFrontWheelSteeringAngle(in float angle)
    {
        m_frontLeft.steerAngle = angle;
        m_frontRight.steerAngle = angle;
    }

    /// <summary>
    /// Function <c>SetBackWheelTorque</c> sets the rear wheels torque. This is also what drives the vehicle forwards
    /// </summary>
    /// <param name="torque">The amount of torque to the rear wheels</param>
    void SetBackWheelTorque(in float torque)
    {
        m_backLeft.motorTorque = torque;
        m_backRight.motorTorque = torque;
    }

    /// <summary>
    /// Function <c>SetAllWheelBrakes</c> sets all the wheels to brake at the given brake force/torque
    /// </summary>
    /// <param name="brakeForce">The amount of brake force/torque to apply on each wheel</param>
    void SetAllWheelBrakes(in float brakeForce)
    {
        m_frontLeft.brakeTorque = brakeForce;
        m_frontRight.brakeTorque = brakeForce;
        m_backLeft.brakeTorque = brakeForce;
        m_backRight.brakeTorque = brakeForce;
    }

    /// <summary>
    /// Function <c>SetDirectionToCorner</c> sets the <c>m_targetDir</c> to the correct direction towards the new corner
    /// </summary>
    /// <param name="distanceToCorner">A reference to get the distance to next corner</param>
    /// <param name="allowPathToPlayer">Allow making a path directly to the player if possible</param>
    void SetDirectionToTarget(ref float distanceToCorner, bool allowPathToPlayer = false)
    {
        if (m_path.corners.Length >= 2)
        {
            m_directPlayerPath = false;

            // Calculate the distance to current corner.
            m_targetDir = m_path.corners[m_currentCorner] - transform.position;
            m_targetDir.y = 0;
            distanceToCorner = m_targetDir.sqrMagnitude;

            // If within range of last corner, create a new path.
            if (m_currentCorner >= m_path.corners.Length - 1)
            {
                GetRandomWorldPath();
                m_currentCorner = 0;
                // Reset direction
                m_targetDir = m_path.corners[m_currentCorner] - transform.position;
                m_targetDir.y = 0;
                distanceToCorner = m_targetDir.sqrMagnitude;
            }
            // If within range of the next corner, advance to it.
            else if (distanceToCorner <= m_distanceToNewCorner && m_currentCorner < m_path.corners.Length - 1)
            {
                m_currentCorner++;
                m_targetDir = m_path.corners[m_currentCorner] - transform.position;
                m_targetDir.y = 0;
                distanceToCorner = m_targetDir.sqrMagnitude;
            }
        }
        else if (allowPathToPlayer)
        {
            m_targetDir = m_playerTransform.position - transform.position;
            m_directPlayerPath = true;
            m_targetDir.y = 0;
            distanceToCorner = m_targetDir.sqrMagnitude;
        }
        else
        {
            GetRandomWorldPath();
            m_currentCorner = 0;
            // Reset direction
            m_targetDir = m_path.corners[m_currentCorner] - transform.position;
            m_targetDir.y = 0;
            distanceToCorner = m_targetDir.sqrMagnitude;
        }
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

        while (m_state == ENEMY_STATE.PLAYER_TARGETING)
        {
            yield return new WaitForSeconds(0.5f);
            NavMesh.CalculatePath(transform.position, m_playerTransform.position, NavMesh.AllAreas, m_path);
        }

        m_playerPathUpdating = false;
        yield return null;
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
        Gizmos.DrawLine(transform.position, transform.position + m_targetDir);

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