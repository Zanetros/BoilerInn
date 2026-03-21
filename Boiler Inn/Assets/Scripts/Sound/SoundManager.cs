using UnityEngine;
using UnityEngine.Audio;
using System.Collections; // Necessário para usar Coroutines

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Settings")]
    public float fadeDuration = 1.0f; // Tempo em segundos que a transição vai durar

    private Coroutine currentFadeCoroutine; // Guarda a transição atual para não bugar se trocar muito rápido

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

        LoadVolume();
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return; 

        // Se já estiver acontecendo uma transição, a gente cancela ela para começar a nova
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Inicia a mágica da transição suave
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
            // Abaixa o volume aos poucos
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        sfxSource.Stop();
        sfxSource.volume = startVolume; // Reseta o volume para o próximo som do jogo!
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // 1. FADE OUT (Abaixa a música atual, se estiver tocando)
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                // Diminui o volume aos poucos (fadeDuration / 2 para ser metade do tempo descendo)
                musicSource.volume -= startVolume * Time.deltaTime / (fadeDuration / 2);
                yield return null; 
            }
            musicSource.Stop();
        }

        // 2. TROCA A FAIXA
        musicSource.clip = newClip;
        musicSource.Play();

        // 3. FADE IN (Sobe o volume da música nova até 1)
        while (musicSource.volume < 1f)
        {
            musicSource.volume += Time.deltaTime / (fadeDuration / 2);
            yield return null;
        }

        // Garante que o volume cravou no máximo (o seu Mixer que dita o quão alto isso realmente é)
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