using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealEnemy : MonoBehaviour
{
    [SerializeField] float revealTime = 5f;
    [SerializeField] GameObject[] m_enemies;

    InputManager m_inputManager;

    private void Start()
    {
        m_inputManager = InputManager.Instance;
    }

    private void Update()
    {
        if (m_inputManager.OnRevealEnemies().triggered)
        {
            StartCoroutine(RevealEnemies());
        }
    }

    private IEnumerator RevealEnemies()
    {
        Debug.Log("FINDING ENEMIES...");
        m_enemies = GameObject.FindGameObjectsWithTag("EnemyRevealed");

        if ( m_enemies.Length <= 0)
        {
            Debug.Log("Couldn't find any enemies"); 
            yield break;
        }

        Debug.Log("Found " +  m_enemies.Length + " enemies");

        foreach (GameObject enemy in m_enemies)
        {
            if (enemy != null) 
            {
                MeshRenderer enemyRenderer = enemy.GetComponent<MeshRenderer>();
                if (enemyRenderer != null)
                {
                    enemyRenderer.enabled = true;
                }
            }
        }

        yield return new WaitForSeconds(revealTime);

        foreach (GameObject enemy in m_enemies)
        {
            if (enemy != null)
            {
                MeshRenderer enemyRenderer = enemy.GetComponent<MeshRenderer>();
                if (enemyRenderer != null)
                {
                    enemyRenderer.enabled = false;
                }
            }
        }
    }
}

