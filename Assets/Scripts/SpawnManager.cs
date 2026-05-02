using Mirror;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("Префабы для спавна")]
    public GameObject monsterPrefab;      // Монстр
    public GameObject collectiblePrefab;  // Предметы (батарейки и т.д.) — можно добавить позже

    void Update()
    {
        // Только сервер может спавнить объекты
        if (!isServer) return;

        // Спавн монстра по клавише M
        if (Input.GetKeyDown(KeyCode.M))
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-25f, 25f),
                Random.Range(-25f, 25f),
                0f);

            GameObject monster = Instantiate(monsterPrefab, randomPos, Quaternion.identity);
            NetworkServer.Spawn(monster);

            Debug.Log($"[SERVER] Монстр заспавнен на позиции {randomPos}");
        }

        // Спавн collectible по клавише P (для ЛР №3)
        if (Input.GetKeyDown(KeyCode.P) && collectiblePrefab != null)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-20f, 20f),
                Random.Range(-20f, 20f),
                0f);

            GameObject item = Instantiate(collectiblePrefab, randomPos, Quaternion.identity);
            NetworkServer.Spawn(item);

            Debug.Log($"[SERVER] Предмет заспавнен на позиции {randomPos}");
        }
    }
}