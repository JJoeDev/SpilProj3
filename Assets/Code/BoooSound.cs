using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoooSound : MonoBehaviour
{
    public AudioSource sound;
    [SerializeField] private HealthManager healthManager;
    [SerializeField] public bool hasdied = false; // Flag to ensure sound plays only once
    [SerializeField] private GameObject player;
    // Start is called before the first frame update

    // Update is called once per frame
    private void Start()
    {
        player = GameObject.FindWithTag("Player"); // Finder spilleren via tag
        healthManager = player != null ? player.GetComponent<HealthManager>() : null; // Henter HealthManager scriptet fra spilleren

        // Kør samme logik som i Update, men uden Update – via coroutine
        StartCoroutine(MonitorDeathLoop());
    }

    // (tom – ingen per-frame logik)
    // void Update() {}

    private IEnumerator MonitorDeathLoop()
    {
        // Vent til “død” (health <= 0 eller player er null)
        yield return new WaitUntil(() =>
        {
            // prøv at (gen)finde player/healthManager hvis de er null
            if (player == null)
            {
                var found = GameObject.FindWithTag("Player"); // Finder spilleren via tag
                if (found != null)
                {
                    player = found;
                    healthManager = player.GetComponent<HealthManager>(); // Henter HealthManager scriptet fra spilleren
                }
            }
            return player == null || (healthManager != null && healthManager.currentHealth <= 0); // Check if health is zero or below
        });

        hasdied = true; // Sætter flaget til sandt, når spilleren dør
        if (hasdied)
        {
            PlayonDeath(); // Kalder metoden til at afspille lyden ved død
        }

        // Vent til “levende igen” (player findes og health > 0)
        yield return new WaitUntil(() =>
        {
            // prøv at (gen)finde player/healthManager hvis de er null
            if (player == null)
            {
                var found = GameObject.FindWithTag("Player"); // Finder spilleren via tag
                if (found != null)
                {
                    player = found;
                    healthManager = player.GetComponent<HealthManager>(); // Henter HealthManager scriptet fra spilleren
                }
            }
            return player != null && healthManager != null && healthManager.currentHealth > 0; // efter at have trykket restart knappen i GameOver.cs, så stopper lyden med at afspille
        });

        // “genstartet/levende” → stop lyden
        hasdied = false; // efter at have trykket restart knappen i GameOver.cs, så stopper lyden med at afspille
        sound.Stop(); // Stopper lyden
        OnDisable();  // Kalder OnDisable metoden for at stoppe lyden

        // Fortsæt loopen igen (vent på næste død)
        StartCoroutine(MonitorDeathLoop());
    }

    private void PlayonDeath()
    {
        sound.PlayOneShot(sound.clip); // Afspiller lyden en gang
        sound.loop = true; // Sætter lyden til at loope  
    }

    private void OnDisable()
    {
        sound.Stop(); // Stopper lyden, hvis objektet deaktiveres
    }
}
