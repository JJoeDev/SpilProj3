using UnityEngine;

public class CannonUpgrade : Upgrade
{
    public GameObject cannonVisual; 

    public override void EnableUpgrade()
    {
        FrontCarCannon cannon = GetComponent<FrontCarCannon>();
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
        FrontCarCannon cannon = GetComponent<FrontCarCannon>();
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
