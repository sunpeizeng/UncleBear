using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean;

//Kevin.Zhang, 1/23/2017
/// <summary>
/// a wrapper of LeanPool to store audio source
/// </summary>
public class AudioSourcePool : DoozyUI.Singleton<AudioSourcePool>
{
    private LeanPool _mLeanPoolInstance;
    private GameObject _mPrefab;

    void Awake()
    {
        _mPrefab = new GameObject("AudioSource");
        AudioSource aSource = _mPrefab.AddComponent<AudioSource>();
        aSource.playOnAwake = false;
        _mPrefab.transform.SetParent(this.transform);
        //_mPrefab.hideFlags = HideFlags.HideInHierarchy;
        _mPrefab.SetActive(false);

        _mLeanPoolInstance = LeanPool.GetOrCreateInstance(_mPrefab);
        _mLeanPoolInstance.gameObject.name = "Pool";
        _mLeanPoolInstance.TimeScaleIndependent = true;
        //turn off notification, avoid using SendMessage
        _mLeanPoolInstance.Notification = LeanPool.NotificationType.None;
        _mLeanPoolInstance.transform.SetParent(this.transform);
    }

    public AudioSource Spawn(AudioClip clip, Vector3 position)
    {
        AudioSource aSource = LeanPool.Spawn(_mPrefab, position, Quaternion.identity, _mLeanPoolInstance.transform).GetComponent<AudioSource>();
        aSource.gameObject.name = "Audio - " + clip.name;
        aSource.clip = clip;
        if (aSource.isPlaying)
        {
            aSource.Stop();
            aSource.time = 0f;
        }

        return aSource;
    }

    public void Free(AudioSource audioSource, float delay = 0f)
    {
        if (audioSource != null)
            LeanPool.Despawn(audioSource.gameObject, delay);
    }

    public void Preload(int count)
    {
        _mLeanPoolInstance.Load(5);
    }
}
