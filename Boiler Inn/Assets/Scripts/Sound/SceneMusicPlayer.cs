using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Música desta Cena")]
    public AudioClip sceneMusic;

    private void Start()
    {
        // Assim que a cena carregar, pede para o SoundManager tocar a música
        if (SoundManager.instance != null && sceneMusic != null)
        {
            SoundManager.instance.PlayMusic(sceneMusic);
        }
        else if (SoundManager.instance == null)
        {
            Debug.LogWarning("SoundManager não encontrado na cena! Lembre-se de começar o jogo pela cena do Menu.");
        }
    }
}