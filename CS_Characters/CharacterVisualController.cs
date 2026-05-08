using UnityEngine;

public enum VisualState
{
    Idle,
    Ready,
    Animating
}

public class CharacterVisualController : MonoBehaviour
{
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private GameObject readyVisual;
    [SerializeField] private GameObject animationVisual;
    [SerializeField] private Animator characterAnimator;

    public void SetVisualState(VisualState state)
    {
        if (idleVisual != null) idleVisual.SetActive(state == VisualState.Idle);
        if (readyVisual != null) readyVisual.SetActive(state == VisualState.Ready);
        if (animationVisual != null) animationVisual.SetActive(state == VisualState.Animating);
    }

    public void PlayActionAnimation(string triggerName)
    {
        SetVisualState(VisualState.Animating);
        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger(triggerName);
        }
    }
}