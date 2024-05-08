using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class InterpolationUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerator InterpolateValue(float startValue, float targetValue, float duration, Action<float> setValueAction)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            setValueAction(Mathf.Lerp(startValue, targetValue, Mathf.Clamp01(elapsedTime / duration)));

            yield return null;
        }

        setValueAction(targetValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerator InterpolateColor(Color startColor, Color targetColor, float duration, Action<Color> setValueAction)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            setValueAction(Color.Lerp(startColor, targetColor, Mathf.Clamp01(elapsedTime / duration)));

            yield return null;
        }

        setValueAction(targetColor);
    }
}
