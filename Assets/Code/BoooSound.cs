using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoooSound : MonoBehaviour
{
    public AudioSource sound; 
    [SerializeField] private HealthManager healthManager;
    [SerializeField] public bool hasdied = false; // Flag to ensure sound plays only once
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (healthManager != null) // Sikkerhed for at healthManager ikke er null, ikke nødvendigt hvis det er sat i inspector.
        {
            return; 
        }
        if (healthManager.currentHealth <= 0) // Check if health is zero or below
        {
            hasdied = true; // Set the flag to true
            if(hasdied)
            {
                sound.Play(); 
                sound.loop = true; // Sætter lyden til at loope
            }
        }
        if(hasdied = false && healthManager.currentHealth > 0) // efter at have trykket restart knappen i GameOver.cs, så stopper lyden med at afspille
        {
           sound.Stop(); 
        }
    }
}
