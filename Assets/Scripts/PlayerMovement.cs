using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;

    [Header("Footstep Audio")]
    public AudioClip[] footstepClips;
    public float footstepInterval = 0.4f;

    // Компоненты
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource footstepSource;
    private PlayerStats playerStats;
    private Camera followCamera;

    // Внутренние переменные
    private Vector2 moveInput;
    private float footstepTimer;
    private float lastSentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        footstepSource = GetComponent<AudioSource>();
        playerStats = GetComponent<PlayerStats>();

        footstepSource.playOnAwake = false;
        footstepSource.loop = false;

        // Настройка Rigidbody2D для физического движения
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("=== ЛОКАЛЬНЫЙ ИГРОК ЗАСПАВНИЛСЯ ===");

        followCamera = Camera.main;
        if (followCamera != null)
        {
            followCamera.orthographicSize = 10f;
            followCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }

        
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Если мёртв — движение запрещено
        if (playerStats != null && playerStats.currentHealth <= 0)
        {
            if (anim != null)
                anim.SetFloat("Speed", 0f);
            return;
        }

        // Чтение ввода
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v).normalized;

        // Звуки шагов
        bool isMoving = moveInput.magnitude > 0.1f;
        HandleFootsteps(isMoving);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerStats != null && playerStats.currentHealth <= 0) return;

        // Вычисляем новую позицию с учётом физики
        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // Поворот спрайта в сторону движения
        if (moveInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
            CmdSetFacingDirection(scale.x);
        }

        // Локальное обновление аниматора
        float currentSpeed = moveInput.magnitude * moveSpeed;
        if (anim != null)
            anim.SetFloat("Speed", currentSpeed);

        // Отправка скорости на сервер для синхронизации с другими клиентами
        if (Mathf.Abs(currentSpeed - lastSentSpeed) > 0.1f)
        {
            CmdSetSpeed(currentSpeed);
            lastSentSpeed = currentSpeed;
        }

        // Камера следует за локальным игроком
        if (followCamera != null)
        {
            followCamera.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    [Command]
    void CmdSetFacingDirection(float dirX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(dirX) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    [Command]
    void CmdSetSpeed(float speed)
    {
        if (anim != null)
            anim.SetFloat("Speed", speed);
    }

    void HandleFootsteps(bool isMoving)
    {
        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    void PlayFootstepSound()
    {
        if (footstepSource != null && footstepSource.enabled && footstepClips != null && footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            footstepSource.PlayOneShot(clip);
        }
    }
}