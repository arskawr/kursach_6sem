using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCarriedArtifactChanged))]
    public int carriedArtifactId = -1;

    [SyncVar(hook = nameof(OnHasKeyChanged))]
    public bool hasKey = false;

    public Image carriedItemIcon;
    public Image keyIcon;

    public Sprite[] artifactIcons;
    public Sprite keySprite;

    public override void OnStartLocalPlayer()
    {
        // Иконка артефакта
        GameObject iconObj = GameObject.Find("CarriedArtifactIcon");
        if (iconObj != null)
        {
            carriedItemIcon = iconObj.GetComponent<Image>();
            if (carriedItemIcon != null)
            {
                iconObj.SetActive(true);
                carriedItemIcon.enabled = true;
                carriedItemIcon.sprite = null;
                iconObj.SetActive(false);
                Debug.Log("PlayerInventory: CarriedArtifactIcon найден и готов.");
            }
        }
        else Debug.LogError("PlayerInventory: CarriedArtifactIcon не найден!");

        // Иконка ключа
        GameObject keyIconObj = GameObject.Find("KeyIcon");
        if (keyIconObj != null)
        {
            keyIcon = keyIconObj.GetComponent<Image>();
            if (keyIcon != null)
            {
                keyIcon.sprite = keySprite;
                keyIcon.gameObject.SetActive(false);
                Debug.Log("PlayerInventory: KeyIcon найден и готов.");
            }
        }
        else Debug.LogError("PlayerInventory: KeyIcon не найден в сцене!");
    }

    void OnCarriedArtifactChanged(int oldId, int newId)
    {
        if (!isLocalPlayer) return;
        Debug.Log($"PlayerInventory (local): артефакт изменён {oldId} -> {newId}");
        if (carriedItemIcon == null) return;
        if (newId == -1)
        {
            carriedItemIcon.sprite = null;
            carriedItemIcon.gameObject.SetActive(false);
        }
        else
        {
            if (artifactIcons != null && newId >= 0 && newId < artifactIcons.Length)
            {
                carriedItemIcon.sprite = artifactIcons[newId];
                carriedItemIcon.gameObject.SetActive(true);
            }
        }
    }

    void OnHasKeyChanged(bool oldVal, bool newVal)
    {
        if (!isLocalPlayer) return;
        Debug.Log($"PlayerInventory (local): ключ изменён {oldVal} -> {newVal}");
        if (keyIcon != null)
        {
            keyIcon.gameObject.SetActive(newVal);
            Debug.Log($"KeyIcon теперь активен: {newVal}");
        }
        else Debug.LogError("PlayerInventory: keyIcon == null!");
    }

    [Command]
    public void CmdPickupArtifact(NetworkIdentity artifactNetId)
    {
        if (carriedArtifactId != -1) return;
        ArtifactPickup artifact = artifactNetId?.GetComponent<ArtifactPickup>();
        if (artifact == null) return;
        if (Vector3.Distance(transform.position, artifact.transform.position) > 2f) return;
        carriedArtifactId = artifact.artifactId;
        artifact.CollectArtifact();
    }

    [Command]
    public void CmdPlaceArtifactOnAltar(NetworkIdentity altarNetId)
    {
        if (carriedArtifactId == -1) return;
        Altar altar = altarNetId?.GetComponent<Altar>();
        if (altar == null) return;
        if (Vector3.Distance(transform.position, altar.transform.position) > 2f) return;
        if (altar.PlaceArtifact(carriedArtifactId, connectionToClient))
        {
            carriedArtifactId = -1;
        }
    }

    [Command]
    public void CmdUseDoor(NetworkIdentity doorNetId)
    {
        Debug.Log($"[Inventory] CmdUseDoor вызван. hasKey={hasKey}");
        if (!hasKey) return;
        Door door = doorNetId?.GetComponent<Door>();
        if (door == null)
        {
            Debug.Log("[Inventory] Дверь не найдена");
            return;
        }
        if (Vector3.Distance(transform.position, door.transform.position) > 2f)
        {
            Debug.Log("[Inventory] Слишком далеко");
            return;
        }
        door.OpenDoor(connectionToClient);
    }
}