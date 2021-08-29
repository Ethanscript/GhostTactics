using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private AudioSource music;
    private AudioSource sound;

    public const string key_music = "key_music";
    public const string key_sound = "key_sound";

    public static AudioManager Instance;

    public AudioMixer audioMixer;

    private void Awake()
    {
        Instance = this;
        music = transform.Find("Music").GetComponent<AudioSource>();
        sound = transform.Find("Music").GetComponent<AudioSource>();

    }

    public void playMusic(AudioClip clip)
    {
        music.clip = clip;
        music.Play();
    }

    public void playSound(AudioClip clip)
    {
        sound.PlayOneShot(clip);
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void getPlayerDefault()
    {
        //music.volume = PlayerPrefs.GetFloat(key_music, 1.0f);
        //sound.volume = PlayerPrefs.GetFloat(key_sound, 1.0f);
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat(key_music, 0.0f));
        audioMixer.SetFloat("SoundVolume", PlayerPrefs.GetFloat(key_sound, 0.0f));

    }
}
