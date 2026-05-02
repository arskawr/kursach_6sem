using Mirror;
using UnityEngine;

public class Altar : NetworkBehaviour
{
    [Header("Slots sprites (4 дочерних SpriteRenderer)")]
    public SpriteRenderer[] slotRenderers;

    [Header("Спрайты артефактов (0..3)")]
    public Sprite[] artifactSprites;

    [Header("Boss Spawn")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;   // точка, где появится босс (и ключ после его смерти)

    [SyncVar(hook = nameof(OnSlotChanged))]
    public int slot0 = -1;
    [SyncVar(hook = nameof(OnSlotChanged))]
    public int slot1 = -1;
    [SyncVar(hook = nameof(OnSlotChanged))]
    public int slot2 = -1;
    [SyncVar(hook = nameof(OnSlotChanged))]
    public int slot3 = -1;

    private bool bossSpawned = false;

    void Start()
    {
        if (slotRenderers.Length != 4)
            Debug.LogError("Altar: нужно ровно 4 слота!");
    }

    void OnSlotChanged(int oldValue, int newValue)
    {
        UpdateAllSlotsVisual();
    }

    void UpdateAllSlotsVisual()
    {
        int[] slots = { slot0, slot1, slot2, slot3 };
        for (int i = 0; i < slots.Length; i++)
        {
            if (slotRenderers[i] != null)
            {
                if (slots[i] >= 0 && slots[i] < artifactSprites.Length)
                {
                    slotRenderers[i].sprite = artifactSprites[slots[i]];
                    slotRenderers[i].color = Color.white;
                }
                else
                {
                    slotRenderers[i].sprite = null;
                    slotRenderers[i].color = Color.clear;
                }
            }
        }
    }

    public bool PlaceArtifact(int artifactId, NetworkConnectionToClient conn)
    {
        if (slot0 == -1) { slot0 = artifactId; return true; }
        if (slot1 == -1) { slot1 = artifactId; return true; }
        if (slot2 == -1) { slot2 = artifactId; return true; }
        if (slot3 == -1) { slot3 = artifactId; return true; }
        return false;
    }

    void Update()
    {
        if (!isServer) return;
        if (slot0 != -1 && slot1 != -1 && slot2 != -1 && slot3 != -1 && !bossSpawned)
        {
            bossSpawned = true;
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        // Делаем слоты невидимыми
        foreach (var sr in slotRenderers)
            if (sr != null) sr.gameObject.SetActive(false);

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            NetworkServer.Spawn(boss);

            // Передаём боссу точку для выпадения ключа – тот же bossSpawnPoint
            Boss bossComp = boss.GetComponent<Boss>();
            if (bossComp != null)
                bossComp.keySpawnPoint = bossSpawnPoint;

            Debug.Log("Босс заспавнен!");

            // Активируем плиты
            foreach (PressurePlate plate in FindObjectsByType<PressurePlate>(FindObjectsSortMode.None))
            {
                plate.ActivatePlate();
            }
        }
        else
            Debug.LogError("Не назначен префаб босса или точка спавна на алтаре!");
    }
}