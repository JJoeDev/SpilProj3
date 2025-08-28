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
        // The screen starts hidden
        if (GameOverScreen != null)
            GameOverScreen.SetActive(false);
    }

    void Update()
    {
        // if the player object is destroyed the screen is displayed
        // We could change this to if the player has 0 health or something else
        if (Player == null)
        {
            ShowGameOver();
        }
    }

    public void ShowGameOver()
    {
        if (GameOverScreen != null)
            GameOverScreen.SetActive(true);

    }

    public void UpgradesButton()
    {
        SceneManager.LoadScene("Upgrades");
    }
    public void ContinueButton()
    {
        SceneManager.LoadScene("William");
    }
    public void MainMenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
