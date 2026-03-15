using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Mixer")]
    public AudioMixer mainMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    void Awake()
    {
        // Sistema Singleton: Se já existir um, destrói o novo. Se não, sobrevive.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // A "Mágica" acontece aqui
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Carrega o volume logo no Awake para não ter delay
        LoadVolume();
    }
    
    public void PlayMusic(AudioClip clip)
    {
        AudioSource source = GetComponentInChildren<AudioSource>();
        if (source.clip == clip) return; // Já está tocando essa música? Não faz nada.
    
        source.clip = clip;
        source.Play();
    }

    public void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        // Mathf.Max evita o log de zero (o bug que você teve)
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(0.0001f, musicVol)) * 20);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(0.0001f, sfxVol)) * 20);
    }

    // Método para tocar um efeito sonoro (ex: acerto de nota)
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // Método para alterar o volume via Slider/UI
    public void SetMusicVolume(float volume)
    {
        // O volume do Mixer usa escala Logarítmica (-80dB a 20dB)
        // O cálculo abaixo converte 0.0001-1.0 para dB
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
    }
}