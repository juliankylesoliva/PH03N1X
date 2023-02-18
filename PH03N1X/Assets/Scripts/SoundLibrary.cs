using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    [SerializeField] AudioClip[] soundList;
    private static Dictionary<string, AudioClip> _soundList = null;

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_soundList == null)
        {
            _soundList = new Dictionary<string, AudioClip>();
            foreach (AudioClip a in soundList)
            {
                Debug.Log(a.name);
                _soundList.Add(a.name, a);
            }
        }
    }

    public static AudioClip GetAudioClip(string name)
    {
        if (_soundList.ContainsKey(name))
        {
            return _soundList[name];
        }
        return null;
    }
}
