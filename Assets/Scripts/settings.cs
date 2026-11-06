using System.Collections.Frozen;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;


//using unity discussions post with edits
public class SettingsMenu : MonoBehaviour
{
    [Header("UI Sliders")]
   
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;



    [Header("Audio Mixer")]
    public AudioMixer audioMixer; // Assign your AudioMixer in Inspector

    // PlayerPrefs keys
  
    private const string MusicKey = "MusicVolume";
    private const string SFXKey = "SFXVolume";


    private void Start()
    {
        // Load saved values or defaults
        float music = PlayerPrefs.GetFloat(MusicKey, 0.75f);
        float sfx = PlayerPrefs.GetFloat(SFXKey, 0.75f);

        // Set sliders
       
        musicSlider.value = music;
        sfxSlider.value = sfx;

        // Apply to mixer
      
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        // Add listeners
       
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    


    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat(SFXKey, value);
        PlayerPrefs.Save();
    }
    


}
