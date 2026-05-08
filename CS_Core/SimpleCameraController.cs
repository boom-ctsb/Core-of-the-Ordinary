using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    [Header("Camera Positions")]
    [SerializeField] private Transform idleCameraPos;
    [SerializeField] private Transform[] focusCameraPositions = new Transform[8];
    [SerializeField] private Transform dysnormaFocusPos;

    [Header("Settings")]
    [SerializeField] private float idleSize = 6f;
    [SerializeField] private float focusSize = 4f;

    [Header("Smooth (optional)")]
    [SerializeField] private bool useSmooth = true;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float zoomSpeed = 8f;

    private Vector3 targetPos;
    private float targetSize;
    private bool hasTarget = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = GetComponent<Camera>();

        if (idleCameraPos == null)
            Debug.LogError("❌ IdleCameraPos ไม่ได้ชี้!");

        for (int i = 0; i < focusCameraPositions.Length; i++)
        {
            if (focusCameraPositions[i] == null)
                Debug.LogWarning($"⚠️ FocusCamera Position {i} ไม่ได้ชี้!");
        }

        if (idleCameraPos != null && mainCamera != null)
        {
            targetPos = idleCameraPos.position;
            targetSize = idleSize;
            hasTarget = true;

            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = targetSize;
        }

        Debug.Log("✅ SimpleCameraController พร้อม");
    }

    private void LateUpdate()
    {
        if (!useSmooth || !hasTarget || mainCamera == null)
            return;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPos,
            1f - Mathf.Exp(-moveSpeed * Time.deltaTime)
        );

        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            targetSize,
            1f - Mathf.Exp(-zoomSpeed * Time.deltaTime)
        );
    }

    public void ToIdle()
    {
        if (idleCameraPos == null || mainCamera == null)
            return;

        SetTarget(idleCameraPos.position, idleSize);
        Debug.Log("📸 Idle Camera");
    }

    /// <summary>
    /// โฟกัสกล้องตามตัวละคร
    /// - Dysnorma -> dysnormaFocusPos
    /// - Ally -> focusCameraPositions[slotIndex]
    /// </summary>
    public void FocusCharacter(BattleCharacter character)
    {
        if (character == null || mainCamera == null) return;

        if (character is Dysnorma)
        {
            FocusDysnormaPosition();
            return;
        }

        // ✅ วิธี B: ใช้ SlotIndex (ตำแหน่งจริงในทีม)
        FocusAllyPosition(character.SlotIndex);
    }

    public void FocusAllyPosition(int slotIndex)
    {
        if (slotIndex < 0)
        {
            Debug.LogWarning($"⚠️ SlotIndex {slotIndex} ยังไม่ถูก set (ข้าม focus)");
            return;
        }

        if (focusCameraPositions == null || focusCameraPositions.Length == 0)
        {
            Debug.LogError("❌ focusCameraPositions ยังไม่ได้ตั้งค่า");
            return;
        }

        if (slotIndex >= focusCameraPositions.Length)
        {
            Debug.LogWarning($"⚠️ SlotIndex {slotIndex} เกินขนาด focusCameraPositions (len={focusCameraPositions.Length})");
            return;
        }

        Transform focusPos = focusCameraPositions[slotIndex];
        if (focusPos == null)
        {
            Debug.LogError($"❌ FocusCamera Position {slotIndex} ไม่ได้ชี้!");
            return;
        }

        SetTarget(focusPos.position, focusSize);
        Debug.Log($"📸 Focus Slot {slotIndex}");
    }

    public void FocusDysnormaPosition()
    {
        if (dysnormaFocusPos == null)
        {
            Debug.LogError("❌ DysnormaFocusPos ไม่ได้ชี้!");
            return;
        }

        SetTarget(dysnormaFocusPos.position, focusSize);
        Debug.Log("📸 Focus Dysnorma");
    }

    private void SetTarget(Vector3 pos, float size)
    {
        targetPos = pos;
        targetSize = size;
        hasTarget = true;

        if (!useSmooth && mainCamera != null)
        {
            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = targetSize;
        }
    }
}