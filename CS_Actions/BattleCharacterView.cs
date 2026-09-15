using UnityEngine;
using System.Collections;

public class BattleCharacterView : MonoBehaviour
{
    [SerializeField] private BattleCharacter battleCharacter;
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private Animator animator;

    private GameObject idleGameObject;   // ท่า Idle (ตัวนี้)
    private GameObject actionGameObject; // ท่า Action (ตั้งไว้เปลี่ยน)

    [Header("Animation")]
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string attackAnimation = "Attack";
    [SerializeField] private string damageAnimation = "Damage";

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (characterSprite == null)
            characterSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        idleGameObject = gameObject;  // GameObject นี้เป็น Idle
        PlayAnimation(idleAnimation);
    }

    private void Update()
    {
        if (battleCharacter != null && battleCharacter.Stats.IsDead)
        {
            canvasGroup.alpha = 0.5f;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// ตั้งค่า ActionGameObject (เรียกจาก Factory)
    /// </summary>
    public void SetCharacterData(string characterName, GameObject actionObj)
    {
        actionGameObject = actionObj;
        Debug.Log($"🎬 {characterName} - Idle: {idleGameObject.name}, Action: {actionGameObject?.name ?? "None"}");
    }

    /// <summary>
    /// เปลี่ยน���ป็น Idle Pose
    /// </summary>
    public void ShowIdlePose()
    {
        if (idleGameObject != null)
        {
            idleGameObject.SetActive(true);
            if (actionGameObject != null)
                actionGameObject.SetActive(false);

            PlayAnimation(idleAnimation);
            Debug.Log($"🎯 {battleCharacter.CharacterName} → Idle Pose");
        }
    }

    /// <summary>
    /// เปลี่ยนเป็น Action Pose
    /// </summary>
    public void ShowActionPose()
    {
        if (actionGameObject != null)
        {
            actionGameObject.SetActive(true);
            idleGameObject.SetActive(false);

            PlayAnimation(attackAnimation);
            Debug.Log($"⚔️ {battleCharacter.CharacterName} → Action Pose");
        }
    }

    public void PlayAnimation(string animationName, float duration = 0)
    {
        if (animator == null) return;
        animator.SetTrigger(animationName);
    }

    public void PlayDamageAnimation(float damage)
    {
        StartCoroutine(DamageAnimationCoroutine(damage));
    }

    private IEnumerator DamageAnimationCoroutine(float damage)
    {
        PlayAnimation(damageAnimation);
        yield return StartCoroutine(FlashWhite(0.1f));
        yield return new WaitForSeconds(0.5f);
        PlayAnimation(idleAnimation);
    }

    private IEnumerator FlashWhite(float duration)
    {
        if (characterSprite != null)
        {
            Color originalColor = characterSprite.color;
            characterSprite.color = Color.white;
            yield return new WaitForSeconds(duration);
            characterSprite.color = originalColor;
        }
    }

    public BattleCharacter BattleCharacter => battleCharacter;
}