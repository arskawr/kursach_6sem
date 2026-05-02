using Mirror;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    private GameObject currentInteractable;

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[Interaction] E нажата. currentInteractable={currentInteractable?.name}");
            if (currentInteractable != null)
            {
                NetworkIdentity netId = currentInteractable.GetComponent<NetworkIdentity>();
                if (netId == null) return;

                // Артефакт
                ArtifactPickup artifact = currentInteractable.GetComponent<ArtifactPickup>();
                if (artifact != null)
                {
                    Debug.Log("[Interaction] Попытка подобрать артефакт");
                    GetComponent<PlayerInventory>().CmdPickupArtifact(netId);
                    return;
                }

                // Алтарь
                Altar altar = currentInteractable.GetComponent<Altar>();
                if (altar != null)
                {
                    Debug.Log("[Interaction] Попытка разместить артефакт на алтаре");
                    GetComponent<PlayerInventory>().CmdPlaceArtifactOnAltar(netId);
                    return;
                }

                // Ключ (игнорируется, подбирается автоматически)
                KeyPickup key = currentInteractable.GetComponent<KeyPickup>();
                if (key != null)
                {
                    Debug.Log("[Interaction] Ключ (автоподбор)");
                    return;
                }

                // Дверь
                Door door = currentInteractable.GetComponent<Door>();
                if (door != null)
                {
                    Debug.Log("[Interaction] Вызов CmdUseDoor");
                    GetComponent<PlayerInventory>().CmdUseDoor(netId);
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLocalPlayer) return;
        Debug.Log($"[Interaction] Триггер вошёл в {other.name}, tag={other.tag}");
        if (other.CompareTag("Artifact") || other.CompareTag("Altar") ||
            other.CompareTag("Key") || other.CompareTag("Door"))
        {
            currentInteractable = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isLocalPlayer) return;
        if (other.gameObject == currentInteractable)
            currentInteractable = null;
    }
}