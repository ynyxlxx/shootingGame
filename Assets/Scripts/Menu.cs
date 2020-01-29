using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToogles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    int activeResIndex;

    void Start() {
        activeResIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullscreen = (PlayerPrefs.GetInt("fullscreen") == 1) ? true : false;

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumPercent;

        for (int i = 0; i <resolutionToogles.Length; i++) {
            resolutionToogles[i].isOn = i == activeResIndex;
        }

        fullscreenToggle.isOn = isFullscreen;
    }

    public void Play() {
        SceneManager.LoadScene("Game");
    }

    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void MainMenu() {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }

    public void SetScreenResolution(int i) {
        if (resolutionToogles[i].isOn) {
            float aspectRatio = 16 / 9f;
            activeResIndex = i;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen) {
        for (int i = 0; i < resolutionToogles.Length; i++) {
            resolutionToogles[i].interactable = !isFullscreen;
        }
        
        if (isFullscreen) {
            Resolution[] allRes = Screen.resolutions;
            Resolution maxRes = allRes[allRes.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, true);
        } else {
            SetScreenResolution(activeResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value) {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }

}
