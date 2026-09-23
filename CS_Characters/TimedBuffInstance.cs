using System;
using UnityEngine;

[Serializable]
public class TimedBuffInstance
{
    [Header("Identity")]
    public string BuffKey;
    public bool IsDebuff = false;

    [Header("Duration")]
    public float DurationSeconds = 5f;

    [NonSerialized] private float timeRemaining = 0f;
    [NonSerialized] private float timeCreated = 0f;
    [NonSerialized] private bool runtimeInitialized = false;

    [Header("Flat Stats")]
    public float AttackAdd = 0f;
    public float DefenseAdd = 0f;
    public float SpeedAdd = 0f;

    [Header("Percent Stats")]
    public float SpeedPercent = 0f;
    public float AccuracyPercent = 0f;
    public float EvasionChanceAdd = 0f;

    [Header("Extra")]
    public float HpRegenPerSecond = 0f;
    public int BonusChargeBoxesOnAttack = 0;

    [Header("UI")]
    public Sprite IconSprite;
    public GameObject IconPrefabOverride;

    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public float TimeCreated => timeCreated;
    public bool IsExpired => runtimeInitialized && timeRemaining <= 0f;

    public void Refresh(float durationSeconds)
    {
        DurationSeconds = Mathf.Max(0f, durationSeconds);
        timeRemaining = DurationSeconds;
        timeCreated = Time.time;
        runtimeInitialized = true;
    }

    public void Tick(float deltaTime)
    {
        if (!runtimeInitialized)
            Refresh(DurationSeconds);

        if (timeRemaining <= 0f)
            return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Mathf.Max(0f, deltaTime));
    }

    public TimedBuffInstance CloneForRuntime()
    {
        TimedBuffInstance clone = new TimedBuffInstance
        {
            BuffKey = BuffKey,
            IsDebuff = IsDebuff,
            DurationSeconds = DurationSeconds,
            AttackAdd = AttackAdd,
            DefenseAdd = DefenseAdd,
            SpeedAdd = SpeedAdd,
            SpeedPercent = SpeedPercent,
            AccuracyPercent = AccuracyPercent,
            EvasionChanceAdd = EvasionChanceAdd,
            HpRegenPerSecond = HpRegenPerSecond,
            BonusChargeBoxesOnAttack = BonusChargeBoxesOnAttack,
            IconSprite = IconSprite,
            IconPrefabOverride = IconPrefabOverride
        };

        clone.Refresh(DurationSeconds);
        return clone;
    }
}

