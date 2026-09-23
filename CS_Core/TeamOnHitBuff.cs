using UnityEngine;

public class TeamOnHitBuff
{
    public BattleCharacter Owner { get; }
    public float OwnerAttackMultiplier { get; private set; }
    public float FlatBonusDamage { get; private set; }
    public bool AddEnemyComboOnProc { get; set; } = true;
    public int ComboAddAmount { get; set; } = 1;

    private float durationRemaining;

    public bool IsExpired => durationRemaining <= 0f;

    public TeamOnHitBuff(BattleCharacter owner, float ownerAttackMultiplier, float flatBonusDamage, float durationSeconds)
    {
        Owner = owner;
        OwnerAttackMultiplier = ownerAttackMultiplier;
        FlatBonusDamage = flatBonusDamage;
        durationRemaining = Mathf.Max(0f, durationSeconds);
    }

    public void Tick(float deltaTime)
    {
        if (durationRemaining <= 0f)
            return;

        durationRemaining = Mathf.Max(0f, durationRemaining - Mathf.Max(0f, deltaTime));
    }

    public void RefreshDuration(float durationSeconds)
    {
        durationRemaining = Mathf.Max(0f, durationSeconds);
    }

    public void UpdatePower(float ownerAttackMultiplier, float flatBonusDamage)
    {
        OwnerAttackMultiplier = ownerAttackMultiplier;
        FlatBonusDamage = flatBonusDamage;
    }
}

