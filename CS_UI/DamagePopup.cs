using System.Collections;
using TMPro;
using UnityEngine;


/// <summary>
/// แสดง Damage number ที่ลอยขึ้น
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float floatHeight = 1f;

    public void ShowDamage(float damage, Vector3 position)
    {
        transform.position = position;
        damageText.text = damage.ToString("F0");

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        Vector3 startPos = transform.position;
        Color startColor = damageText.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // ลอยขึ้น
            transform.position = startPos + Vector3.up * (floatHeight * t);

            // จางลง
            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            damageText.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}