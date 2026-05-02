using Mirror;
using UnityEngine;

public class KeyPickup : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCollectedChanged))]
    private bool isCollected = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnCollectedChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            spriteRenderer.enabled = false;
            col.enabled = false;
        }
        else
        {
            spriteRenderer.enabled = true;
            col.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory != null && !inventory.hasKey)
        {
            isCollected = true;
            inventory.hasKey = true;
            Debug.Log($"{other.name} подобрал ключ.");
            NetworkServer.Destroy(gameObject);
        }
    }
}