using UnityEngine;
using UnityEngine.UI;

public class OptionView : ViewBase
{
    public Slider music;
    public Slider sound;


    private void Awake()
    {
        music.value = PlayerPrefs.GetFloat(AudioManager.key_music, 1.0f);
        sound.value = PlayerPrefs.GetFloat(AudioManager.key_sound, 1.0f);
    }


    private void Start()
    {
        hide();
    }


    public void changeMusic(float f)
    {
        PlayerPrefs.SetFloat(AudioManager.key_music, f);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.getPlayerDefault();
        }
    }

    public void changeSound(float f)
    {
        PlayerPrefs.SetFloat(AudioManager.key_sound, f);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.getPlayerDefault();
        }
    }

}
