using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RestartButton : MenuButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene("William1"); // or current scene
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
