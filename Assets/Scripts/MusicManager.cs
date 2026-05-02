using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource audioSource;

    void Awake()
    {
        // Реализуем синглтон, который не уничтожается при смене сцен
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Установить громкость фоновой музыки (0..1)
    /// </summary>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Получить текущую громкость
    /// </summary>
    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
}