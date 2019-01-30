using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public interface IFadeImageSubscriber
{
    void NotifyFadeInFinished();
    void NotifyFadeOutFinished();
}

public class FadeImage : MonoSubscribable<IFadeImageSubscriber>
{
    private Image m_image;

    public const float AlphaToFadeIn = 0.0f;
    public const float AlphaToFadeOut = 1.0f;

    private void Awake()
    {
        m_image = GetComponent<Image>();
    }

    public void SetOpacity(bool opaque)
    {
        Color color = m_image.color;
        color.a = opaque ? 1.0f : .0f;
        m_image.color = color;
    }

    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(m_image.color.a, AlphaToFadeIn, duration));
    }

    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(m_image.color.a, AlphaToFadeOut, duration));
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

        color.a = to;
        m_image.color = color;

        if (to == AlphaToFadeIn)
        {
            foreach(IFadeImageSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyFadeInFinished();
            }
        }
        else if (to == AlphaToFadeOut)
        {
            foreach (IFadeImageSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyFadeOutFinished();
            }
        }
    }
}
