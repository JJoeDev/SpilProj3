using UnityEngine;

public interface IDamageable
{
    /// <param name="amount">Mængde skade (kan være float eller heltal afrundet andre steder).</param>
    /// <param name="source">GameObject der forårsagede skaden (fx spiller eller fjende).</param>
    void TakeDamage(float amount, GameObject source);
}
