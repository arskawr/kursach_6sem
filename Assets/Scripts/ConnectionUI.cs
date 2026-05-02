using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [Header("Кнопки")]
    public Button hostButton;
    public Button clientButton;
    public Button stopButton;

    private NetworkManager networkManager;

    void Awake()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
            Debug.LogError("NetworkManager не найден!");
    }

    void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        stopButton.onClick.AddListener(OnStopClicked);
    }

    private void OnHostClicked()
    {
        networkManager.StartHost();
        gameObject.SetActive(false);        // скрываем UI
    }

    private void OnClientClicked()
    {
        networkManager.StartClient();
        gameObject.SetActive(false);        // скрываем UI
    }

    private void OnStopClicked()
    {
        networkManager.StopHost();
        networkManager.StopClient();
        gameObject.SetActive(true);         // показываем UI обратно
    }
}