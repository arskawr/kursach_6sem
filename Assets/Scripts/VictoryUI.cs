using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class VictoryUI : MonoBehaviour
{
    public Button menuButton;
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Скрываем панель, но НЕ деактивируем объект
        Hide();

        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void ReturnToMenu()
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}