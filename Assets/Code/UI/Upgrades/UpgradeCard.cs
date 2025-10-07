using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Upgrade Data")]
    [SerializeField] private string upgradeID; // Unique ID for saving/loading
    public string UpgradeID => upgradeID; // Read-only accessor

    [SerializeField] private Image m_cardImage;
    [SerializeField] private GameObject m_lock;
    [SerializeField] private GameObject m_soldText;
    [SerializeField] private Graphic[] m_upgradeRequirementTexts; // Stuff to reveal on mouse hover
    [SerializeField] private Graphic[] m_otherGraphics; // Stuff to unreveal on mouse hover
    [SerializeField] private UpgradeManager m_upgradeManager;
    [SerializeField] private Upgrade linkedUpgrade;

    public StatTracker statTracker;

    bool m_hovering;
    Coroutine m_hoverRoutine;

    public Upgrade LinkedUpgrade => linkedUpgrade;    

    [Tooltip("Whether this upgrade is unlocked")]
    public bool isUnlocked = false;


    private void Start()
    {
        statTracker = StatTracker.Instance;
    }

    public virtual void Update()
    {
        UpdateCard();
    }


    public virtual bool CheckUpgradeUnlocked()
    {
        return isUnlocked;
    }

    public void UpdateCard()
    {
        if (!m_hovering)
        {
            if (isUnlocked)
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
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (isUnlocked)
        {
            m_soldText.SetActive(true);
            if (LinkedUpgrade != null)
            {
                LinkedUpgrade.EnableUpgrade();
            }
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ieShakeCard(.2f));
        }
    }

    // Show upgrade requirements on hover if card isnt unlocked
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (isUnlocked) return;

        if (m_hoverRoutine != null) StopCoroutine(m_hoverRoutine);
        m_hoverRoutine = StartCoroutine(ieShowUpgradeRequirement(0.2f, true));
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (isUnlocked) return;

        if (m_hoverRoutine != null) StopCoroutine(m_hoverRoutine);
        m_hoverRoutine = StartCoroutine(ieShowUpgradeRequirement(0.2f, false));
    }


    IEnumerator ieShowUpgradeRequirement(float easeInTime, bool pointerEnter)
    {
        m_hovering = true;

        float elapsed = 0;

        while (elapsed < easeInTime)
        {
            elapsed += Time.deltaTime;

            float cardGraphicsEase = pointerEnter ? Mathf.Lerp(1f, 0f, elapsed / easeInTime) : Mathf.Lerp(0, 1f, elapsed / easeInTime);
            float upgradeRequirementEase  = pointerEnter ? Mathf.Lerp(0, 1f, elapsed / easeInTime) : Mathf.Lerp(1f, 0f, elapsed / easeInTime);

            float cardEase = pointerEnter ? Mathf.Lerp(0.5f, 0f, elapsed / easeInTime) : Mathf.Lerp(0, 0.5f, elapsed / easeInTime);

            m_cardImage.color = new Color(
                cardEase,
                cardEase,
                cardEase
            );

            foreach (var graphic in m_otherGraphics)
            {
                graphic.color = new Color(
                    cardGraphicsEase,
                    cardGraphicsEase,
                    cardGraphicsEase,
                    cardGraphicsEase
                );
            }

            foreach (var txt in m_upgradeRequirementTexts)
            {
                txt.color = new Color(
                    upgradeRequirementEase,
                    upgradeRequirementEase,
                    upgradeRequirementEase,
                    upgradeRequirementEase
                );
            }

            yield return null;
        }

        // Ensure that the colors are set to the correct final values

        foreach (var graphic in m_otherGraphics)
        {
            graphic.color = new Color(
                1f,
                1f,
                1f,
                pointerEnter ? 0f : 1f
            );
        }

        foreach (var txt in m_upgradeRequirementTexts)
        {
            txt.color = new Color(
                1f,
                1f,
                1f,
                pointerEnter ? 1f : 0f
            );
        }

       if (!pointerEnter) m_hovering = false;
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
}
