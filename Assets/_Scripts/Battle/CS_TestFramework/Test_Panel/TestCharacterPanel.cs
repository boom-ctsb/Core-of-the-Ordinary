using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel ทดสอบ - อัปเดตจำนวนช่อง Charge ค���ั้งละ 2 หน่วย
/// </summary>
public class TestCharacterPanel : MonoBehaviour
{
    [SerializeField] private TestCharacter character;        // ตัวละครที่เชื่อมกับ Panel นี้
    [SerializeField] private TextMeshProUGUI timeText;       // แสดงค่า ATB
    [SerializeField] private DynamicBar timeBar;             // Bar แสดงค่า ATB
    [SerializeField] private Image[] chargeBoxes = new Image[10]; // เชื่อมต่อ Image ช่อง Charge 10 ช่อง
    [SerializeField] private Color chargeFullColor = Color.green; // สีเมื่อช่อง Charge เต็ม
    [SerializeField] private Color chargeEmptyColor = new Color(0.2f, 0.2f, 0.2f); // สีเมื่อช่อง Charge ว่าง
    [SerializeField] private Button attackButton;            // ปุ่มสำหรับการโจมตี

    private bool isATBFull = false;  // บอกว่า ATB เต็มแล้วหรือไม่

    private void Start()
    {
        if (character == null)
        {
            character = GetComponentInParent<TestCharacter>();
        }

        if (timeBar == null)
        {
            timeBar = GetComponentInChildren<DynamicBar>();
        }

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }

        Debug.Log($"✅ TestCharacterPanel ถูกเตรียม {character.CharacterName}");

        // กำหนดค่า Charge Boxes เป็นสถานะ Empty ในตอนเริ่มต้น
        UpdateChargeBoxes();
    }

    private void Update()
    {
        if (character == null || timeBar == null) return;

        // ดึงค่า ATB ปัจจุบันกับ Max
        float currentATB = character.ATBTimer;
        float maxATB = character.ATBMaxTime;

        // อัปเดตค่า ATB Timer
        if (currentATB >= maxATB)
        {
            if (!isATBFull)
            {
                isATBFull = true; // Mark ว่า TimeBar เต็ม
                Debug.Log($"⏹️ {character.CharacterName} TimeBar เต็ม! รอกดโจมตี...");
            }

            // แสดงว่า TimeBar เต็ม
            timeText.text = $"{maxATB:F0}/{maxATB:F0}";
            timeBar.SetFillAmount(maxATB, maxATB);
        }
        else
        {
            isATBFull = false; // Reset สถานะเมื่อ ATB ไม่เต็ม
            timeText.text = $"{currentATB:F0}/{maxATB:F0}";
            timeBar.SetFillAmount(currentATB, maxATB);
        }

        // อัปเดตช่อง Charge Boxes
        UpdateChargeBoxes();
    }

    /// <summary>
    /// อัปเดตสถานะของ Charge Boxes ตามจำนวน Charge
    /// </summary>
    private void UpdateChargeBoxes()
    {
        if (character == null || chargeBoxes == null || chargeBoxes.Length == 0) return;

        int currentCharge = character.ChargeCount;

        Debug.Log($"🔄 อัปเดต Charge Count: {currentCharge} / {character.MaxChargeCount}");

        // อัปเดตสีของ Charge Boxes
        for (int i = 0; i < chargeBoxes.Length; i++)
        {
            if (i < currentCharge)
            {
                chargeBoxes[i].color = chargeFullColor; // ช่องเต็ม
            }
            else
            {
                chargeBoxes[i].color = chargeEmptyColor; // ช่องว่าง
            }
        }
    }

    /// <summary>
    /// เมื่อกดปุ่มโจมตี
    /// </summary>
    /// เมื่อกดปุ่มโจมตี
    /// </summary>
    public void OnAttackButtonClicked() // ⭐ เปลี่ยนจาก private → public
    {
        if (!isATBFull)
        {
            Debug.LogWarning($"⛔ {character.CharacterName} ไม่สามารถโจมตีได้! TimeBar ยังไม่เต็ม");
            return; // ป้องกันการโจมตีเมื่อ TimeBar ยังไม่เต็ม
        }

        Debug.Log($"⚔️ {character.CharacterName} โจมตี!");

        // เพิ่ม Charge Count ช่องละ 2 แต้ม
        character.AddCharge(2);

        // รีเซต TimeBar เพื่อเริ่มใหม่
        ResetTimeBar();
    }

    /// <summary>
    /// รีเซต TimeBar ให้เริ่มการนับใหม่หลังโจมตี
    /// </summary>
    public void ResetTimeBar()
    {
        if (character == null) return;

        isATBFull = false;    // Reset สถานะ ATB
        character.ResetATB(); // Reset ค่า ATB โค้ดใน TestCharacter.cs
        Debug.Log($"🔄 {character.CharacterName} TimeBar รีเซตแล้ว!");
    }
}