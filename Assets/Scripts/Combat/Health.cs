using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 5f;

    public float CurrentHealth { get; private set; }
    public float Fraction => maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float> Damaged;
    public event Action<float> Healed;
    public event Action Died;

    void Awake() { CurrentHealth = maxHealth; }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        amount = Mathf.Max(0f, amount);
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Damaged?.Invoke(amount);
        if (CurrentHealth <= 0f) Died?.Invoke();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        amount = Mathf.Max(0f, amount);
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        Healed?.Invoke(amount);
    }
}
