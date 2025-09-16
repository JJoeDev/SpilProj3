using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MenuButton
{
    [SerializeField] Image m_cardImage;
    [SerializeField] UpgradeManager m_upgradeManager;
    [SerializeField] GameObject m_lock;
    public bool enabled = false;

    // Update is called once per frame
    void Update()
    {
        UpdateCard();
    }

    private void Awake()
    {
        UpdateCard();
    }

    public void UpdateCard()
    {
        if (enabled)
        {
            m_cardImage.color = Color.white;
            m_lock.SetActive(false);
        }
        else
        {
            m_cardImage.color = new Color(0.5f, 0.5f, 0.5f);
            m_lock.SetActive(true);
        }
    }

    public override void OnPointerClick(PointerEventData pointerEventData)
    {
        if (enabled)
        {
            m_upgradeManager.upgrades[m_upgradeManager.upgradeCount - 1].EnableUpgrade();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ieShakeCard(.2f));
        }
    }


    IEnumerator ieShakeCard(float animTime)
    {
        float elapsed = 0;

        while (elapsed < animTime)
        {
            elapsed += Time.deltaTime;

            transform.rotation = Quaternion.Euler(
                transform.rotation.x,
                transform.rotation.y,
                -10f
            );

            yield return null;
        }

        transform.rotation = Quaternion.identity;
    }
    public override void OnPointerEnter(PointerEventData pointerEventData)
    {
        
    }

    public override void OnPointerExit(PointerEventData pointerEventData)
    {
        
    }
}
