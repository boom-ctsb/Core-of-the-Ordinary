using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum BattleOutcome
{
    None,
    Won,
    Lost
}
/// <summary>
/// เก็บความคืบหน้า EP ที่กำลังเล่น "ข้ามซีน" (เช่น ไป BattleScene แล้วกลับมา StoryScene)
/// เป็น static ล้วน ไม่ต้องวางใน Scene
/// </summary>
public static class StoryProgress
{
    public static EpisodeData Episode { get; private set; }
    public static int StepIndex { get; private set; }
    public static BattleOutcome LastOutcome { get; private set; } = BattleOutcome.None;
    /// <summary>ซีนที่ต้องกลับมาหลังสู้จบ</summary>
    public static string StorySceneName { get; private set; }
    /// <summary>true ระหว่างอยู่ในการสู้ที่ถูกเรียกจากเนื้อเรื่อง (BattleManager ใช้เช็ค)</summary>
    public static bool BattleActive { get; private set; }
    /// <summary>ชื่อศัตรูที่ step กำหนด (ว่าง = ใช้ค่าใน BattleManager)</summary>
    public static string EnemyName { get; private set; }
    public static bool HasPlayerPosition { get; private set; }
    public static Vector3 PlayerPosition { get; private set; }
    private static bool returningFromBattle;
    private static readonly HashSet<string> completedTasks = new HashSet<string>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Clear();
    }
    // ─────────────────────────────────────────
    // Session
    // ─────────────────────────────────────────
    /// <summary>เริ่ม EP ใหม่ตั้งแต่ต้น (ล้างของเก่า)</summary>
    public static void Begin(EpisodeData episode)
    {
        Clear();
        Episode = episode;
    }
    /// <summary>
    /// เรียกตอน EpisodeManager เริ่ม — true ถ้ากลับมาจากการสู้ของ EP เดิม
    /// (ใช้ได้ครั้งเดียวต่อการกลับมา 1 ครั้ง)
    /// </summary>
    public static bool TryResume(EpisodeData episode, out BattleOutcome outcome)
    {
        outcome = BattleOutcome.None;
        if (!returningFromBattle || Episode != episode) return false;
        returningFromBattle = false;
        outcome = LastOutcome;
        LastOutcome = BattleOutcome.None;
        return true;
    }
    public static void Clear()
    {
        Episode = null;
        StepIndex = 0;
        LastOutcome = BattleOutcome.None;
        StorySceneName = null;
        BattleActive = false;
        EnemyName = null;
        HasPlayerPosition = false;
        returningFromBattle = false;
        completedTasks.Clear();
    }
    public static void SetStepIndex(int index) => StepIndex = index;
    // ─────────────────────────────────────────
    // Tasks
    // ─────────────────────────────────────────
    public static void MarkTaskDone(string taskId)
    {
        if (!string.IsNullOrEmpty(taskId)) completedTasks.Add(taskId);
    }
    public static bool IsTaskDone(string taskId)
    {
        return !string.IsNullOrEmpty(taskId) && completedTasks.Contains(taskId);
    }
    // ─────────────────────────────────────────
    // Battle
    // ─────────────────────────────────────────
    /// <summary>บันทึกสถานะแล้วโหลดซีนสู้ — เรียกจาก EpisodeManager</summary>
    public static bool LaunchBattle(string battleScene, string enemyName, int stepIndex, Vector3 playerPos)
    {
        if (!Application.CanStreamedLevelBeLoaded(battleScene))
        {
            Debug.LogError($"[StoryProgress] Scene '{battleScene}' ไม่อยู่ใน Build Settings!");
            return false;
        }
        StepIndex = stepIndex;
        StorySceneName = SceneManager.GetActiveScene().name;
        EnemyName = enemyName;
        PlayerPosition = playerPos;
        HasPlayerPosition = true;
        BattleActive = true;
        LastOutcome = BattleOutcome.None;
        Time.timeScale = 1f;
        SceneManager.LoadScene(battleScene);
        return true;
    }
    /// <summary>เรียกจาก BattleManager เมื่อสู้จบ — กลับไปซีนเนื้อเรื่อง</summary>
    public static void FinishBattle(bool won)
    {
        if (!BattleActive) return;
        BattleActive = false;
        returningFromBattle = true;
        LastOutcome = won ? BattleOutcome.Won : BattleOutcome.Lost;
        Time.timeScale = 1f;
        SceneManager.LoadScene(StorySceneName);
    }
}