using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterAI : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float visionRange = 15f;
    public float teleportDistance = 20f;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;

    [Header("Fear")]
    public float fearRadius = 8f;          // радиус, в котором монстр боится включённого фонарика

    [Header("Target Update")]
    public float updateTargetInterval = 0.5f;

    private Transform targetPlayer;
    private Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;
    private float nextTargetUpdate;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        if (!isServer) return;

        // Периодический поиск ближайшего игрока
        if (Time.time >= nextTargetUpdate)
        {
            FindNearestPlayer();
            nextTargetUpdate = Time.time + updateTargetInterval;
        }

        // Проверяем, не боится ли монстр включённого фонарика
        if (IsAfraid())
        {
            rb.linearVelocity = Vector2.zero;
            SetAnimatorMoving(false);
            return;
        }

        // Обычное преследование
        if (targetPlayer != null && CanSeePlayer())
        {
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            // Поворот спрайта по направлению движения
            if (rb.linearVelocity.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            SetAnimatorMoving(true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            SetAnimatorMoving(false);
        }
    }

    void SetAnimatorMoving(bool moving)
    {
        if (anim != null && HasParameter("IsMoving"))
            anim.SetBool("IsMoving", moving);
    }

    bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
            if (param.name == paramName) return true;
        return false;
    }

    bool IsAfraid()
    {
        foreach (NetworkIdentity netId in NetworkServer.spawned.Values)
        {
            if (netId == null) continue;
            Flashlight flashlight = netId.GetComponentInChildren<Flashlight>();
            if (flashlight != null && flashlight.isOn)
            {
                float dist = Vector2.Distance(transform.position, netId.transform.position);
                if (dist <= fearRadius)
                    return true;
            }
        }
        return false;
    }

    private Collider2D targetPlayerCollider;   // новое поле

    void FindNearestPlayer()
    {
        float closest = float.MaxValue;
        Transform newTarget = null;
        Collider2D newCollider = null;
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
                    newTarget = netId.transform;
                    newCollider = netId.GetComponent<Collider2D>();   // получаем коллайдер
                }
            }
        }
        targetPlayer = newTarget;
        targetPlayerCollider = newCollider;
    }

    bool CanSeePlayer()
    {
        if (targetPlayer == null || targetPlayerCollider == null) return false;

        // Цель – центр коллайдера игрока (а не ноги)
        Vector2 targetPoint = targetPlayerCollider.bounds.center;
        float dist = Vector2.Distance(transform.position, targetPoint);
        if (dist > visionRange) return false;

        Vector2 direction = (targetPoint - (Vector2)transform.position).normalized;
        int layerMask = LayerMask.GetMask("Player");   // только слой Player

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dist, layerMask);

        // Отладка (можно удалить после проверки)
        Debug.DrawLine(transform.position, targetPoint,
            hit.collider != null && hit.collider.CompareTag("Player") ? Color.green : Color.red, 0.5f);

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isServer) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (IsAfraid()) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null && stats.currentHealth > 0)
            {
                stats.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
    }

    [Server]
    public void TakeFlashlightDamage()
    {
        if (targetPlayer != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * teleportDistance;
            transform.position = targetPlayer.position + (Vector3)randomDir;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fearRadius);
    }
}