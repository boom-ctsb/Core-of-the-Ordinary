using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Simple fade helper using a full-screen Image (on top Canvas).
// Assign the Image (black panel) and call FadeOutCoroutine / FadeInCoroutine.
// Optional: assign to TeleportManager.fadeController.
public class FadeController : MonoBehaviour
{
    public Image fadeImage; // full-screen black image
    public float fadeDuration = 0.35f;

    void Reset()
    {
        // try find Image in children
        fadeImage = GetComponentInChildren<Image>();
    }

    public IEnumerator FadeOutCoroutine()
    {
        if (fadeImage == null) yield break;
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, 1f);
        yield return null;
    }

    public IEnumerator FadeInCoroutine()
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = 1f - Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        fadeImage.color = new Color(c.r, c.g, c.b, 0f);
        fadeImage.gameObject.SetActive(false);
        yield return null;
    }
}