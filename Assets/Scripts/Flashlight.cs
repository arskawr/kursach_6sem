using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Flashlight : NetworkBehaviour
{
    [Header("Battery")]
    public float maxBattery = 100f;
    [SyncVar(hook = nameof(OnBatteryChanged))]
    public float currentBattery;
    public float drainRate = 10f;

    [Header("Fear")]
    public float fearRadius = 8f;
    public LayerMask monsterLayer;

    [Header("Components")]
    public Light lightSource;             // Spot Light 2D
    public SpriteRenderer coneRenderer;   // спрайт луча (если есть)

    [Header("UI")]
    public Slider batterySlider;          // ссылка на слайдер в префабе

    [SyncVar(hook = nameof(OnFlashlightStateChanged))]
    public bool isOn = false;

    private void Awake()
    {
        SetVisualState(false);
    }

    public override void OnStartServer()
    {
        currentBattery = maxBattery;
        isOn = false;
        SetVisualState(false);
    }

    public override void OnStartClient()
    {
        SetVisualState(isOn);
        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.F))
                CmdToggleFlashlight(!isOn);
        }

        // Расход батареи только на сервере
        if (isServer)
        {
            if (isOn && currentBattery > 0)
            {
                currentBattery -= drainRate * Time.deltaTime;
                if (currentBattery <= 0)
                {
                    currentBattery = 0;
                    isOn = false;
                    SetVisualState(false);
                }
            }
        }
    }

    [Command]
    void CmdToggleFlashlight(bool state)
    {
        // Не даём включить фонарик, если батарея разряжена
        if (state && currentBattery <= 0)
            return;

        isOn = state;
        SetVisualState(isOn);
        if (isOn) FearMonsters();
    }

    [Server]
    void FearMonsters()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fearRadius, monsterLayer);
        foreach (var hit in hits)
        {
            MonsterAI ai = hit.GetComponent<MonsterAI>();
            if (ai != null) ai.TakeFlashlightDamage();
        }
    }

    void OnFlashlightStateChanged(bool oldState, bool newState)
    {
        SetVisualState(newState);
    }

    void OnBatteryChanged(float oldValue, float newValue)
    {
        if (batterySlider != null)
            batterySlider.value = newValue;
    }

    void SetVisualState(bool state)
    {
        if (lightSource != null) lightSource.enabled = state;
        if (coneRenderer != null) coneRenderer.enabled = state;
    }

    [Server]
    public void AddBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fearRadius);
    }
}