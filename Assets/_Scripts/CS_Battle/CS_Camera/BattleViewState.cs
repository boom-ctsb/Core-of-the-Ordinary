/// <summary>
/// enum สำหรับสถานะการแสดงผล Battle
/// </summary>
public enum BattleViewState
{
    Idle,              // ทั้งทีมอยู่ฉากปกติ
    FocusedCharacter,  // โฟกัสตัวละครคนเดียว (แสดง Skill Menu)
    SkillAnimation,    // เล่น animation
}