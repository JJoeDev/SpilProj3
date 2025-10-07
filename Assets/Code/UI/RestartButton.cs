using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RestartButton : MenuButton
{
    [SerializeField] private BoooSound m_sound; // Reference to the BoooSound script
    public override void OnPointerClick(PointerEventData eventData)
    {
        m_sound.hasdied = false; // Reset the flag when restarting the game / så lyden stopper med at afspille
        SceneManager.LoadScene("Patrick-1"); // or current scene
    }

    public override void OnPointerEnter(PointerEventData pointerEventData)
    {
        base.OnPointerEnter(pointerEventData);
    }

    public override void OnPointerExit(PointerEventData pointerEventData)
    {
        base.OnPointerExit(pointerEventData);
    }
}
