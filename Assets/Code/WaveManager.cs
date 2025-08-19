    using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    
    public GameObject[] enemyPrefab;
    private int m_enemyCount;
    public int waveNumber = 1;

    public float timeBetweenEnemySpawn;
    public float timeBetweenWaves;

    public Transform[] spawnPoints;

    private bool m_spawningWave;
     private int m_enemiesToSpawn;
    [SerializeField] private TextMeshProUGUI m_waveCounter;

    void Awake()
    {
        m_enemiesToSpawn = 1;
        StartCoroutine(SpawnEnemyWave(m_enemiesToSpawn));
    }


    // Update is called once per frame
    void Update()
    {
        m_enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length; //Enemy tag er ikke lavet endnu
        if (m_enemyCount == 0 && !m_spawningWave)
        {
            waveNumber++;
            m_enemiesToSpawn++;
            StartCoroutine(SpawnEnemyWave(m_enemiesToSpawn));
        }


    }
    IEnumerator SpawnEnemyWave(int enemiesToSpawn)
    {
        m_spawningWave = true;
        m_waveCounter.text = "Wave " + waveNumber;
        Debug.Log("Wave " + waveNumber);
        m_waveCounter.gameObject.SetActive(true);
        yield return new WaitForSeconds(timeBetweenWaves);
        m_waveCounter.gameObject.SetActive(false);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)]);
            Debug.Log("enemy spawned ");
            yield return new WaitForSeconds(timeBetweenEnemySpawn);
        }
        m_spawningWave = false;

    }
}
