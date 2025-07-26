using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource backGroundAudioSource;
    [SerializeField] private AudioSource soundEffectAudioSource;
    [SerializeField] private AudioClip backgroundMusicClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip itemClip;

    void Start()
    {
        PlayBackGroundMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayBackGroundMusic()
    {
        backGroundAudioSource.clip = backgroundMusicClip;
        backGroundAudioSource.Play();
    }

    public void PlayItemSound()
    {
        soundEffectAudioSource.PlayOneShot(itemClip);
    }

    public void PlayJumpSound() { 
        soundEffectAudioSource.PlayOneShot(jumpClip);
    }
}
