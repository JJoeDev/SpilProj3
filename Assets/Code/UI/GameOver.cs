using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject m_gameOverScreen;
    [SerializeField] private GameObject m_player;
    [SerializeField] private BoooSound m_sound;
    void Start()
    {
        // Makes sure the screen starts hidden
        if (m_gameOverScreen != null)
            m_gameOverScreen.SetActive(false);
    }

    void Update()
    {
        // if the player object is destroyed the screen is displayed
        // We could change this to if the player has 0 health or something else
        if (m_player == null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            ShowGameOver();
        }
    }

    public void ShowGameOver()
    {
        // makes sure the game over screen is shown
        if (m_gameOverScreen != null)
            m_gameOverScreen.SetActive(true);

    }
    
    public void RestartButton()
    {
        m_sound.hasdied = false; // Reset the flag when restarting the game / så lyden stopper med at afspille
        // Needs to be the our main scene
        SceneManager.LoadScene("William1");
    }
    
    public void MainMenuButton()
    {
        // Needs to be our main menu
        SceneManager.LoadScene("MainMenu");
    }
}
