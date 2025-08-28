    using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    
    public GameObject[] enemyPrefab; //Array med fjendetyper
    private int m_enemyCount; //antal fjender tilbage
    public int waveNumber = 1; //bølgenummer

    public float timeBetweenEnemySpawn; //tidsinterval mellem fjenders fremkaldelse
    public float timeBetweenWaves;  //tidsinterval mellem bølger

    public Transform[] spawnPoints; // array med fremkaldelsespunkter

    private bool m_spawningWave; //bool, der holder styr på, om en bølge er ved at blive fremkaldt
     private int m_enemiesToSpawn; //antallet af fjender der skal fremkaldes
    [SerializeField] private TextMeshProUGUI m_waveCounter; //UI element, der visualiserer bølgenummeret

    void Awake() // første bølge starter, når scenen vågner
    {
        m_enemiesToSpawn = 1; //antallet af fjender der skal fremkaldes bestemmes her
        StartCoroutine(SpawnEnemyWave(m_enemiesToSpawn)); //starter en siderutine
    }


    // Update is called once per frame
    void Update()
    {
        m_enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length; //Enemy tag er ikke lavet endnu
        if (m_enemyCount == 0 && !m_spawningWave)
        {
            waveNumber++; //bølgenummeret går op
            m_enemiesToSpawn++;  //antallet af fjender der skal fremkaldes går op.
            StartCoroutine(SpawnEnemyWave(m_enemiesToSpawn)); //starter siderutinen igen
        }


    }
    IEnumerator SpawnEnemyWave(int enemiesToSpawn) //Siderutinen sørger for at fremkalde tilfældige typer af fjender ved tilfældige fremkaldelsespunkter
    {
        m_spawningWave = true; //bool er true, så den ikke fremkalder flere bølger af gangen
        m_waveCounter.text = "Wave " + waveNumber; //her viser den bølgenummeret til spilleren
        Debug.Log("Wave " + waveNumber); //Lille debug med bølgenummer
        m_waveCounter.gameObject.SetActive(true);
        yield return new WaitForSeconds(timeBetweenWaves); //Starter tidstageren til mellem bølgerne
        m_waveCounter.gameObject.SetActive(false); //fjerner UI elementet igen
        for (int i = 0; i < enemiesToSpawn; i++) //for loop til fremkaldelse af fjender forgår i loopet
        {
            Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)]);// her fremkalder den en tilfældig fjende i et tilfældigt fremkaldelsespunkt
            Debug.Log("enemy spawned "); //Debugger at en fjende er fremkaldt
            yield return new WaitForSeconds(timeBetweenEnemySpawn); //Venter de bestemte antal sekunder, mellem fjenders fremkaldelse
        }
        m_spawningWave = false; //sætter bool'en til falsk, så der kan fremkaldes en ny bølge senere

    }
}
