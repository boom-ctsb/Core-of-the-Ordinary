using System;
using UnityEngine;

public class UltimateCooldown : MonoBehaviour
{
    [Header("Cooldown (seconds)")]
    [SerializeField] private float cooldownSeconds = 10f;

    [SerializeField] private bool startOnCooldown = false;

    private float timeRemaining = 0f;

    public event Action<UltimateCooldown> OnCooldownChanged;

    public float CooldownSeconds => cooldownSeconds;
    public float TimeRemaining => timeRemaining;

    public bool IsReady => timeRemaining <= 0f;

    private void Awake()
    {
        cooldownSeconds = Mathf.Max(0.1f, cooldownSeconds);

        if (startOnCooldown)
            timeRemaining = cooldownSeconds;
        else
            timeRemaining = 0f;
    }

    private void Update()
    {
        if (timeRemaining <= 0f) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining < 0f) timeRemaining = 0f;

        OnCooldownChanged?.Invoke(this);
    }

    public void StartCooldown()
    {
        timeRemaining = cooldownSeconds;
        OnCooldownChanged?.Invoke(this);
    }

    public void SetCooldownSeconds(float seconds)
    {
        cooldownSeconds = Mathf.Max(0.1f, seconds);
        timeRemaining = Mathf.Clamp(timeRemaining, 0f, cooldownSeconds);
        OnCooldownChanged?.Invoke(this);
    }

    public float GetReadyPercent01()
    {
        // 0 = ไม่พร้อม, 1 = พร้อม
        if (cooldownSeconds <= 0.0001f) return 1f;
        return Mathf.Clamp01(1f - (timeRemaining / cooldownSeconds));
    }
}