using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    private bool isInAnimation = false;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public IEnumerator FadeInCR(float time)
    {
        if (isInAnimation) { yield break; }
        isInAnimation = true;

        float currentLerp = 0f;
        while (currentLerp < 1f)
        {
            spriteRenderer.color = Color.Lerp(Color.clear, Color.white, currentLerp);

            currentLerp += (Time.deltaTime / time);
            yield return null;
        }

        isInAnimation = false;
        yield return null;
    }

    public IEnumerator FadeOutCR(float time)
    {
        if (isInAnimation) { yield break; }
        isInAnimation = true;

        float currentLerp = 0f;
        while (currentLerp < 1f)
        {
            spriteRenderer.color = Color.Lerp(Color.white, Color.clear, currentLerp);

            currentLerp += (Time.deltaTime / time);
            yield return null;
        }

        isInAnimation = false;
        yield return null;
    }

    public IEnumerator FizzleOutCR(float time)
    {
        if (isInAnimation) { yield break; }
        isInAnimation = true;

        float currentTimer = 0f;
        while (currentTimer < time)
        {
            spriteRenderer.color = ((int)(currentTimer * 10) % 2 == 0 ? Color.white : Color.clear);

            currentTimer += (Time.deltaTime / time);
            yield return null;
        }

        spriteRenderer.color = Color.clear;
        isInAnimation = false;
        yield return null;
    }
}
