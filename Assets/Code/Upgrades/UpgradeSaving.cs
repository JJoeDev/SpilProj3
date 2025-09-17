using System.Collections.Generic;
using UnityEngine;

public class UpgradeSaving : MonoBehaviour
{
    public static UpgradeSaving Instance;

    public List<string> acquiredUpgrades = new List<string>();
    public int savedScore = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetScore(int score)
    {
        savedScore = score;
    }

    public int GetScore()
    {
        return savedScore;
    }
}
