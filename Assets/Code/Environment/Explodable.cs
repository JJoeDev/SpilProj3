using UnityEngine;

public abstract class Explodable : MonoBehaviour
{
    public virtual void Explode() 
    {

    }

    /// <summary>
    /// This method is used for physics-explodables
    /// </summary>
    /// <param name="exploderPos"></param>
    /// <param name="explosionForce"></param>
    public virtual void Explode(Vector3 exploderPos, float explosionForce)
    {

    }
}
