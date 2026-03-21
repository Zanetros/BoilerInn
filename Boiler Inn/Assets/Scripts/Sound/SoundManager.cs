using UnityEngine;
using UnityEngine.Audio;
using System.Collections; 

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Settings")]
    public float fadeDuration = 1.0f; 

    private Coroutine currentFadeCoroutine; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ========================================================
        // A MÁGICA: A música se torna imune ao pause global do jogo!
        // ========================================================
        if (musicSource != null)
        {
            musicSource.ignoreListenerPause = true;
        }
        // ========================================================

        LoadVolume();
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return; 

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeMusic(clip));
    }
    
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    
    public void FadeOutSFX(float duration = 0.2f)
    {
        if (sfxSource.isPlaying)
        {
            StartCoroutine(DoFadeOutSFX(duration));
        }
    }

    private IEnumerator DoFadeOutSFX(float duration)
    {
        float startVolume = sfxSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        sfxSource.Stop();
        sfxSource.volume = startVolume; 
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                // ATENÇÃO: Mudamos Time.deltaTime para Time.unscaledDeltaTime
                // Isso garante que o fade da música funcione mesmo se o jogo estiver pausado (Time.timeScale = 0)!
                musicSource.volume -= startVolume * Time.unscaledDeltaTime / (fadeDuration / 2);
                yield return null; 
            }
            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.Play();

        while (musicSource.volume < 1f)
        {
            musicSource.volume += Time.unscaledDeltaTime / (fadeDuration / 2);
            yield return null;
        }

        musicSource.volume = 1f; 
        currentFadeCoroutine = null;
    }

    public void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(0.0001f, musicVol)) * 20);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, sfxVol)) * 20);
    }

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }
}