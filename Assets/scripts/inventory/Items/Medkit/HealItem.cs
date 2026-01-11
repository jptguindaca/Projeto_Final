using UnityEngine;

public class HealItem : MonoBehaviour, IUsable
{
    public int healAmount = 25;

    public void Use(GameObject player)
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.AddHealth(healAmount);
            Debug.Log("Healed for " + healAmount + " health.");
        }
    }
}
