using UnityEngine;

public class CannonUpgrade : Upgrade
{
    public GameObject cannonVisual; // optional, if you have a model/mesh for the cannon

    public override void EnableUpgrade()
    {
        CarCannon cannon = GetComponent<CarCannon>();
        if (cannon != null)
        {
            cannon.enabled = true; // turn on the cannon script
        }

        // Show cannon visually
        if (cannonVisual != null)
        {
            cannonVisual.SetActive(true);
        }
    }

    public override void DisableUpgrade()
    {
        CarCannon cannon = GetComponent<CarCannon>();
        if (cannon != null)
        {
            cannon.enabled = false; // turn off cannon script
        }

        if (cannonVisual != null)
        {
            cannonVisual.SetActive(false);
        }
    }
}
