using System;

[Serializable]
public class CharacterStats
{
    // ===== Base (จาก Data) =====
    public float MaxHP { get; set; }
    public float CurrentHP { get; set; }

    public float Attack { get; set; }
    public float Defense { get; set; }
    public float Speed { get; set; }

    public bool IsDead => CurrentHP <= 0;

    public CharacterStats()
    {
    }

    public CharacterStats(float hp, float attack, float defense, float speed)
    {
        MaxHP = hp;
        CurrentHP = hp;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }
}