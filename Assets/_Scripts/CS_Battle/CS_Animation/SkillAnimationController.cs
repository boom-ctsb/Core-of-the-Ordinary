using System.Collections;
using UnityEngine;

/// <summary>
/// จัดการ animation สำหรับ Skill
/// </summary>
public class SkillAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float animationSpeed = 1f;

    private bool isAnimationPlaying = false;

    public bool IsAnimationPlaying => isAnimationPlaying;

    /// <summary>
    /// เล่น animation ของ Skill
    /// </summary>
    public IEnumerator PlaySkillAnimation(string skillName, BattleCharacter caster)
    {
        if (animator == null)
        {
            isAnimationPlaying = true;
            yield return new WaitForSeconds(GetAnimationLength(skillName));
            isAnimationPlaying = false;
            yield break;
        }

        isAnimationPlaying = true;
        int triggerHash = Animator.StringToHash(skillName);
        animator.SetTrigger(triggerHash);

        // Wait until we enter the expected state (or timeout)
        const float enterTimeout = 1.0f;
        float enterTimer = 0f;
        int layer = 0; // adjust if your animation lives on another layer

        // Try to detect the state by name; fallback to clip length if detection fails
        bool stateFound = false;
        AnimatorStateInfo stateInfo = default;

        while (enterTimer < enterTimeout)
        {
            if (animator.IsInTransition(layer))
            {
                // still transitioning; wait a frame
                yield return null;
                enterTimer += Time.deltaTime;
                continue;
            }

            stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            if (stateInfo.IsName(skillName))
            {
                stateFound = true;
                break;
            }

            yield return null;
            enterTimer += Time.deltaTime;
        }

        if (stateFound)
        {
            // Wait until the state finishes (normalizedTime >= 1) or transitions away
            while (true)
            {
                if (animator.IsInTransition(layer))
                    break;

                stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (!stateInfo.IsName(skillName))
                    break;

                // normalizedTime can be >1 if looped; stop after it reaches >= 1
                if (stateInfo.normalizedTime >= 1f)
                    break;

                yield return null;
            }
        }
        else
        {
            // Fallback: wait clip length if we couldn't detect state
            yield return new WaitForSeconds(GetAnimationLength(skillName));
        }

        isAnimationPlaying = false;
    }

    private float GetAnimationLength(string skillName)
    {
        if (animator == null)
            return 1f;

        var controller = animator.runtimeAnimatorController;
        if (controller == null)
            return 1f;

        var clips = controller.animationClips;
        if (clips == null || clips.Length == 0)
            return 1f;

        foreach (AnimationClip clip in clips)
        {
            if (clip != null && clip.name == skillName)
                return clip.length / Mathf.Max(0.0001f, animationSpeed);
        }

        return 1f; // default fallback
    }
}