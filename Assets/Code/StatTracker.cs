using UnityEngine;

public class StatTracker : MonoBehaviour
{
    private static StatTracker _instance;
    public static StatTracker Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Two statTrackers exist - Deleting the duplicate!");
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [Header("Player Stats")]
    public int totalEnemiesKilled       = 0;
    public int enemiesPushedIntoBarrels = 0;
    public int smallHoopsJumpedThrough  = 0;
    public int bigHoopsJumpedThrough    = 0;
    public int enemiesKilledWithCannon  = 0;
}
