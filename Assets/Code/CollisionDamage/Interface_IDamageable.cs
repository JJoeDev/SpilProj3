using UnityEngine;

public interface IDamageable
{
    /// <param name="amount">M�ngde skade (kan v�re float eller heltal afrundet andre steder).</param>
    /// <param name="source">GameObject der for�rsagede skaden (fx spiller eller fjende).</param>
    void TakeDamage(float amount, GameObject source);
}
