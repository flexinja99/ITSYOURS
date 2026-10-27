
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip explosionClip;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        float musicVolume =
            PlayerPrefs.GetFloat("MusicVolume", 0.25f);

        float sfxVolume =
            PlayerPrefs.GetFloat("SfxVolume", 0.8f);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
            sfxSource.spatialBlend = 0f;
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayExplosion()
    {
        if (sfxSource != null &&
            explosionClip != null)
        {
            sfxSource.PlayOneShot(explosionClip);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
