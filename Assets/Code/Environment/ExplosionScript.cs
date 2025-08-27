using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    [SerializeField] private GameObject m_explosionPrefab; // assign prefab-asset i Inspector
    [SerializeField] private bool m_destroySelfAfterExplode = false;

    public void Explode()
    {
        if (m_explosionPrefab != null)
        {
            Instantiate(m_explosionPrefab, transform.position, Quaternion.identity);
        }

        if (m_destroySelfAfterExplode)
            Destroy(gameObject);
    }
}
