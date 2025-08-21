using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoToButton : MenuButton
{
    [SerializeField] GameObject m_currentMenuObject; // The object to disable
    [SerializeField] GameObject m_goToObject;        // The object to enable

    public override void OnPointerClick(PointerEventData pointerEventData)
    {
        m_goToObject.SetActive(true);
        m_currentMenuObject.SetActive(false);
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
