using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle fullscreenToggle;
    public Slider volumeSlider;
    public Button backButton;

    [Header("References")]
    public MainMenuUI mainMenuUI;   // ссылка на главный скрипт меню (для вызова CloseSettings)

    private const string FullscreenKey = "Fullscreen";
    private const string VolumeKey = "MusicVolume";

    void Start()
    {
        // Загрузка сохранённых настроек
        if (PlayerPrefs.HasKey(FullscreenKey))
            Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;

        float savedVolume = 1f;
        if (PlayerPrefs.HasKey(VolumeKey))
            savedVolume = PlayerPrefs.GetFloat(VolumeKey);

        // Применяем громкость к MusicManager, если он есть
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(savedVolume);
            volumeSlider.value = savedVolume;
        }
        else
        {
            volumeSlider.value = savedVolume;
            Debug.LogWarning("MusicManager не найден в сцене. Создайте объект MusicManager.");
        }

        // Установка UI в текущее состояние
        fullscreenToggle.isOn = Screen.fullScreen;

        // Подписка на события
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        backButton.onClick.AddListener(Close);
    }

    void OnFullscreenChanged(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);

        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    void Close()
    {
        if (mainMenuUI != null)
            mainMenuUI.CloseSettings();
        else
            Debug.LogWarning("MainMenuUI не назначен в SettingsMenu");
    }
}