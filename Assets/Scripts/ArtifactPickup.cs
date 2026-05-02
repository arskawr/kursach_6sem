using Mirror;
using UnityEngine;

public class ArtifactPickup : NetworkBehaviour
{
    public int artifactId = 0;
    public Sprite artifactIcon;

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

    [Server]
    public void CollectArtifact()
    {
        isCollected = true;
    }

    [Server]
    public void RespawnArtifact()
    {
        isCollected = false;
    }
}