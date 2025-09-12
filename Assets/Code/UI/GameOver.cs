using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverScreen;
    public GameObject Player;

    void Start()
    {
        // Makes sure the screen starts hidden
        if (GameOverScreen != null)
            GameOverScreen.SetActive(false);
    }

    void Update()
    {
        // if the player object is destroyed the screen is displayed
        // We could change this to if the player has 0 health or something else
        if (Player == null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ShowGameOver();
        }
    }

    public void ShowGameOver()
    {
        // makes sure the game over screen is shown
        if (GameOverScreen != null)
            GameOverScreen.SetActive(true);

    }
    public void RestartButton()
    {
        // Needs to be the our main scene
        SceneManager.LoadScene("William1");
    }
    public void MainMenuButton()
    {
        // Needs to be our main menu
        SceneManager.LoadScene("MainMenu");
    }
}
