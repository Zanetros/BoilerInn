using UnityEngine;
using UnityEngine.Audio;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [Header("Playlist Settings")]
    [Tooltip("If true, the playlist system is active.")]
    public bool isPlaylistPlaying = true; 
    public AudioClip[] playlist;
    
    [Header("Debug")]
    [Tooltip("Shows the currently playing music")]
    public AudioClip currentMusic;

    private List<AudioClip> unplayedTracks = new List<AudioClip>();

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

        if (musicSource != null)
        {
            musicSource.ignoreListenerPause = true;
        }

        LoadVolume();
        StartCoroutine(PlaylistMonitorRoutine());
    }

    private string lastSceneName = "";

    private IEnumerator PlaylistMonitorRoutine()
    {
        lastSceneName = SceneManager.GetActiveScene().name;

        while (true)
        {
            // Checa a cada 0.5 segundos (tempo real). É extremamente leve comparado ao Update!
            yield return new WaitForSecondsRealtime(0.5f);

            string currentScene = SceneManager.GetActiveScene().name;

            // Se a cena mudou...
            if (currentScene != lastSceneName)
            {
                Debug.Log($"[SoundManager] Detectou mudança de cena de {lastSceneName} para {currentScene}!");
                lastSceneName = currentScene;

                if (currentScene == "Menu")
                {
                    StopPlaylist();
                }
                else
                {
                    StartPlaylist();
                }
            }

            if (!isPlaylistPlaying) continue;
            if (currentScene == "Menu") continue;

            if (!musicSource.isPlaying && currentFadeCoroutine == null)
            {
                // Se o jogo for minimizado/Alt+Tab, o Unity pausa o áudio e isPlaying fica false.
                // Mas o 'time' continua maior que 0. Quando a música acaba naturalmente, o 'time' zera.
                if (musicSource.clip == null || musicSource.time == 0f)
                {
                    PlayNextInPlaylist();
                }
            }
        }
    }

    public void StartPlaylist()
    {   
        isPlaylistPlaying = true;
        SoundManager.instance.musicSource.loop = false;
        unplayedTracks = new List<AudioClip>(playlist);
        PlayNextInPlaylist();
    }

    public void StopPlaylist()
    {
        isPlaylistPlaying = false;
    }

    [ContextMenu("Skip to Next Song")]
    public void PlayNextInPlaylist()
    {
        if (playlist == null || playlist.Length == 0)
        {
            Debug.LogWarning("A Playlist está vazia! Adicione músicas no array 'Playlist' no Inspector do SoundManager.");
            isPlaylistPlaying = false;
            return;
        }

        if (unplayedTracks.Count == 0)
        {
            unplayedTracks = new List<AudioClip>(playlist);
        }

        if (unplayedTracks.Count == 0) return;

        int randomIndex = Random.Range(0, unplayedTracks.Count);
        AudioClip nextClip = unplayedTracks[randomIndex];
        unplayedTracks.RemoveAt(randomIndex);

        if (musicSource != null) musicSource.loop = false;

        PlayMusic(nextClip, false);
    }
    
    public void PlayMusic(AudioClip clip, bool stopPlaylist = true)
    {
        if (stopPlaylist)
        {
            if (isPlaylistPlaying) Debug.LogWarning("A Playlist foi DESLIGADA porque uma música específica foi chamada com stopPlaylist = true!");
            isPlaylistPlaying = false;
        }

        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return; 

        currentMusic = clip;
        Debug.Log($"🎵 Tocando música: {clip.name}");

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeMusic(clip));
    }
    
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.volume = 1f;
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
        if (fadeDuration <= 0f)
        {
            musicSource.clip = newClip;
            musicSource.volume = 1f;
            musicSource.Play();
            currentFadeCoroutine = null;
            yield break;
        }

        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * (Time.unscaledDeltaTime / (fadeDuration / 2));
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