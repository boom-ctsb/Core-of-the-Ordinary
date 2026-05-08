using UnityEngine;

// เดินซ้าย/ขวาเท่านั้น ไม่ตก ไม่หมุน (ใช้ Rigidbody2D)
// หันซ้าย/ขวาด้วย spriteRenderer.flipX
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator animator; // ถ้าไม่มี ให้ปล่อยว่าง

    [Header("Input")]
    public string horizontalAxis = "Horizontal"; // A/D, Left/Right

    void Reset()
    {
        // ช่วยเซ็ต default เมื่อ Add Component ผ่าน Inspector
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // ใน Awake()
        if (rb != null)
        {
            rb.gravityScale = 0f; // ไม่ให้ตก
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY; // ห้ามหมุนและล็อก Y
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw(horizontalAxis); // -1..1

        // ตั้ง velocity ใน FixedUpdate แต่คำนวณ flip/animator ที่นี่
        UpdateFacingAndAnim(h);
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw(horizontalAxis);
        Vector2 vel = rb != null ? rb.velocity : Vector2.zero;
        // เคลื่อนเฉพาะแกน X เท่านั้น (รักษา Y เดิมไว้ = 0 ตามการตั้ง gravityScale)
        vel.x = h * moveSpeed;
        vel.y = 0f; // บังคับไม่ให้เปลี่ยน y
        if (rb != null) rb.velocity = vel;
    }

    void UpdateFacingAndAnim(float horizontal)
    {
        // flip sprite ตามทิศ
        if (spriteRenderer != null)
        {
            if (horizontal > 0.01f) spriteRenderer.flipX = false; // art assumed facing right
            else if (horizontal < -0.01f) spriteRenderer.flipX = true;
        }

        // Animator parameters (ถ้ามี)
        if (animator != null)
        {
            animator.SetFloat("MoveX", horizontal);
            animator.SetFloat("Speed", Mathf.Abs(horizontal));
            // ถ้าต้องการ front/back ให้ใช้ animator.SetBool("FacingFront", ...) จากโค้ดอื่นเรียก
        }
    }

    // PUBLIC API ---------------------------------------------------------------
    // เรียกเพื่อบังคับเปลี่ยนภาพเป็นหน้า (front) หรือ หลัง (back) โดยไม่หมุน Transform
    // ติดตั้งใน Animator parameter (optional)
    public void SetFacingFront(bool showFront)
    {
        if (animator != null)
        {
            animator.SetBool("FacingFront", showFront);
            // ใน Animator ให้เชื่อม Bool "FacingFront" เพื่อสลับ sprite/animation เป็นหน้าหรือหลัง
        }
    }

    // helper: หันไปขวา/ซ้าย แบบโปรแกรม (ไม่เปลี่ยน velocity)
    public void SetFacingDirection(bool faceRight)
    {
        if (spriteRenderer != null) spriteRenderer.flipX = !faceRight;
        if (animator != null) animator.SetBool("FacingRight", faceRight);
    }
}