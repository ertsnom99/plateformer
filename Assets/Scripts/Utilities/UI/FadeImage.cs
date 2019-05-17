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
    private Image _image;

    public const float AlphaToFadeIn = 0.0f;
    public const float AlphaToFadeOut = 1.0f;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetOpacity(bool opaque)
    {
        Color color = _image.color;
        color.a = opaque ? 1.0f : .0f;
        _image.color = color;
    }

    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(_image.color.a, AlphaToFadeIn, duration));
    }

    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAlpha(_image.color.a, AlphaToFadeOut, duration));
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        Color color = _image.color;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            color.a = Mathf.Lerp(from, to, (Time.time - startTime) / duration);
            _image.color = color;

            yield return 0;
        }

        color.a = to;
        _image.color = color;

        if (to == AlphaToFadeIn)
        {
            foreach(IFadeImageSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyFadeInFinished();
            }
        }
        else if (to == AlphaToFadeOut)
        {
            foreach (IFadeImageSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyFadeOutFinished();
            }
        }
    }
}
