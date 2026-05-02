using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider healthSlider;

    private PlayerStats stats;

    void Start()
    {
        stats = GetComponentInParent<PlayerStats>();
        if (healthSlider == null) healthSlider = GetComponentInChildren<Slider>();

        if (stats != null)
        {
            healthSlider.maxValue = stats.maxHealth;
            healthSlider.value = stats.currentHealth;
        }
    }

    public void UpdateHealth()
    {
        if (stats != null && healthSlider != null)
            healthSlider.value = stats.currentHealth;
    }
}