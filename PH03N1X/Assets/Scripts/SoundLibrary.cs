using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    private static AudioSource src;

    [SerializeField] AudioClip[] soundList;
    private static Dictionary<string, AudioClip> _soundList = null;

    void Awake()
    {
        Initialize();
        src = this.gameObject.GetComponent<AudioSource>();
    }

    private void Initialize()
    {
        if (_soundList == null)
        {
            _soundList = new Dictionary<string, AudioClip>();
            foreach (AudioClip a in soundList)
            {
                _soundList.Add(a.name, a);
            }
        }
    }

    public static AudioClip GetClip(string name)
    {
        if (_soundList.ContainsKey(name))
        {
            return _soundList[name];
        }
        return null;
    }

    public static void Play(string name, float volume)
    {
        src.clip = GetClip(name);
        src.volume = Mathf.Min(volume);
        src.Play();
    }
}
