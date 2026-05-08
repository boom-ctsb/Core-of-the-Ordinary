using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Data/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Combo")]
    public int comboMax = 10;

    [Header("AI")]
    public bool preferFrontRowTargets = true;
    public int hitsPerAttackMin = 1;
    public int hitsPerAttackMax = 1;
}