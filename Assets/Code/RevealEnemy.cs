using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealEnemy : MonoBehaviour
{
    [SerializeField] float revealTime = 5f;
    GameObject[] m_enemies; // List of all enemies in the scene

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
        m_enemies = GameObject.FindGameObjectsWithTag("RevealedEnemy");

        if (m_enemies == null || m_enemies.Length == 0)
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

