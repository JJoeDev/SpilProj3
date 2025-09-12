using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealEnemy : MonoBehaviour
{
    [SerializeField] int m_leftAliveThreshold = 3; // Number of enemies left to be alive before revealing
    List<GameObject> m_enemies; // List of all enemies in the scene


    private void RevealEnemies()
    {
        if (m_enemies == null || m_enemies.Count == 0) return;

        foreach (GameObject enemy in m_enemies)
        {
            if (enemy != null)
            {
                // Assuming enemies have a Renderer component to toggle visibility
                Renderer enemyRenderer = enemy.GetComponent<Renderer>();
                if (enemyRenderer != null)
                {
                    enemyRenderer.enabled = true; // Reveal the enemy
                }
            }
        }
    }
}

