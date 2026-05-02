using System.Collections;
using Mirror;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openedSprite;

    [SyncVar(hook = nameof(OnDoorOpened))]
    private bool isOpened = false;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    void OnDoorOpened(bool oldVal, bool newVal)
    {
        spriteRenderer.sprite = newVal ? openedSprite : closedSprite;
    }

    [Server]
    public void OpenDoor(NetworkConnectionToClient conn)
    {
        if (isOpened) return;
        isOpened = true;
        Debug.Log($"Дверь открыта игроком {conn.identity.name}");

        StartCoroutine(GameCompleteCoroutine());
    }

    [Server]
    IEnumerator GameCompleteCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        RpcShowVictory();
        yield return new WaitForSeconds(5f);
        NetworkManager.singleton.StopHost();
    }

    [ClientRpc]
    void RpcShowVictory()
    {
        GameObject panelObj = GameObject.Find("VictoryPanel");
        if (panelObj != null)
        {
            VictoryUI victoryUI = panelObj.GetComponent<VictoryUI>();
            if (victoryUI != null)
                victoryUI.Show();
            else
                Debug.LogError("VictoryPanel найден, но компонент VictoryUI отсутствует");
        }
        else
            Debug.LogError("Не найден объект VictoryPanel в сцене!");
    }
}