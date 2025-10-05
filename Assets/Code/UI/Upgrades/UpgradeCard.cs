using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Upgrade Data")]
    [SerializeField] private string upgradeID; // Unique ID for saving/loading
    public string UpgradeID => upgradeID; // Read-only accessor

    [SerializeField] private Image m_cardImage;
    [SerializeField] private GameObject m_lock;
    [SerializeField] private UpgradeManager m_upgradeManager;
    [SerializeField] private Upgrade linkedUpgrade; // Drag in Inspector
    public Upgrade LinkedUpgrade => linkedUpgrade;    

    [Tooltip("Whether this upgrade is unlocked")]
    public bool isUnlocked = false;              

    private void Update()
    {
        UpdateCard();
    }


    public virtual bool CheckUpgradeUnlocked()
    {
        return isUnlocked;
    }

    public void UpdateCard()
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

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (isUnlocked)
        {
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
