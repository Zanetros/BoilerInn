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
        if (instance == null) instance = this;
        else Destroy(gameObject);
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