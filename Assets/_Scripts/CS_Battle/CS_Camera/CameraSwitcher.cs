using UnityEngine;

/// <summary>
/// สลับระหว่าง Idle Camera (เห็นทั้งฉาก) และ Focus Camera (โฟกัสตัวละครแต่ละตัว)
/// </summary>
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private Camera idleCamera;      // กล้องดูทั้งฉาก (ฉากที่ 1)
    [SerializeField] private Camera[] focusCameras;  // กล้องแต่ละตัว (ฉากที่ 2)

    private void Start()
    {
        SetIdleCamera();
    }

    /// <summary>
    /// ฉากที่ 1: กล้อง Idle (รอเทิร์น)
    /// </summary>
    public void SetIdleCamera()
    {
        DisableAllCameras();
        idleCamera.enabled = true;
        Debug.Log("📸 Idle Camera");
    }

    /// <summary>
    /// ฉากที่ 2: กล้อง Focus ของตัวละครตัวที่ได้เทิร์น
    /// </summary>
    public void SetFocusCamera(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= focusCameras.Length)
        {
            Debug.LogWarning($"⚠�� Focus Camera {characterIndex} ไม่มี!");
            return;
        }

        DisableAllCameras();
        focusCameras[characterIndex].enabled = true;
        Debug.Log($"📸 Focus Camera {characterIndex}");
    }

    private void DisableAllCameras()
    {
        idleCamera.enabled = false;
        foreach (var cam in focusCameras)
        {
            if (cam != null)
                cam.enabled = false;
        }
    }
}