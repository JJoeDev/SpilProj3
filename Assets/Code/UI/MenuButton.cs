using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float hoverSizeIncreasePercentage = 20; 
    [SerializeField] float easingTime = 0.15f;


    TMP_Text m_buttonText;
    float m_startFontSize;
    public void Awake()
    {
        m_buttonText = GetComponent<TMP_Text>();
        m_startFontSize = m_buttonText.fontSize;
    }

    public virtual void OnPointerClick(PointerEventData pointerEventData)
    {
        //Some basic button functionality
    }

    public virtual void OnPointerEnter(PointerEventData pointerEventData)
    {
        StartCoroutine(ieSmoothHover(easingTime));
    }

    public virtual void OnPointerExit(PointerEventData pointerEventData)
    {
        StartCoroutine(ieSmoothExit(easingTime));
    }

    IEnumerator ieSmoothHover(float smoothTime)
    {
        float elapsed = 0;
        float t;
        while (elapsed < smoothTime)
        {
            elapsed += Time.deltaTime;
            t = elapsed / smoothTime;
            m_buttonText.fontSize = Mathf.Lerp(
                m_startFontSize, m_startFontSize * (1 + hoverSizeIncreasePercentage * 0.01f), 1 - Mathf.Pow(1 - t, 4)
            );
            yield return null;
        }
    }
    IEnumerator ieSmoothExit(float smoothTime)
    {
        float elapsed = smoothTime;
        float t;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            t = elapsed / smoothTime;
            m_buttonText.fontSize = Mathf.Lerp(
                m_startFontSize, m_startFontSize * (1 + hoverSizeIncreasePercentage * 0.01f), 1 - Mathf.Pow(1 - t, 4)
            );
            yield return null;
        }
    }
}
