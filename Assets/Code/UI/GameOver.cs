using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject m_gameOverScreen;
    [SerializeField] private GameObject m_player;

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

        // (fjernet disable af BoooSound her, så lyden kan blive ved i game over)

        // sluk StadiumDrums i game over (så dens loop stoppes pænt)
        var drums = FindObjectOfType<StadiumDrums>();
        if (drums) drums.enabled = false;
    }

    public void RestartButton()
    {
        // sluk BoooSound før sceneskift (sikker oprydning)
        var boo = FindObjectOfType<BoooSound>();
        if (boo) boo.enabled = false;

        // sluk StadiumDrums før sceneskift (sikker oprydning)
        var drums = FindObjectOfType<StadiumDrums>();
        if (drums) drums.enabled = false;

        // Needs to be the our main scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart the current scene
    }

    public void MainMenuButton()
    {
        // sluk BoooSound før sceneskift (sikker oprydning)
        var boo = FindObjectOfType<BoooSound>();
        if (boo) boo.enabled = false;

        // sluk StadiumDrums før sceneskift (sikker oprydning)
        var drums = FindObjectOfType<StadiumDrums>();
        if (drums) drums.enabled = false;

        // Needs to be our main menu
        SceneManager.LoadScene("MainMenu");
    }
}
