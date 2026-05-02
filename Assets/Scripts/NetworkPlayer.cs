using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float sprintMultiplier = 1.6f;

    [SyncVar] private Vector2 serverPosition;
    [SyncVar] private float serverRotation;

    private Rigidbody2D rb;
    private Vector2 inputVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        ReadInput();
        // Отправляем ввод на сервер
        CmdSendInput(inputVector, Input.GetKey(KeyCode.LeftShift));
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            // Сервер обновляет физику
            Vector2 velocity = serverPosition; // placeholder
        }
        else
        {
            // Клиентская интерполяция позиции (если используем NetworkTransform, можно не делать)
            transform.position = Vector2.Lerp(transform.position, serverPosition, 0.1f);
        }
    }

    void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        inputVector = new Vector2(x, y).normalized;
    }

    [Command]
    void CmdSendInput(Vector2 input, bool sprint)
    {
        // Сервер применяет движение
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        Vector2 newPos = (Vector2)transform.position + input * speed * Time.fixedDeltaTime;
        rb.position = newPos;
        // Обновляем SyncVar для клиентов
        serverPosition = newPos;
    }

    // Опционально: RPC для проигрывания звуков/эффектов
    [ClientRpc]
    public void RpcPlayFootstep()
    {
        // проиграть звук на клиентах
    }
}
