using Mirror;
using UnityEngine;

public class BatteryPickup : NetworkBehaviour
{
    public float batteryAmount = 30f;   // Сколько батареи восстанавливает
    public float rotationSpeed = 100f;  // Вращение для красоты

    void Update()
    {
        // Вращение (можно только на клиенте)
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;   // Только сервер обрабатывает подбор
        if (!other.CompareTag("Player")) return;

        Flashlight flashlight = other.GetComponentInChildren<Flashlight>(); // ищем фонарик у игрока
        if (flashlight != null)
        {
            flashlight.AddBattery(batteryAmount);
            NetworkServer.Destroy(gameObject);   // Удаляем предмет со всех клиентов
        }
    }
}