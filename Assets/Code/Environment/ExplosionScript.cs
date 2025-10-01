using UnityEngine;

public class VehicleExplosion : Explodable 
{
    [SerializeField] private GameObject m_explosion;
   public void Explodes()
    {
        m_explosion = Instantiate(m_explosion, transform.position, Quaternion.identity);
        Instantiate(m_explosion);
        Destroy(gameObject);
    }
}
