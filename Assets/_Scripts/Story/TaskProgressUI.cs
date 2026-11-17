using System.Text;
using TMPro;
using UnityEngine;
/// <summary>
/// แสดงภารกิจของขั้นปัจจุบัน เช่น "สำรวจบ้านร้าง 1/3" / "จัดการ Dysnorma"
/// ข้อความมาจาก EpisodeStep.objectiveText — อัปเดตเฉพาะตอนความคืบหน้า/ขั้นเปลี่ยน ไม่ใช่ทุกเฟรม
/// ติดกับ GameObject ที่มี TextMeshProUGUI
/// </summary>
public class TaskProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText;
    [Header("Format")]
    [Tooltip("{0} = ข้อความภารกิจของขั้น, {1} = ทำแล้ว, {2} = ทั้งหมด")]
    [SerializeField] private string countFormat = "{0} {1}/{2}";
    [Tooltip("ข้อความต่อท้ายตอน task ครบแล้วแต่ยังไม่เปลี่ยนขั้น (ว่าง = ใช้ countFormat เดิม)")]
    [SerializeField] private string completedSuffix = "";
    [Tooltip("แสดงรายชื่อ task ของขั้นนี้ พร้อมติ๊กว่าทำแล้ว")]
    [SerializeField] private bool showTaskList = false;
    [SerializeField] private string doneMark = "[x]";
    [SerializeField] private string todoMark = "[ ]";
    [Tooltip("ซ่อนข้อความเมื่อขั้นนี้ไม่มี objectiveText / ไม่อยู่ในขั้นใดเลย")]
    [SerializeField] private bool hideWhenEmpty = true;
    private readonly StringBuilder sb = new StringBuilder();
    private void Awake()
    {
        if (progressText == null)
            progressText = GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        EpisodeManager.OnTaskProgressChanged += Refresh;
        Refresh();
    }
    private void OnDisable()
    {
        EpisodeManager.OnTaskProgressChanged -= Refresh;
    }
    public void Refresh()
    {
        if (progressText == null) return;
        EpisodeManager ep = EpisodeManager.Instance;
        EpisodeStep step = ep != null ? ep.CurrentStep : null;
        if (step == null || string.IsNullOrEmpty(step.objectiveText))
        {
            progressText.text = "";
            progressText.enabled = !hideWhenEmpty;
            return;
        }
        progressText.enabled = true;
        sb.Clear();
        bool counted = step.type == StepType.Explore && step.showCount;
        if (counted)
        {
            ep.GetStepProgress(out int done, out int total);
            sb.AppendFormat(countFormat, step.objectiveText, done, total);
            if (done >= total && total > 0 && !string.IsNullOrEmpty(completedSuffix))
                sb.Append(' ').Append(completedSuffix);
        }
        else
        {
            sb.Append(step.objectiveText);
        }
        if (showTaskList && step.tasks != null)
        {
            foreach (EpisodeTask t in step.tasks)
            {
                if (t == null) continue;
                sb.Append('\n').Append(t.isCompleted ? doneMark : todoMark).Append(' ').Append(t.taskName);
            }
        }
        progressText.text = sb.ToString();
    }
}