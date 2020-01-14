using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum AudioChannel { Master, Sfx, Music };

    float masterVolumePercent = 1f;
    float sfxVolumPercent = 1;
    float musicVolumePercent = 1;

    AudioSource[] musicSources;
    AudioSource sfx2DSource;
    int activeMusicSourceIndex;

    public static AudioManager instance;
    bool isInstantiated = false;


    Transform audioListener;
    Transform playerTransform;

    SoundLibrary library;

    void Awake() {
        //保证只有一个AudioManager的实例
        if (instance != null && isInstantiated) {
            Destroy(gameObject);
        } else {
            isInstantiated = true;
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++) {
                GameObject newMusicSource = new GameObject("Music source" + (1 + i));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2Dsource = new GameObject("2D Sfx source");
            sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>();
            newSfx2Dsource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            playerTransform = FindObjectOfType<Player>().transform;
            library = GetComponent<SoundLibrary>();

            masterVolumePercent = PlayerPrefs.GetFloat("master vol", masterVolumePercent);
            sfxVolumPercent = PlayerPrefs.GetFloat("sfx vol", sfxVolumPercent);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol", musicVolumePercent);
        }
    }

    void Update() {
        if (playerTransform != null) {
            audioListener.position = playerTransform.position;
        }
    }

    //设置音量
    public void SetVolume(float volumePercent, AudioChannel channel) {
        switch (channel) {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumPercent = volumePercent;
                break;
            case AudioChannel.Music:
                sfxVolumPercent = volumePercent;
                break;
        }

        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = musicVolumePercent * masterVolumePercent;

        //保存用户偏好
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumPercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        //在1到0之间改变
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumPercent * masterVolumePercent);
        }
    }

    public void PlaySound(string soundName, Vector3 pos) {
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName) {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumPercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration) {
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercent * musicVolumePercent, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercent * musicVolumePercent, 0, percent);
            yield return null;
        }
    }
}
