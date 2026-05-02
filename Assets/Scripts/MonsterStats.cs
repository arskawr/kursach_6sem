using Mirror;
using UnityEngine;

public class MonsterStats : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth = 50;
    public int maxHealth = 50;

    private void OnHealthChanged(int oldVal, int newVal)
    {
        if (currentHealth <= 0)
        {
            if (isServer)
                NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
    }
}