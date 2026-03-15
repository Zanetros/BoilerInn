using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    [Header("Configurações")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Define o valor inicial dos Sliders (entre 0.0001 e 1)
        // O valor padrão de 0.75 costuma ser um bom começo
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);

        // Aplica os valores logo ao iniciar o jogo
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);

        // Adiciona ouvintes para detectar quando o jogador move o slider
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        float dB;
        if (value > 0.0001f) 
        {
            dB = Mathf.Log10(value) * 20;
        }
        else 
        {
            dB = -80f; // Silêncio total
        }

        mainMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        float dB;
        if (value > 0.0001f) 
        {
            dB = Mathf.Log10(value) * 20;
        }
        else 
        {
            dB = -80f; // Silêncio total
        }

        mainMixer.SetFloat("SFXVol", dB);
        PlayerPrefs.SetFloat("SFXVol", value);
    }
}