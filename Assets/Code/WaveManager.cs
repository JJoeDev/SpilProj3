    using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    
    public GameObject[] enemyPrefab; //Array med fjendetyper
    private int m_enemyCount; //antal fjender tilbage
    public int waveNumber = 1; //b�lgenummer

    public float timeBetweenEnemySpawn; //tidsinterval mellem fjenders fremkaldelse
    public float timeBetweenWaves;  //tidsinterval mellem b�lger

    public Transform[] spawnPoints; // array med fremkaldelsespunkter

    private bool m_spawningWave; //bool, der holder styr p�, om en b�lge er ved at blive fremkaldt
     private int m_enemiesToSpawn; //antallet af fjender der skal fremkaldes
    [SerializeField] private TextMeshProUGUI m_waveCounter; //UI element, der visualiserer b�lgenummeret

    void Awake() // f�rste b�lge starter, n�r scenen v�gner
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
            waveNumber++; //b�lgenummeret g�r op
            m_enemiesToSpawn++;  //antallet af fjender der skal fremkaldes g�r op.
            StartCoroutine(SpawnEnemyWave(m_enemiesToSpawn)); //starter siderutinen igen
        }


    }
    IEnumerator SpawnEnemyWave(int enemiesToSpawn) //Siderutinen s�rger for at fremkalde tilf�ldige typer af fjender ved tilf�ldige fremkaldelsespunkter
    {
        m_spawningWave = true; //bool er true, s� den ikke fremkalder flere b�lger af gangen
        m_waveCounter.text = "Wave " + waveNumber; //her viser den b�lgenummeret til spilleren
        Debug.Log("Wave " + waveNumber); //Lille debug med b�lgenummer
        m_waveCounter.gameObject.SetActive(true);
        yield return new WaitForSeconds(timeBetweenWaves); //Starter tidstageren til mellem b�lgerne
        m_waveCounter.gameObject.SetActive(false); //fjerner UI elementet igen
        for (int i = 0; i < enemiesToSpawn; i++) //for loop til fremkaldelse af fjender forg�r i loopet
        {
            Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], spawnPoints[Random.Range(0, spawnPoints.Length)]);// her fremkalder den en tilf�ldig fjende i et tilf�ldigt fremkaldelsespunkt
            Debug.Log("enemy spawned "); //Debugger at en fjende er fremkaldt
            yield return new WaitForSeconds(timeBetweenEnemySpawn); //Venter de bestemte antal sekunder, mellem fjenders fremkaldelse
        }
        m_spawningWave = false; //s�tter bool'en til falsk, s� der kan fremkaldes en ny b�lge senere

    }
}
