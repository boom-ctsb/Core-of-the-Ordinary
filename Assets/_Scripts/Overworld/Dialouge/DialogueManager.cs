using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton ควบคุม Dialogue ทั้งหมด
/// - รับ DialogueData จาก InteractableDialogue
/// - เดิน Beat ทีละตัว
/// - Lock/Unlock PlayerController
/// - จัดการ repeatBeats
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private DialogueSceneDirector sceneDirector;

    [Header("Dialogue Canvas")]
    [Tooltip("Canvas ทั้งหมดของ Dialogue — จะ SetActive true/false")]
    [SerializeField] private GameObject dialogueCanvas;

    [Header("Input")]
    [Tooltip("กด key นี้ หรือ คลิกซ้าย เพื่อไปบรรทัดถัดไป")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    public bool IsPlaying { get; private set; } = false;

    private DialogueBeat[] currentBeats;
    private int currentBeatIndex = 0;
    private PlayerController lockedPlayer;
    private bool waitingForInput = false;
    private Coroutine sceneWaitCoroutine;

    // จำว่า NPC ไหนเคยคุยแล้วบ้าง (ใช้ DialogueData instance เป็น key)
    private readonly HashSet<DialogueData> spokenData = new HashSet<DialogueData>();

    // ─────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ปิด canvas ตอนเริ่ม
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    // ─────────────────────────────────────────
    // เริ่ม Dialogue — เรียกจาก InteractableDialogue
    // ─────────────────────────────────────────

    public void StartDialogue(DialogueData data, PlayerController player)
    {
        if (IsPlaying) return;
        if (data == null || data.beats == null || data.beats.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] DialogueData ว่างเปล่า ไม่มี beats");
            return;
        }

        IsPlaying = true;
        lockedPlayer = player;

        // Lock player
        if (lockedPlayer != null)
            lockedPlayer.enabled = false;

        // เลือก beats ที่จะเล่น
        bool hasSpoken = spokenData.Contains(data);
        currentBeats = data.GetBeatsToPlay(hasSpoken);
        currentBeatIndex = 0;

        // บันทึกว่าเคยคุยแล้ว
        spokenData.Add(data);

        // เปิด canvas + สร้างฉาก
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        sceneDirector.BuildScene(data);

        if (verboseLog)
            Debug.Log($"[DialogueManager] StartDialogue — beats={currentBeats.Length}, hasSpoken={hasSpoken}");

        // เล่น beat แรก
        PlayCurrentBeat();
    }

    // ─────────────────────────────────────────
    // Update — รับ input
    // ─────────────────────────────────────────

    private void Update()
    {
        if (!IsPlaying) return;
        if (!waitingForInput) return;

        bool pressed = Input.GetKeyDown(advanceKey) || Input.GetMouseButtonDown(0);
        if (pressed)
            AdvanceToNextBeat();
    }

    // ─────────────────────────────────────────
    // เล่น Beat ปัจจุบัน
    // ─────────────────────────────────────────

    private void PlayCurrentBeat()
    {
        if (currentBeatIndex >= currentBeats.Length)
        {
            EndDialogue();
            return;
        }

        DialogueBeat beat = currentBeats[currentBeatIndex];

        // Apply การเปลี่ยนแปลงฉาก (พร้อมกันทุกชิ้น)
        sceneDirector.ApplyChanges(beat);

        if (beat.isSceneBeat)
        {
            // Scene Beat: ซ่อน chatbox แล้วรอ waitSeconds
            dialogueUI.HideChatbox();
            waitingForInput = false;

            if (sceneWaitCoroutine != null)
                StopCoroutine(sceneWaitCoroutine);

            sceneWaitCoroutine = StartCoroutine(WaitAndAdvance(beat.waitSeconds));

            if (verboseLog)
                Debug.Log($"[DialogueManager] SceneBeat[{currentBeatIndex}] wait={beat.waitSeconds}s, changes={beat.changes.Length}");
        }
        else
        {
            // Text Beat: แสดงข้อความ รอ input
            dialogueUI.ShowText(beat.speakerName, beat.dialogueText);
            waitingForInput = true;

            if (verboseLog)
                Debug.Log($"[DialogueManager] TextBeat[{currentBeatIndex}] speaker='{beat.speakerName}', changes={beat.changes.Length}");
        }
    }

    // ─────────────────────────────────────────
    // ไปต่อ
    // ─────────────────────────────────────────

    private void AdvanceToNextBeat()
    {
        waitingForInput = false;
        currentBeatIndex++;
        PlayCurrentBeat();
    }

    // ─────────────────────────────────────────
    // Scene Beat: รอแล้วไปต่อเอง
    // ─────────────────────────────────────────

    private IEnumerator WaitAndAdvance(float seconds)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, seconds));
        currentBeatIndex++;
        PlayCurrentBeat();
    }

    // ─────────────────────────────────────────
    // จบ Dialogue
    // ─────────────────────────────────────────

    private void EndDialogue()
    {
        IsPlaying = false;
        waitingForInput = false;

        // ซ่อน chatbox + ล้างฉาก
        dialogueUI.HideChatbox();
        sceneDirector.ClearScene();

        // ปิด canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // Unlock player
        if (lockedPlayer != null)
        {
            lockedPlayer.enabled = true;
            lockedPlayer = null;
        }

        if (verboseLog)
            Debug.Log("[DialogueManager] EndDialogue");
    }

    // ─────────────────────────────────────────
    // Public: Reset ประวัติการคุย (ถ้าต้องการ)
    // ─────────────────────────────────────────

    public void ResetSpokenHistory(DialogueData data)
    {
        if (data != null) spokenData.Remove(data);
    }

    public void ResetAllSpokenHistory()
    {
        spokenData.Clear();
    }
}