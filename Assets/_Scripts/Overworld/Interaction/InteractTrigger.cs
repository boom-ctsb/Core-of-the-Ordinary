using UnityEngine;

/// <summary>วิธีเริ่ม interact — ใช้ร่วมกันทั้ง ObjectInteractable และ TalkInteractable</summary>
public enum InteractTrigger
{
    PressKey,     // เข้าใกล้แล้วต้องกดปุ่ม interact
    AutoOnEnter   // เข้าใกล้แล้วเริ่มเลย (เช่น เข้าฉาก Dialogue ทันที / ถึงจุดที่กำหนด)
}