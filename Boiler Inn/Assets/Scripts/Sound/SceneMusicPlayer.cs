using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Música desta Cena")]
    public AudioClip sceneMusic;

    private void Start()
    {
        if (SoundManager.instance != null)
        {
            if (sceneMusic != null)
            {
                SoundManager.instance.PlayMusic(sceneMusic, true);
                SoundManager.instance.musicSource.loop = true;
            }
            else
            {
                SoundManager.instance.StartPlaylist();
                SoundManager.instance.musicSource.loop = false;
                SoundManager.instance.isPlaylistPlaying = true;
            }
        }
        else if (SoundManager.instance == null)
        {
            Debug.LogWarning("SoundManager não encontrado na cena! Lembre-se de começar o jogo pela cena do Menu.");
        }
    }
}