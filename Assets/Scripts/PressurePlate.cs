using Mirror;
using UnityEngine;

public class PressurePlate : NetworkBehaviour
{
    [Header("Visuals")]
    public Sprite activeSprite;
    public Sprite usedSprite;

    [SyncVar(hook = nameof(OnStateChanged))]
    public bool isActive = false;
    [SyncVar(hook = nameof(OnUsedChanged))]
    public bool isUsed = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (isUsed)
        {
            spriteRenderer.sprite = usedSprite;
            col.enabled = false;
        }
        else if (isActive)
        {
            spriteRenderer.sprite = activeSprite;
            col.enabled = true;
        }
        else
        {
            spriteRenderer.sprite = null;
            col.enabled = false;
        }
    }

    void OnStateChanged(bool oldVal, bool newVal) => UpdateVisuals();
    void OnUsedChanged(bool oldVal, bool newVal) => UpdateVisuals();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;
        if (!isActive || isUsed) return;
        if (!other.CompareTag("Player")) return;

        Boss boss = FindFirstObjectByType<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(1);
            Debug.Log($"Плита активирована! Урон боссу. Осталось HP: {boss.currentHealth}");
        }

        isUsed = true;
        isActive = false;
    }

    [Server]
    public void ActivatePlate()
    {
        if (!isUsed)
        {
            isActive = true;
            Debug.Log($"Плита {name} активирована.");
        }
    }
}