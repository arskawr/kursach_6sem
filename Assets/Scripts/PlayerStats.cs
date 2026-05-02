using Mirror;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerStats : NetworkBehaviour
{
    [Header("Health")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int currentHealth = 100;
    public int maxHealth = 100;

    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Death")]
    public GameObject deathScreenPrefab;

    private Animator anim;
    private AudioSource audioSource;
    private bool isDead = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        // UI здоровья
        var healthUI = GetComponentInChildren<HealthBarUI>();
        if (healthUI != null) healthUI.UpdateHealth();

        if (newValue < oldValue && newValue > 0)
        {
            // Hurt
            if (anim != null) anim.SetTrigger("Hurt");
            if (hurtSound != null) audioSource.PlayOneShot(hurtSound);
        }

        if (newValue <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(HandleDeath());
        }
    }

    private IEnumerator HandleDeath()
    {
        // Проигрываем анимацию смерти
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }

        // Звук смерти
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        // Ждём длительность анимации (узнаём из клипа "Death")
        float deathAnimLength = GetDeathAnimationLength();
        if (deathAnimLength > 0)
            yield return new WaitForSeconds(deathAnimLength);
        else
            yield return new WaitForSeconds(1.5f);  // запасной вариант

        // Отключаем управление и коллайдер
        DisableControl();

        // Показываем экран смерти только локальному игроку
        if (isLocalPlayer && deathScreenPrefab != null)
        {
            GameObject screen = Instantiate(deathScreenPrefab);
            screen.GetComponent<DeathScreen>().Setup();
        }
    }

    private float GetDeathAnimationLength()
    {
        if (anim == null || anim.runtimeAnimatorController == null) return 0f;
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name.ToLower().Contains("dead"))
                return clip.length;
        }
        return 1.5f;
    }

    private void DisableControl()
    {
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = false;
        var flashlight = GetComponentInChildren<Flashlight>();
        if (flashlight != null) flashlight.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
    }

    [Server]
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    [Server]
    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }
}