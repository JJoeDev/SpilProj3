using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ToMainMenu : MenuButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene("MainMenu"); // or current scene
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