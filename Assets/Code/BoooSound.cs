using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoooSound : MonoBehaviour
{
    public AudioSource sound;
    [SerializeField] private HealthManager healthManager;
    [SerializeField] public bool hasdied = false; // Flag to ensure sound plays only once
    [SerializeField] private GameObject player;
 
    private Coroutine monitorCo;

    private void Start()
    {
        player = GameObject.FindWithTag("Player"); // Finder spilleren via tag
        healthManager = player != null ? player.GetComponent<HealthManager>() : null;

        if (sound == null) sound = GetComponent<AudioSource>();
        if (sound != null) sound.loop = true;

        
        if (monitorCo == null)                                  
            monitorCo = StartCoroutine(MonitorDeathLoop());
    }
    private void OnEnable()
    {
        if (sound == null) sound = GetComponent<AudioSource>();
        if (sound != null) sound.loop = true;

        if (player == null)
        {
            player = GameObject.FindWithTag("Player"); // Finder spilleren via tag
            healthManager = player != null ? player.GetComponent<HealthManager>() : null;
        }
        else if (healthManager == null)
        {
            healthManager = player.GetComponent<HealthManager>();
        }

        if (monitorCo == null)                              
            monitorCo = StartCoroutine(MonitorDeathLoop());
    }

    void OnDisable()
    {
        if (monitorCo != null) { StopCoroutine(monitorCo); monitorCo = null; }
        if (sound != null) sound.Stop();

        hasdied = false;
    }

    private IEnumerator MonitorDeathLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() =>
            {
                if (player == null)
                {
                    var found = GameObject.FindWithTag("Player"); // Finder spilleren via tag
                    if (found != null)
                    {
                        player = found;
                        healthManager = player.GetComponent<HealthManager>();
                    }
                }
                return player == null || (healthManager != null && healthManager.currentHealth <= 0); // Check if health is zero or below
            });

            hasdied = true; // Sætter flaget til sandt, når spilleren dør
            if (hasdied)
            {
                PlayonDeath(); // Kalder metoden til at afspille lyden ved død
            }

            yield return new WaitUntil(() =>
            {
                if (player == null)
                {
                    var found = GameObject.FindWithTag("Player"); // Finder spilleren via tag
                    if (found != null)
                    {
                        player = found;
                        healthManager = player.GetComponent<HealthManager>();
                    }
                }
                return player != null && healthManager != null && healthManager.currentHealth > 0; // efter at have trykket restart knappen i GameOver.cs, så stopper lyden med at afspille
            });
            hasdied = false; // efter at have trykket restart knappen i GameOver.cs, så stopper lyden med at afspille
            if (sound != null) sound.Stop(); // Stopper lyden
        }
    }

    private void PlayonDeath()
    {
        if (sound == null) return;
        sound.loop = true; // Sætter lyden til at loope
        if (!sound.isPlaying) sound.Play(); // Afspiller lyden (kildens clip) i loop
    }
}
