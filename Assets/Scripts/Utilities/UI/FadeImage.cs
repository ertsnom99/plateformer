using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour
{
    private Image m_image;

    private void Awake()
    {
        m_image = GetComponent<Image>();
    }

    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(m_image.color.a, 0.0f, duration));
    }

    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(m_image.color.a, 1.0f, duration));
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        Color color = m_image.color;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            color.a = Mathf.Lerp(from, to, (Time.time - startTime) / duration);
            m_image.color = color;

            yield return 0;
        }
    }
}
