using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class DeathScreen : MonoBehaviour
{
    public Button menuButton;

    void Awake()
    {
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void Setup()
    {
        // Можно добавить анимацию появления
    }

    void ReturnToMenu()
    {
        // Останавливаем сетевые соединения
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        // Загружаем сцену меню (убедитесь, что сцена добавлена в Build Settings)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}