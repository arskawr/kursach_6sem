using Mirror;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    public int attackDamage = 20;
    public float attackRange = 2f;
    public LayerMask targetLayers; // В инспекторе выберите слои Player и Monster

    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
            CmdAttack();
    }

    [Command]
    void CmdAttack()
    {
        Vector2 direction = transform.right; // или в сторону мыши — по желанию
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, attackRange, targetLayers);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<PlayerStats>(out var playerStats))
                playerStats.TakeDamage(attackDamage);
            else if (hit.collider.TryGetComponent<MonsterStats>(out var monsterStats))
                monsterStats.TakeDamage(attackDamage);

            Debug.Log($"[SERVER] Атака попала в {hit.collider.name}");
        }
    }
}