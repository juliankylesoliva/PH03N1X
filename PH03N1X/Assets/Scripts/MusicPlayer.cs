using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static AudioSource src;

    void Awake()
    {
        src = this.gameObject.GetComponent<AudioSource>();
    }

    public static void PlayMusic()
    {
        if (src.isPlaying) { return; }
        src.volume = 1f;
        src.Play();
    }

    public static IEnumerator FadeOut(float seconds)
    {
        if (!src.isPlaying) { yield break; }

        float currentLerp = 1f;
        while (currentLerp > 0f)
        {
            currentLerp -= (Time.deltaTime / seconds);
            if (currentLerp < 0f) { currentLerp = 0f; }

            src.volume = currentLerp;

            yield return null;
        }
        src.Stop();

        yield return null;
    }

    public static void StopMusic()
    {
        if (!src.isPlaying) { return; }
        src.Stop();
    }
}
