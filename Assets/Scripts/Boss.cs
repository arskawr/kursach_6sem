using Mirror;
using UnityEngine;
using System.Collections;

public class Boss : NetworkBehaviour
{
    [Header("Stats")]
    public int maxHealth = 6;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Attack")]
    public int attackDamage = 15;
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    private float nextAttackTime;

    [Header("Key")]
    public GameObject keyPrefab;
    // Точка спавна ключа (устанавливается алтарём при создании босса)
    [HideInInspector] public Transform keySpawnPoint;

    [Header("Visual Feedback")]
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.2f;
    private Color originalColor;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public Animator anim;

    private Transform targetPlayer;
    private float targetUpdateTimer = 0f;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (anim == null) anim = GetComponent<Animator>();
        originalColor = spriteRenderer.color;
    }

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        nextAttackTime = Time.time + 1f;
    }

    void Update()
    {
        if (!isServer) return;

        targetUpdateTimer -= Time.deltaTime;
        if (targetUpdateTimer <= 0f)
        {
            FindNearestPlayer();
            targetUpdateTimer = 0.3f;
        }

        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist <= attackRange && Time.time >= nextAttackTime)
            {
                AttackPlayer(targetPlayer);
                nextAttackTime = Time.time + attackCooldown;
            }

            if (anim != null) anim.SetBool("IsMoving", true);
        }
        else
        {
            if (anim != null) anim.SetBool("IsMoving", false);
        }
    }

    [Server]
    void FindNearestPlayer()
    {
        float closest = float.MaxValue;
        Transform nearest = null;
        foreach (NetworkIdentity netId in NetworkServer.spawned.Values)
        {
            if (netId == null) continue;
            PlayerStats stats = netId.GetComponent<PlayerStats>();
            if (stats != null && stats.currentHealth > 0)
            {
                float dist = Vector3.Distance(transform.position, netId.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    nearest = netId.transform;
                }
            }
        }
        targetPlayer = nearest;
    }

    [Server]
    void AttackPlayer(Transform player)
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(attackDamage);
            Debug.Log($"Босс атакует {player.name} на {attackDamage} урона");
        }
    }

    [Server]
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
    }

    void OnHealthChanged(int oldVal, int newVal)
    {
        if (newVal < oldVal)
        {
            StartCoroutine(FlashDamage());
            if (anim != null) anim.SetTrigger("Hit");
        }
        if (newVal <= 0 && isServer)
        {
            Die();
        }
    }

    IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    [Server]
    void Die()
    {
        if (anim != null) anim.SetBool("IsDead", true);

        // Спавним ключ в точке keySpawnPoint. Если она не задана, то на месте босса.
        Vector3 spawnPos = (keySpawnPoint != null) ? keySpawnPoint.position : transform.position;
        if (keyPrefab != null)
        {
            GameObject key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(key);
        }

        StartCoroutine(DeathCoroutine());
    }

    [Server]
    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(2f);
        NetworkServer.Destroy(gameObject);
    }
}