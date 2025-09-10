using System.Collections.Generic;
using UnityEngine;

public class UpgradeSaving : MonoBehaviour
{
    public static UpgradeSaving Instance;

    public List<string> acquiredUpgrades = new List<string>();
    public int savedScore = 0; // Add this to track score

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

    // Optional helper to set score
    public void SetScore(int score)
    {
        savedScore = score;
    }

    // Optional helper to get score
    public int GetScore()
    {
        return savedScore;
    }
}
