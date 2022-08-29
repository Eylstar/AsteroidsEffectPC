using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public static Sounds Instance;
    [SerializeField] private AudioClip asteroidExplode = null;
    [SerializeField] private AudioClip shipExplode = null;
    [SerializeField] private AudioClip shipShoot = null;

    void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public void PlayShootSound()
    {
        PlaySound(shipShoot, 0.2f * PlayerPrefs.GetFloat("Sound"));
    }

    public void PlayAsteroidExplodeSound()
    {
        PlaySound(asteroidExplode, 0.5f * PlayerPrefs.GetFloat("Sound"));
    }

    public void PlayShipExplodeSound()
    {
        PlaySound(shipExplode, 1f * PlayerPrefs.GetFloat("Sound"));
    }

    public void PlaySound(AudioClip Clip, float volume)
    {
        AudioSource.PlayClipAtPoint(Clip, transform.position, volume);
    }
}