using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public float targetHalfHeight = 5f;   // сколько юнитов должно быть видно по вертикали (половина высоты)
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSize();
    }

    void Update()
    {
        // Обновляем при изменении размера окна
        if (cam.aspect != lastAspect)
        {
            UpdateCameraSize();
        }
    }

    private float lastAspect;

    void UpdateCameraSize()
    {
        float currentAspect = cam.aspect;
        // Зная желаемую высоту, вычисляем orthographicSize = targetHalfHeight
        cam.orthographicSize = targetHalfHeight;
        lastAspect = currentAspect;
    }
}