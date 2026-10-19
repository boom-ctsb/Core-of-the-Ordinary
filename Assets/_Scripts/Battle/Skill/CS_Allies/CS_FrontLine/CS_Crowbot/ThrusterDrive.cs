using System.Collections;
using UnityEngine;

/// <summary>
/// สกิล Thruster Drive ของ Crowbot
/// </summary>
public class ThrusterDrive : SkillAction
{
    /*private float atkMultiplier = 1.4f;   // สามารถปรับค่า multiplier ได้ภายหลัง

    public ThrusterDrive() : base
        ("Thruster Drive",                                  // name
         "โจมตีศัตรูเดียว ด้วยค่า Atk x1.4 ...",                    // description
          0f,                                              // damage (อาจใช้ dummy ไว้ก่อนถ้า override ทีหลัง)
         "ThrusterDrive")                                  // animationName
    {
        this.ActionName = "Thruster Drive";
        this.Description = "โจมตีศัตรูเดียว ด้วยค่า Atk x1.4 และเมื่อจบเทิร์น ATB จะเริ่มต้นใหม่ที่ 10%";
        this.animationName = "ThrusterDrive";
        this.chargeCost = 2f;
    }
    public override IEnumerator Execute(BattleManager battleManager, BattleCharacter source, BattleCharacter target)
    {
        // ⏸️ หยุด TimeBar
        battleManager.PauseAllTimeBars();

        // 🎬 เรียก Animation ของ Thruster Drive
        yield return battleManager.PlayCharacterAnimation(animationName, source);

        // 💢 ทำ Damage (Atk x multiplier)
        float damageDealt = source.Stats.Attack * atkMultiplier;
        target.TakeDamage(damageDealt);

        // 💥 แสดง Damage Popup
        battleManager.ShowDamageEffect(damageDealt, target.transform.position);

        // ▶️ กลับมาเดิน TimeBar ทุกตัว
        battleManager.ResumeAllTimeBars();

        // ⭐ เพิ่ม Effect: เริ่ม turn หน้า Crowbot ที่ ATB 10% (หลัง ResetATB)
        // — ใส่ logic หลัง Action สำเร็จ เช่น ใน BattleManager.EndPlayerTurn()
        if (source is Crowbot)
        {
            ((Crowbot)source).NextTurnInitialAtbPercent = 0.1f;
        }
    }*/
}