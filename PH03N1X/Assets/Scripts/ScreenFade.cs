using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    [SerializeField] Image fadePanel;
    [SerializeField] Color unfadedColor;
    [SerializeField] Color completeFadeColor;

    private static float currentLerpValue = 0f;

    void Update()
    {
        fadePanel.color = Color.Lerp(unfadedColor, completeFadeColor, currentLerpValue);
    }

    public static IEnumerator SetFadeLerp(float lerp, float seconds)
    {
        float targetLerp = lerp;
        if (targetLerp < 0f) { targetLerp = 0f; }
        else if (targetLerp > 1f) { targetLerp = 1f; }
        else { /* Nothing */ }

        float fadeTime = (seconds > 0f ? seconds : 1f);

        while (currentLerpValue != targetLerp)
        {
            if (currentLerpValue < targetLerp)
            {
                currentLerpValue += (Time.deltaTime / fadeTime);
                if (currentLerpValue > targetLerp) { currentLerpValue = targetLerp; }
            }
            else
            {
                currentLerpValue -= (Time.deltaTime / fadeTime);
                if (currentLerpValue < targetLerp) { currentLerpValue = targetLerp; }
            }
            yield return null;
        }

        yield return null;
    }
}
