using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button hostButton;
    public Button clientButton;
    public Button stopButton;
    public Button settingsButton;

    [Header("Panels")]
    public GameObject mainMenuPanel;   // панель с кнопками Start/Stop/Settings
    public GameObject settingsPanel;   // панель настроек

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager не найден в сцене!");
            return;
        }

        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    void OnHostClicked()
    {
        networkManager.StartHost();
    }

    void OnClientClicked()
    {
        networkManager.StartClient();
    }

    void OnStopClicked()
    {
        if (NetworkServer.active)
            networkManager.StopHost();
        else if (NetworkClient.isConnected)
            networkManager.StopClient();
    }

    void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Вызывается из скрипта SettingsMenu при нажатии «Назад»
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}