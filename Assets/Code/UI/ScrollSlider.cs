using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollBar : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] float m_maxScroll;
    [SerializeField] float m_minScroll;
    [SerializeField] RectTransform m_slider;
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] UpgradeRoadMap m_upgradeRoadMap;

    InputManager m_inputManager;

    float m_scrollDistance;

    private void Start()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_inputManager = InputManager.Instance;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        MoveSliderAndRoadmap();
    }

    //Detect current clicks on the GameObject (the one with the script attached)
    public void OnDrag(PointerEventData pointerEventData)
    {
        MoveSliderAndRoadmap();
    }

    void MoveSliderAndRoadmap()
    {
        Vector2 mousePos = m_inputManager.OnMouseMove();

        Vector2 localMousePos = m_rectTransform.parent.InverseTransformPoint(mousePos);

        m_slider.anchoredPosition = new Vector2(Mathf.Clamp(localMousePos.x, m_minScroll, m_maxScroll), m_slider.anchoredPosition.y);

        m_scrollDistance = Mathf.InverseLerp(m_minScroll, m_maxScroll, m_slider.anchoredPosition.x);

        Debug.Log(m_scrollDistance);

        m_upgradeRoadMap.rectTransform.anchoredPosition = new Vector2(
            Mathf.Lerp(m_upgradeRoadMap.maxScrollDist, m_upgradeRoadMap.minScrollDist, m_scrollDistance),
            m_upgradeRoadMap.rectTransform.anchoredPosition.y
        );
    }
}
