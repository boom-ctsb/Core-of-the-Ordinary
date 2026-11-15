using System.Text;
using TMPro;
using UnityEngine;
/// <summary>
/// แสดงความคืบหน้าภารกิจของ EP ปัจจุบัน เช่น "ตามหาของสำคัญ 1/3"
/// ติดกับ GameObject ที่มี TextMeshProUGUI
/// </summary>
public class TaskProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI progressText;
    [Header("Filter")]
    [Tooltip("เปิด = นับเฉพาะ task ชนิดที่เลือก (เช่น Collect)\nปิด = นับทุก task ของ EP")]
    [SerializeField] private bool filterByType = true;
    [SerializeField] private TaskType taskType = TaskType.Collect;
    [Header("Format")]
    [Tooltip("{0} = ชื่อหัวข้อ, {1} = ทำแล้ว, {2} = ทั้งหมด")]
    [SerializeField] private string format = "{0} {1}/{2}";
    [SerializeField] private string title = "ตามหาของสำคัญ";
    [Tooltip("ข้อความตอนครบทุกอัน (ว่าง = ใช้ format เดิม)")]
    [SerializeField] private string completedText = "ครบแล้ว! ไปต่อที่จุดหมาย";
    [Tooltip("แสดงรายชื่อ task แต่ละอัน พร้อมติ๊กว่าทำแล้ว")]
    [SerializeField] private bool showTaskList = false;
    [SerializeField] private string doneMark = "[x]";
    [SerializeField] private string todoMark = "[ ]";
    [Tooltip("ซ่อนข้อความถ้า EP นี้ไม่มี task ตามตัวกรอง")]
    [SerializeField] private bool hideWhenNoTasks = true;
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
        EpisodeTask[] tasks = EpisodeManager.Instance != null
            ? EpisodeManager.Instance.Tasks
            : System.Array.Empty<EpisodeTask>();
        int total = 0;
        int done = 0;
        for (int i = 0; i < tasks.Length; i++)
        {
            EpisodeTask t = tasks[i];
            if (t == null) continue;
            if (filterByType && t.taskType != taskType) continue;
            total++;
            if (t.isCompleted) done++;
        }
        if (total == 0)
        {
            progressText.text = "";
            progressText.enabled = !hideWhenNoTasks;
            return;
        }
        progressText.enabled = true;
        sb.Clear();
        if (done >= total && !string.IsNullOrEmpty(completedText))
            sb.Append(completedText);
        else
            sb.AppendFormat(format, title, done, total);
        if (showTaskList)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                EpisodeTask t = tasks[i];
                if (t == null) continue;
                if (filterByType && t.taskType != taskType) continue;
                sb.Append('\n').Append(t.isCompleted ? doneMark : todoMark).Append(' ').Append(t.taskName);
            }
        }
        progressText.text = sb.ToString();
    }
}